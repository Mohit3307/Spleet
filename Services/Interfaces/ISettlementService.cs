using Spleet.Models;
using Spleet.ViewModels;

namespace Spleet.Services.Interfaces
{
    public interface ISettlementService
    {
        Task<bool> IsMemberAsync(Guid groupId, Guid userId);
        Task<Group?> GetGroupAsync(Guid groupId);
        Task<Settlement?> GetAsync(Guid settlementId);
        Task<SettlementViewModel> GetSettleUpDataAsync(Guid groupId, Guid currentUserId);
        Task<Guid> MarkPaidAsync(Guid groupId, Guid payerId, Guid payeeId, decimal amount, string? note);
        Task ConfirmAsync(Guid settlementId, Guid currentUserId);
        Task DisputeAsync(Guid settlementId, Guid currentUserId);
        Task AutoConfirmOverdueAsync();
    }
}
