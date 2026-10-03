using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly SpleetDbContext _context;

        public GroupRepository(SpleetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Group>> GetGroupsForUserAsync(Guid userId)
        {
            return await _context.Groups
                .Where(g => !g.IsArchived && g.Members.Any(m => m.UserId == userId && m.IsActive))
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<Group?> GetByIdAsync(Guid groupId)
        {
            return await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<Group?> GetByIdWithMembersAsync(Guid groupId)
        {
            return await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task AddAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserMemberAsync(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
        }
    }
}
