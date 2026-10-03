using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IGroupMemberRepository
    {
        Task AddAsync(GroupMember member);
        Task<GroupMember?> GetMembershipAsync(Guid groupId, Guid userId);
        Task<List<GroupMember>> GetMembersAsync(Guid groupId);
    }
}
