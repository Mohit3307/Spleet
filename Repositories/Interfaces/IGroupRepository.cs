using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetGroupsForUserAsync(Guid userId);
        Task<Group?> GetByIdAsync(Guid groupId);
        Task<Group?> GetByIdWithMembersAsync(Guid groupId);
        Task AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task<bool> IsUserMemberAsync(Guid groupId, Guid userId);
    }
}
