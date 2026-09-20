using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class GroupMemberRepository : Repository<GroupMember>, IGroupMemberRepository
    {
        public GroupMemberRepository(SpleetDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<GroupMember>> GetMembersByGroup(Guid groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.IsActive)
                .Include(gm => gm.User)
                .ToListAsync();
        }

        public async Task<GroupMember?> GetMembership(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers
                .FirstOrDefaultAsync(gm =>
                    gm.GroupId == groupId &&
                    gm.UserId == userId);
        }

        public async Task<bool> IsMember(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm =>
                    gm.GroupId == groupId &&
                    gm.UserId == userId);
        }

        public async Task UpdateBalance(Guid groupId, Guid userId, decimal newBalance)
        {
            var member = await GetMembership(groupId, userId);
            if (member != null)
            {
                member.CurrentBalance = newBalance;
            }
        }
    }
}
