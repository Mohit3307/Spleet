using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IGroupInviteRepository : IRepository<GroupInvite>
    {
        Task<GroupInvite> CreateInviteLink(Guid groupId, Guid createdByUserId);
        Task DiscardInviteLink(Guid inviteId);
        Task<GroupInvite?> GetByToken(string token);
        Task<IEnumerable<GroupInvite>> GetActiveInvitesForGroup(Guid groupId);
    }
}
