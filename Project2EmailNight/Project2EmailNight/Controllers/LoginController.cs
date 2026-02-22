using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginDto userLoginDto)
        {

            if (!ModelState.IsValid)
                return View(userLoginDto);

            AppUser? user;
            var input = (userLoginDto.Username ?? "").Trim();

            if (input.Contains("@"))
                user = await _userManager.FindByEmailAsync(input);
            else
                user = await _userManager.FindByNameAsync(input);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                return View(userLoginDto);
            }

            if (!user.EmailConfirmed)
            {
                TempData["warn"] = "Lütfen email adresinizi doğrulayın.";
                return RedirectToAction("VerifyEmail", "Register", new { email = user.Email });
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                userLoginDto.Password,
                isPersistent: false,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
                return RedirectToAction("Index", "Dashboard");

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Çok fazla hatalı deneme. Hesabınız 5 dakika kilitlendi.");
                return View(userLoginDto);
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
            return View(userLoginDto);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
