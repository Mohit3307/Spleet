using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spleet.Models;
using Spleet.Services.Interfaces;
using Spleet.ViewModels;

namespace Spleet.Controllers
{
    [Authorize]
    public class SettlementsController : Controller
    {
        private readonly ISettlementService _settlementService;
        private readonly UserManager<User> _userManager;

        public SettlementsController(
            ISettlementService settlementService,
            UserManager<User> userManager)
        {
            _settlementService = settlementService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(Guid groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await _settlementService.IsMemberAsync(groupId, user.Id))
                return Forbid();

            var group = await _settlementService.GetGroupAsync(groupId);
            if (group == null) return NotFound();

            var model = await _settlementService.GetSettleUpDataAsync(groupId, user.Id);
            ViewBag.Group = group;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var settlement = await _settlementService.GetAsync(id);
            if (settlement == null) return NotFound();

            if (!await _settlementService.IsMemberAsync(settlement.GroupId, user.Id))
                return Forbid();

            await _settlementService.AutoConfirmOverdueAsync();
            settlement = await _settlementService.GetAsync(id);

            return View(settlement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(
            Guid groupId,
            Guid payeeUserId,
            decimal amount,
            string? note)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await _settlementService.IsMemberAsync(groupId, user.Id))
                return Forbid();

            try
            {
                var id = await _settlementService.MarkPaidAsync(
                    groupId,
                    user.Id,
                    payeeUserId,
                    amount,
                    note);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { groupId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            try
            {
                await _settlementService.ConfirmAsync(id, user.Id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispute(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            try
            {
                await _settlementService.DisputeAsync(id, user.Id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
