using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Repositories
{
    public class GroupInviteRepository : Repository<GroupInvite>, IGroupInviteRepository
    {
        public GroupInviteRepository(SpleetDbContext context) : base(context) { }

        public async Task<GroupInvite> CreateInviteLink(Guid groupId, Guid createdByUserId)
        {
            var invite = new GroupInvite
            {
                GroupId = groupId,
                CreatedByUserId = createdByUserId
            };
            await _context.GroupInvites.AddAsync(invite);
            return invite;
        }

        public async Task DiscardInviteLink(Guid inviteId)
        {
            var invite = await GetById(inviteId);
            if (invite != null)
            {
                invite.IsRevoked = true;
            }
        }

        public async Task<GroupInvite?> GetByToken(string token)
        {
            return await _context.GroupInvites.FirstOrDefaultAsync(i => i.Token == token);
        }

        public async Task<IEnumerable<GroupInvite>> GetActiveInvitesForGroup(Guid groupId)
        {
            return await _context.GroupInvites
                .Where(i => i.GroupId == groupId && !i.IsRevoked && i.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
