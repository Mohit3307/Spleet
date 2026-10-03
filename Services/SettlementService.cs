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

        public async Task<Settlement?> GetAsync(Guid settlementId) =>
            await _context.Settlements
                .Include(s => s.Payer)
                .Include(s => s.Payee)
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == settlementId);

        public async Task<SettlementViewModel> GetSettleUpDataAsync(Guid groupId, Guid currentUserId)
        {
            await AutoConfirmOverdueAsync();

            var suggestions = await _debtService.GetSuggestionsAsync(groupId);
            var pending = await _context.Settlements
                .Include(s => s.Payer)
                .Include(s => s.Payee)
                .Where(s => s.GroupId == groupId && s.Status == SettlementStatus.Pending)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return new SettlementViewModel
            {
                GroupId = groupId,
                CurrentUserId = currentUserId,
                Suggestions = suggestions,
                PendingSettlements = pending
            };
        }

        public async Task<Guid> MarkPaidAsync(
            Guid groupId,
            Guid payerId,
            Guid payeeId,
            decimal amount,
            string? note)
        {
            if (payerId == payeeId)
                throw new ArgumentException("Payer and payee must be different.");

            if (amount <= 0)
                throw new ArgumentException("Settlement amount must be greater than zero.");

            if (!await IsMemberAsync(groupId, payerId) || !await IsMemberAsync(groupId, payeeId))
                throw new ArgumentException("Both users must be active members of the group.");

            var settlement = await _settlementRepository.MarkAsPaid(
                groupId, payerId, payeeId, decimal.Round(amount, 2), note);

            await _context.SaveChangesAsync();
            return settlement.Id;
        }

        public async Task ConfirmAsync(Guid settlementId, Guid currentUserId)
        {
            await AutoConfirmOverdueAsync();

            var settlement = await _settlementRepository.GetById(settlementId);
            if (settlement == null)
                throw new InvalidOperationException("Settlement not found.");

            if (settlement.Status != SettlementStatus.Pending)
                throw new InvalidOperationException("This settlement is no longer pending.");

            if (settlement.PayeeUserId != currentUserId)
                throw new UnauthorizedAccessException();

            await _settlementRepository.ConfirmSettlement(settlementId);
            await _context.SaveChangesAsync();
        }

        public async Task DisputeAsync(Guid settlementId, Guid currentUserId)
        {
            await AutoConfirmOverdueAsync();

            var settlement = await _settlementRepository.GetById(settlementId);
            if (settlement == null)
                throw new InvalidOperationException("Settlement not found.");

            if (settlement.Status != SettlementStatus.Pending)
                throw new InvalidOperationException("This settlement is no longer pending.");

            if (settlement.PayeeUserId != currentUserId)
                throw new UnauthorizedAccessException();

            await _settlementRepository.DisputeSettlement(settlementId);
            await _context.SaveChangesAsync();
        }

        public async Task AutoConfirmOverdueAsync()
        {
            var overdue = await _settlementRepository.GetOverdueForAutoConfirm();
            var changed = false;

            foreach (var settlement in overdue)
            {
                settlement.Status = SettlementStatus.Completed;
                settlement.ConfirmedAt = DateTime.UtcNow;
                changed = true;
            }

            if (changed)
                await _context.SaveChangesAsync();
        }
    }
}
