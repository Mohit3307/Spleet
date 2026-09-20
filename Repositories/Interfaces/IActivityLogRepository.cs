using Spleet.Models;

namespace Spleet.Repositories.Interfaces
{
    public interface IActivityLogRepository : IRepository<ActivityLogEntry>
    {
        Task<IEnumerable<ActivityLogEntry>> GetRecentForGroup(Guid groupId, int count = 20);
        Task LogActivity(Guid groupId, Guid actorUserId, ActivityType type, string summary, Guid? relatedEntityId);
    }
}
