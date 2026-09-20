using Microsoft.EntityFrameworkCore;
using Spleet.Models;
using Spleet.Repositories.Interfaces;
using Spleet.Data;

namespace Spleet.Repositories
{
    public class SettlementRepository : Repository<Settlement>, ISettlementRepository
    {
        public SettlementRepository(SpleetDbContext context) : base(context) { }

        public async Task<Settlement> MarkAsPaid(Guid groupId, Guid payerId, Guid payeeId, decimal amount, string? note)
        {
            var settlement = new Settlement
            {
                GroupId = groupId,
                PayerUserId = payerId,
                PayeeUserId = payeeId,
                Amount = amount,
                Note = note,
                Status = SettlementStatus.Pending,
                AutoConfirmDeadline = DateTime.UtcNow.AddHours(24)
            };
            await _context.Settlements.AddAsync(settlement);
            return settlement;
        }

        public async Task ConfirmSettlement(Guid settlementId)
        {
            var settlement = await GetById(settlementId);
            if (settlement != null)
            {
                settlement.Status = SettlementStatus.Completed;
                settlement.ConfirmedAt = DateTime.UtcNow;
            }
        }

        public async Task DisputeSettlement(Guid settlementId)
        {
            var settlement = await GetById(settlementId);
            if (settlement != null)
            {
                settlement.Status = SettlementStatus.Disputed;
            }
        }

        public async Task<IEnumerable<Settlement>> GetPendingForGroup(Guid groupId)
        {
            return await _context.Settlements
                .Where(s => s.GroupId == groupId && s.Status == SettlementStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetPendingForUser(Guid userId)
        {
            return await _context.Settlements
                .Where(s => s.PayeeUserId == userId && s.Status == SettlementStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetOverdueForAutoConfirm()
        {
            return await _context.Settlements
                .Where(s => s.Status == SettlementStatus.Pending && s.AutoConfirmDeadline < DateTime.UtcNow)
                .ToListAsync();
        }
    }
}