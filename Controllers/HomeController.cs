using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spleet.Models;
using Spleet.Repositories.Interfaces;

namespace Spleet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IGroupRepository _groupRepository;
        private readonly IExpenseRepository _expenseRepository;

        public HomeController(
            UserManager<User> userManager,
            IGroupRepository groupRepository,
            IExpenseRepository expenseRepository)
        {
            _userManager = userManager;
            _groupRepository = groupRepository;
            _expenseRepository = expenseRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var groups =
                await _groupRepository.GetGroupsForUserAsync(user.Id);

            var recentExpenses = new List<Expense>();

            foreach (var group in groups)
            {
                var groupExpenses =
                    await _expenseRepository.GetRecentForGroup(group.Id, 5);

                recentExpenses.AddRange(groupExpenses);
            }

            recentExpenses = recentExpenses
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.User = user;
            ViewBag.RecentExpenses = recentExpenses;

            return View(groups);
        }
    }
}