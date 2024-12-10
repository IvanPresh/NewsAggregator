using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.ViewModels; // Ensure you have the appropriate using directive
using NewsAggregator.Web.Models.Domain;
using NewsAggregator.Web.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAggregator.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperAdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Action method to list all users with their roles
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModels = new List<ManageUserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModels.Add(new ManageUserRolesViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return View(userRolesViewModels);
        }

        // Example action method to get user details along with roles
        public async Task<IActionResult> ManageUserRoles(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(); // Handle user not found
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ManageUserRolesViewModel
            {
                User = user,
                Roles = roles.ToList()
            };

            return View(model);
        }
    }
}
