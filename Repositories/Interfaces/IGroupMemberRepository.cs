using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IGroupMemberRepository : IRepository<GroupMember>
    {
        Task<IEnumerable<GroupMember>> GetMembersByGroup(Guid groupId);
        Task<GroupMember?> GetMembership(Guid groupId, Guid userId);
        Task<bool> IsMember(Guid groupId, Guid userId);
        Task UpdateBalance(Guid groupId, Guid userId, decimal newBalance);
    }
}
