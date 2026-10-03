using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly IGroupInviteRepository _inviteRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;

        public InvitesController(
            IGroupInviteRepository inviteRepository,
            IGroupRepository groupRepository,
            IGroupMemberRepository groupMemberRepository,
            IUserRepository userRepository,
            UserManager<User> userManager)
        {
            _inviteRepository = inviteRepository;
            _groupRepository = groupRepository;
            _groupMemberRepository = groupMemberRepository;
            _userRepository = userRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid groupId)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);

            if (group == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Only group members can create invites.
            if (!await _groupRepository.IsUserMemberAsync(groupId, user.Id))
                return Forbid();

            var invite = await _inviteRepository.CreateInviteLink(
                groupId,
                user.Id);

            return RedirectToAction(
                nameof(Preview),
                new { token = invite.Token });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Preview(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest();

            var invite = await _inviteRepository.GetByToken(token);

            if (invite == null)
                return NotFound();

            if (invite.IsRevoked)
                return NotFound();

            if (invite.ExpiresAt < DateTime.UtcNow)
                return NotFound();

            return View(invite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string token)
        {
            var invite = await _inviteRepository.GetByToken(token);

            if (invite == null)
                return NotFound();

            if (invite.IsRevoked)
                return NotFound();

            if (invite.ExpiresAt < DateTime.UtcNow)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var existingMember =
                await _groupMemberRepository.GetMembershipAsync(
                    invite.GroupId,
                    user.Id);

            if (existingMember != null)
            {
                return RedirectToAction(
                    "Details",
                    "Groups",
                    new { id = invite.GroupId });
            }

            var member = new GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = invite.GroupId,
                UserId = user.Id,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _groupMemberRepository.AddAsync(member);

            return RedirectToAction(
                "Details",
                "Groups",
                new { id = invite.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddByEmail(
            Guid groupId,
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();

            // Only existing group members can add another member.
            if (!await _groupRepository.IsUserMemberAsync(
                    groupId,
                    currentUser.Id))
            {
                return Forbid();
            }

            var user = await _userRepository.GetByEmail(email);

            if (user == null)
                return NotFound();

            var existingMember =
                await _groupMemberRepository.GetMembershipAsync(
                    groupId,
                    user.Id);

            if (existingMember != null)
            {
                return RedirectToAction(
                    "Details",
                    "Groups",
                    new { id = groupId });
            }

            var member = new GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                UserId = user.Id,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _groupMemberRepository.AddAsync(member);

            return RedirectToAction(
                "Details",
                "Groups",
                new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(
            Guid inviteId,
            Guid groupId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var membership =
                await _groupMemberRepository.GetMembershipAsync(
                    groupId,
                    user.Id);

            if (membership == null ||
                membership.Role != GroupRole.Admin)
            {
                return Forbid();
            }

            await _inviteRepository.DiscardInviteLink(inviteId);

            return RedirectToAction(
                "Details",
                "Groups",
                new { id = groupId });
        }
    }
}