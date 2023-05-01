using CleanArchitecture.Core.Domain.IdentityEntities;
using CleanArchitecture.Core.DTO;
using CleanArchitecture.Core.Enums;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize("NotAuthorized")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthorized")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            //Check for validation errors
            if (ModelState.IsValid == false)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(registerDTO);
            }

            ApplicationUser user = new ApplicationUser() { Email = registerDTO.Email, PhoneNumber = registerDTO.Phone, UserName = registerDTO.Email, PersonName = registerDTO.PersonName };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                //Check status of radio button
                if (registerDTO.UserType == RoleOptions.Admin)
                {
                    //Create 'Admin' role
                    if (await _roleManager.FindByNameAsync(RoleOptions.Admin.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name = RoleOptions.Admin.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }

                    //Add the new user into 'Admin' role
                    await _userManager.AddToRoleAsync(user, RoleOptions.Admin.ToString());
                }
                else
                {
                    //Add the new user into 'User' role
                    await _userManager.AddToRoleAsync(user, RoleOptions.User.ToString());
                }
                //Sign in
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return View(registerDTO);
            }

        }

        [HttpGet]
        [Authorize("NotAuthorized")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO login, string ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                return View(login);
            }

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }

                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }                

            ModelState.AddModelError("Login", "Invalid email or password");
            return View(login);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }

        public async Task<IActionResult> IsEmailAlreadyExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return Json(true);

            return Json(false);
        }
    }
}
