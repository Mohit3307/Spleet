using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(SpleetDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Group>> GetGroupsByUser(Guid userId)
        {
            return await _context.Groups
                .Where(g => _context.GroupMembers
                    .Any(gm => gm.GroupId == g.Id && gm.UserId == userId && gm.IsActive))
                .ToListAsync();
        }

        public async Task<Group?> GetGroupWithMembers(Guid groupId)
        {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }
    }
}
