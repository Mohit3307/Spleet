using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly SpleetDbContext _context;

        public GroupMemberRepository(SpleetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(GroupMember member)
        {
            await _context.GroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task<GroupMember?> GetMembershipAsync(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
        }

        public async Task<List<GroupMember>> GetMembersAsync(Guid groupId)
        {
            return await _context.GroupMembers
                .Include(m => m.User)
                .Where(m => m.GroupId == groupId && m.IsActive)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();
        }
    }
}
