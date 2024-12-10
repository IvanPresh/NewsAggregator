using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.ViewModels;
using NewsAggregator.Web.Models.Domain;
using System.Security.Claims;

namespace NewsAggregator.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var applicationUser = new ApplicationUser
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email
                };

                var applicationUserResult = await _userManager.CreateAsync(applicationUser, registerViewModel.Password);

                if (applicationUserResult.Succeeded)
                {
                    // Assign the default user role
                    var roleApplicationUser = await _userManager.AddToRoleAsync(applicationUser, "user");

                    if (roleApplicationUser.Succeeded)
                    {
                        await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                        return RedirectToAction("Index", "News");
                    }
                }

                // Handle errors from registration
                foreach (var error in applicationUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(registerViewModel);
        }

        // GET: Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View();
        }

        // POST: Account/Login
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);

                        // Redirect based on roles
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("SuperAdmin"))
                        {
                            return RedirectToAction("Index", "SuperAdmin");
                        }
                        else if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        // Redirect to the News page for all other users
                        return RedirectToAction("Index", "News");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return View(loginViewModel);
        }


        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "News");
        }

        // Method to create roles if they don't exist
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateRoles(string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            return RedirectToAction("ManageRoles"); // Redirect to a role management page
        }

        // Action to assign a role to a user
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, roleName);
                return RedirectToAction("ManageUsers"); // Redirect to a user management page
            }
            return NotFound();
        }
    }
}
