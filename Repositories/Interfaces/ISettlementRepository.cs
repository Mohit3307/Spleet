using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface ISettlementRepository : IRepository<Settlement>
    {
        Task<Settlement> MarkAsPaid(Guid groupId, Guid payerId, Guid payeeId, decimal amount, string? note);
        Task ConfirmSettlement(Guid settlementId);
        Task DisputeSettlement(Guid settlementId);
        Task<IEnumerable<Settlement>> GetPendingForGroup(Guid groupId);
        Task<IEnumerable<Settlement>> GetPendingForUser(Guid userId);
        Task<IEnumerable<Settlement>> GetOverdueForAutoConfirm();
    }
}