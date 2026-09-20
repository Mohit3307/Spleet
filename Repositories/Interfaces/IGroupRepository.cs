using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<IEnumerable<Group>> GetGroupsByUser(Guid userId);
        Task<Group?> GetGroupWithMembers(Guid groupId);
    }
}
