
using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class SettlementRepository : Repository<Settlement>, ISettlementRepository
    {
        public SettlementRepository(SpleetDbContext context) : base(context)
        {
        }

        public async Task<Settlement> MarkAsPaid(
            Guid groupId,
            Guid payerId,
            Guid payeeId,
            decimal amount,
            string? note)
        {
            var settlement = new Settlement
            {
                GroupId = groupId,
                PayerUserId = payerId,
                PayeeUserId = payeeId,
                Amount = decimal.Round(amount, 2),
                Note = note,
                Status = SettlementStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                AutoConfirmDeadline = DateTime.UtcNow.AddHours(24)
            };

            await _context.Settlements.AddAsync(settlement);
            return settlement;
        }

        public async Task ConfirmSettlement(Guid settlementId)
        {
            var settlement = await _context.Settlements
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
                throw new InvalidOperationException("Settlement not found.");

            settlement.Status = SettlementStatus.Completed;
            settlement.ConfirmedAt = DateTime.UtcNow;
        }

        public async Task DisputeSettlement(Guid settlementId)
        {
            var settlement = await _context.Settlements
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
                throw new InvalidOperationException("Settlement not found.");

            settlement.Status = SettlementStatus.Disputed;
        }

        public async Task<IEnumerable<Settlement>> GetPendingForGroup(Guid groupId)
        {
            return await _context.Settlements
                .Where(s => s.GroupId == groupId &&
                            s.Status == SettlementStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetPendingForUser(Guid userId)
        {
            return await _context.Settlements
                .Where(s => s.PayeeUserId == userId &&
                            s.Status == SettlementStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetOverdueForAutoConfirm()
        {
            var now = DateTime.UtcNow;

            return await _context.Settlements
                .Where(s => s.Status == SettlementStatus.Pending &&
                            s.AutoConfirmDeadline != null &&
                            s.AutoConfirmDeadline <= now)
                .ToListAsync();
        }
    }
}
