using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class ActivityLogRepository : Repository<ActivityLogEntry>, IActivityLogRepository
    {
        public ActivityLogRepository(SpleetDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ActivityLogEntry>> GetRecentForGroup(
            Guid groupId,
            int count = 20)
        {
            return await _context.ActivityLogEntries
                .Where(a => a.GroupId == groupId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task LogActivity(Guid groupId, Guid actorUserId, ActivityType type, string summary, Guid? relatedEntityId)
        {
            var entry = new ActivityLogEntry
            {
                GroupId = groupId,
                ActorUserId = actorUserId,
                Type = type,
                Summary = summary,
                RelatedEntityId = relatedEntityId
            };
            await _context.ActivityLogEntries.AddAsync(entry);
        }
    }
}
