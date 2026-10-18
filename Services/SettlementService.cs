
using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;
using Spleet.Services.Interfaces;
using Spleet.ViewModels;

namespace Spleet.Services
{
    public class SettlementService : ISettlementService
    {
        private readonly SpleetDbContext _context;
        private readonly ISettlementRepository _settlementRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IDebtSimplificationService _debtService;

        public SettlementService(
            SpleetDbContext context,
            ISettlementRepository settlementRepository,
            IGroupRepository groupRepository,
            IDebtSimplificationService debtService)
        {
            _context = context;
            _settlementRepository = settlementRepository;
            _groupRepository = groupRepository;
            _debtService = debtService;
        }

        public Task<bool> IsMemberAsync(Guid groupId, Guid userId) =>
            _groupRepository.IsUserMemberAsync(groupId, userId);

        public Task<Group?> GetGroupAsync(Guid groupId) =>
            _groupRepository.GetByIdWithMembersAsync(groupId);

        public async Task<Settlement?> GetAsync(Guid settlementId)
        {
            return await _context.Settlements
                .Include(s => s.Payer)
                .Include(s => s.Payee)
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == settlementId);
        }

        public async Task<SettlementViewModel> GetSettleUpDataAsync(
            Guid groupId,
            Guid currentUserId)
        {
            await AutoConfirmOverdueAsync();

            var suggestions =
                await _debtService.GetSuggestionsAsync(groupId);

            var pending = await _context.Settlements
                .Include(s => s.Payer)
                .Include(s => s.Payee)
                .Where(s =>
                    s.GroupId == groupId &&
                    s.Status == SettlementStatus.Pending)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var history = await _context.Settlements
                .Include(s => s.Payer)
                .Include(s => s.Payee)
                .Where(s =>
                    s.GroupId == groupId &&
                    (s.Status == SettlementStatus.Completed ||
                     s.Status == SettlementStatus.Disputed))
                .OrderByDescending(s => s.ConfirmedAt ?? s.CreatedAt)
                .ToListAsync();

            return new SettlementViewModel
            {
                GroupId = groupId,
                CurrentUserId = currentUserId,
                Suggestions = suggestions,
                PendingSettlements = pending,
                SettlementHistory = history
            };
        }

        public async Task<Guid> MarkPaidAsync(
            Guid groupId,
            Guid payerId,
            Guid payeeId,
            decimal amount,
            string? note)
        {
            await AutoConfirmOverdueAsync();

            if (payerId == payeeId)
            {
                throw new ArgumentException(
                    "Payer and payee must be different.");
            }

            amount = decimal.Round(amount, 2);

            if (amount <= 0)
            {
                throw new ArgumentException(
                    "Settlement amount must be greater than zero.");
            }

            if (!await IsMemberAsync(groupId, payerId) ||
                !await IsMemberAsync(groupId, payeeId))
            {
                throw new ArgumentException(
                    "Both users must be active members of the group.");
            }

            if (note?.Length > 200)
            {
                throw new ArgumentException(
                    "The settlement note cannot exceed 200 characters.");
            }

            // Prevent duplicate or excessive pending payments.
            var existingPendingAmount = await _context.Settlements
                .Where(s =>
                    s.GroupId == groupId &&
                    s.PayerUserId == payerId &&
                    s.PayeeUserId == payeeId &&
                    s.Status == SettlementStatus.Pending)
                .SumAsync(s => (decimal?)s.Amount) ?? 0m;

            // Use the current debt suggestions, which already account
            // for completed settlements.
            var suggestions =
                await _debtService.GetSuggestionsAsync(groupId);

            var suggestion = suggestions.FirstOrDefault(s =>
                s.PayerUserId == payerId &&
                s.PayeeUserId == payeeId);

            if (suggestion == null)
            {
                throw new ArgumentException(
                    "There is no outstanding suggested debt between these users.");
            }

            var remainingAmount =
                decimal.Round(suggestion.Amount - existingPendingAmount, 2);

            if (remainingAmount <= 0)
            {
                throw new ArgumentException(
                    "There is already a pending payment covering this debt.");
            }

            if (amount > remainingAmount)
            {
                throw new ArgumentException(
                    $"The payment exceeds the remaining suggested debt of {remainingAmount:0.00}.");
            }

            var settlement = await _settlementRepository.MarkAsPaid(
                groupId,
                payerId,
                payeeId,
                amount,
                note?.Trim());

            await _context.SaveChangesAsync();

            return settlement.Id;
        }

        public async Task ConfirmAsync(
            Guid settlementId,
            Guid currentUserId)
        {
            await AutoConfirmOverdueAsync();

            var settlement =
                await _settlementRepository.GetById(settlementId);

            if (settlement == null)
            {
                throw new InvalidOperationException(
                    "Settlement not found.");
            }

            if (settlement.Status != SettlementStatus.Pending)
            {
                throw new InvalidOperationException(
                    "This settlement is no longer pending.");
            }

            if (settlement.PayeeUserId != currentUserId)
            {
                throw new UnauthorizedAccessException();
            }

            await _settlementRepository.ConfirmSettlement(settlementId);
            await _context.SaveChangesAsync();
        }

        public async Task DisputeAsync(
            Guid settlementId,
            Guid currentUserId)
        {
            await AutoConfirmOverdueAsync();

            var settlement =
                await _settlementRepository.GetById(settlementId);

            if (settlement == null)
            {
                throw new InvalidOperationException(
                    "Settlement not found.");
            }

            if (settlement.Status != SettlementStatus.Pending)
            {
                throw new InvalidOperationException(
                    "This settlement is no longer pending.");
            }

            if (settlement.PayeeUserId != currentUserId)
            {
                throw new UnauthorizedAccessException();
            }

            await _settlementRepository.DisputeSettlement(settlementId);
            await _context.SaveChangesAsync();
        }

        public async Task AutoConfirmOverdueAsync()
        {
            var overdue =
                await _settlementRepository.GetOverdueForAutoConfirm();

            var changed = false;

            foreach (var settlement in overdue)
            {
                settlement.Status = SettlementStatus.Completed;
                settlement.ConfirmedAt = DateTime.UtcNow;
                changed = true;
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
