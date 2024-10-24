using CourseProject.Areas.Identity.Data;
using CourseProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CourseProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public UsersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessSelected(List<string> selectedUsers, string actionType, string selectedRole)
        {
            if (selectedUsers == null || !selectedUsers.Any())
            {
                TempData["Message"] = "Please select at least one user.";
                return RedirectToAction("Index");
            }

            if (actionType == "AssignRole" && selectedRole == null)
            {
                TempData["Message"] = "Please select a role to assign.";
                return RedirectToAction("Index");
            }

            var users = await userManager.Users.Where(u => selectedUsers.Contains(u.Id)).ToListAsync();

            await ExecuteAction(users, actionType, selectedRole);

            TempData["Message"] = GetTempDataMessage(users.Count(), actionType);

            return RedirectToAction("Index");
        }

        public async Task ExecuteAction(List<ApplicationUser> users, string actionType, string selectedRole)
        {
            switch (actionType)
            {
                case "Block":
                    await Block(users);
                    break;

                case "Unblock":
                    await Unblock(users);
                    break;

                case "AssignRole":
                    await AssignRole(users, selectedRole);
                    break;

                case "Delete":
                    await Delete(users);
                    break;

                default:
                    break;
            }
        }

        public string GetTempDataMessage(int usersCount, string actionType)
        {
            var actionMessages = new Dictionary<string, Func<int, string>>
            {
                { "Block", count => count == 1 ? "User has been blocked." : "Users have been blocked." },
                { "Unblock", count => count == 1 ? "User has been unblocked." : "Users have been unblocked." },
                { "Delete", count => count == 1 ? "User has been deleted." : "Users have been deleted." },
                { "AssignRole", count => count == 1 ? "Role assigned." : "Roles assigned." }
            };

            if (actionMessages.TryGetValue(actionType, out var getMessage))
            {
                return getMessage(usersCount);
            }

            return "Unknown action.";
        }

        public async Task Block(List<ApplicationUser> users)
        {
            foreach (var user in users)
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(Constants.DefaultYearsToLockout);
                await userManager.UpdateAsync(user);
                await userManager.UpdateSecurityStampAsync(user);
            }
        }

        public async Task Unblock(List<ApplicationUser> users)
        {
            foreach (var user in users)
            {
                user.LockoutEnd = null;
                await userManager.UpdateAsync(user);
            }
        }

        public async Task AssignRole(List<ApplicationUser> users, string selectedRole)
        {
            foreach (var user in users)
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, selectedRole);
            }
        }

        public async Task Delete(List<ApplicationUser> users)
        {
            foreach (var user in users)
            {
                await userManager.DeleteAsync(user);
            }
        }
    }
}
