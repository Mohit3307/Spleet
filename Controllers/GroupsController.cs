using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spleet.Models;
using Spleet.Repositories.Interfaces;
using Spleet.Services.Interfaces;

namespace Spleet.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly UserManager<User> _userManager;
        private readonly IDebtSimplificationService _debtSimplificationService;

        public GroupsController(
            IGroupRepository groupRepository,
            IGroupMemberRepository groupMemberRepository,
            IExpenseRepository expenseRepository,
            UserManager<User> userManager,
            IDebtSimplificationService debtSimplificationService)
        {
            _groupRepository = groupRepository;
            _groupMemberRepository = groupMemberRepository;
            _expenseRepository = expenseRepository;
            _userManager = userManager;
            _debtSimplificationService = debtSimplificationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var groups =
                await _groupRepository.GetGroupsForUserAsync(user.Id);

            return View(groups);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var group =
                await _groupRepository.GetByIdWithMembersAsync(id);

            if (group == null)
                return NotFound();

            if (!await _groupRepository.IsUserMemberAsync(id, user.Id))
                return Forbid();

            var recentExpenses =
                await _expenseRepository.GetRecentForGroup(id, 5);

            var balances =
                await _debtSimplificationService.GetBalancesAsync(id);

            ViewBag.RecentExpenses = recentExpenses;
            ViewBag.Balances = balances;

            return View(group);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name,
            string? description,
            string? groupType,
            string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "name",
                    "Group name is required.");

                return View();
            }

            if (name.Trim().Length > 100)
            {
                ModelState.AddModelError(
                    "name",
                    "Group name cannot exceed 100 characters.");

                return View();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var group = new Group
            {
                Id = Guid.NewGuid(),

                Name = name.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(description)
                        ? null
                        : description.Trim(),

                GroupType =
                    string.IsNullOrWhiteSpace(groupType)
                        ? "other"
                        : groupType.Trim(),

                CreatedByUserId = user.Id,

                CurrencyCode =
                    string.IsNullOrWhiteSpace(currencyCode)
                        ? "INR"
                        : currencyCode.Trim().ToUpperInvariant(),

                IsArchived = false,

                CreatedAt = DateTime.UtcNow
            };

            await _groupRepository.AddAsync(group);

            var membership = new GroupMember
            {
                Id = Guid.NewGuid(),

                GroupId = group.Id,

                UserId = user.Id,

                Role = GroupRole.Admin,

                JoinedAt = DateTime.UtcNow,

                IsActive = true,

                LastUsedSplitType = SplitType.Equal,

                CurrentBalance = 0m
            };

            await _groupMemberRepository.AddAsync(membership);

            return RedirectToAction(
                nameof(Details),
                new { id = group.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var membership =
                await _groupMemberRepository
                    .GetMembershipAsync(id, user.Id);

            if (membership == null ||
                membership.Role != GroupRole.Admin)
            {
                return Forbid();
            }

            var group =
                await _groupRepository.GetByIdAsync(id);

            if (group == null)
                return NotFound();

            group.IsArchived = true;

            await _groupRepository.UpdateAsync(group);

            return RedirectToAction(nameof(Index));
        }
    }
}