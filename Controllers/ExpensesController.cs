using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spleet.Models;
using Spleet.Services.Interfaces;
using Spleet.ViewModels;

namespace Spleet.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly UserManager<User> _userManager;

        public ExpensesController(
            IExpenseService expenseService,
            UserManager<User> userManager)
        {
            _expenseService = expenseService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(Guid groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await _expenseService.IsMemberAsync(groupId, user.Id))
                return Forbid();

            var expenses = await _expenseService.GetGroupExpensesAsync(groupId);
            var group = await _expenseService.GetGroupAsync(groupId);

            if (group == null) return NotFound();

            ViewBag.Group = group;
            return View(expenses);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var data = await _expenseService.GetCreateDataAsync(groupId, user.Id);
            if (data == null) return Forbid();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var data = await _expenseService.GetCreateDataAsync(model.GroupId, user.Id);
                if (data == null) return Forbid();
                data.Description = model.Description;
                data.Amount = model.Amount;
                data.Category = model.Category;
                data.PaidByUserId = model.PaidByUserId;
                data.SplitType = model.SplitType;
                data.ExpenseDate = model.ExpenseDate;
                data.SelectedUserIds = model.SelectedUserIds ?? new List<Guid>();
                data.CustomShares = model.CustomShares ?? new Dictionary<Guid, decimal>();
                return View(data);
            }

            try
            {
                var expenseId = await _expenseService.CreateAsync(model, user.Id);
                return RedirectToAction(nameof(Details), new { id = expenseId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var data = await _expenseService.GetCreateDataAsync(model.GroupId, user.Id);
                if (data == null) return Forbid();
                data.Description = model.Description;
                data.Amount = model.Amount;
                data.Category = model.Category;
                data.PaidByUserId = model.PaidByUserId;
                data.SplitType = model.SplitType;
                data.ExpenseDate = model.ExpenseDate;
                data.SelectedUserIds = model.SelectedUserIds ?? new List<Guid>();
                data.CustomShares = model.CustomShares ?? new Dictionary<Guid, decimal>();
                return View(data);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var expense = await _expenseService.GetDetailsAsync(id);
            if (expense == null) return NotFound();

            if (!await _expenseService.IsMemberAsync(expense.GroupId, user.Id))
                return Forbid();

            return View(expense);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var data = await _expenseService.GetEditDataAsync(id, user.Id);
            if (data == null) return NotFound();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExpenseEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var data = await _expenseService.GetEditDataAsync(model.Id, user.Id);
                if (data == null) return NotFound();
                CopyEditValues(data, model);
                return View(data);
            }

            try
            {
                await _expenseService.UpdateAsync(model, user.Id);
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var data = await _expenseService.GetEditDataAsync(model.Id, user.Id);
                if (data == null) return NotFound();
                CopyEditValues(data, model);
                return View(data);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var groupId = await _expenseService.GetGroupIdAsync(id);
            if (groupId == null) return NotFound();

            if (!await _expenseService.IsMemberAsync(groupId.Value, user.Id))
                return Forbid();

            await _expenseService.SoftDeleteAsync(id, user.Id);
            return RedirectToAction(nameof(Index), new { groupId });
        }

        private static void CopyEditValues(ExpenseEditViewModel target, ExpenseEditViewModel source)
        {
            target.Description = source.Description;
            target.Amount = source.Amount;
            target.Category = source.Category;
            target.PaidByUserId = source.PaidByUserId;
            target.SplitType = source.SplitType;
            target.ExpenseDate = source.ExpenseDate;
            target.SelectedUserIds = source.SelectedUserIds ?? new List<Guid>();
            target.CustomShares = source.CustomShares ?? new Dictionary<Guid, decimal>();
        }
    }
}
