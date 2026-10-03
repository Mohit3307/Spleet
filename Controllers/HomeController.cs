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

        public HomeController(UserManager<User> userManager, IGroupRepository groupRepository)
        {
            _userManager = userManager;
            _groupRepository = groupRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var groups = await _groupRepository.GetGroupsForUserAsync(user.Id);
            ViewBag.User = user;
            return View(groups);
        }
    }
}
