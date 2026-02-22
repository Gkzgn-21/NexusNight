using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Project2EmailNight.Services;

namespace Project2EmailNight.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _mail;

        public RegisterController(UserManager<AppUser> userManager, IConfiguration config, IEmailService mail)
        {
            _userManager = userManager;
            _config = config;
            _mail = mail;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = new AppUser
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                UserName = dto.Username,
                EmailConfirmed = false
            };

            var code = RandomNumberGenerator.GetInt32(100000, 1000000);
            user.ConfirmCode = code;

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);

                return View(dto);
            }

            try
            {
                await _mail.SendAsync(dto.Email, "NexusNight - Doğrulama Kodu", $"Doğrulama kodunuz: {code}");
            }
            catch (Exception ex)
            {
                TempData["MailError"] = "Doğrulama maili gönderilemedi: " + ex.Message;
            }

            return RedirectToAction("VerifyEmail", new { email = dto.Email });
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            return View(new VerifyEmailDto { Email = email });
        }

        [HttpGet]
        public async Task<IActionResult> ResendCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["MailError"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("VerifyEmail", new { email });
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000);
            user.ConfirmCode = code;

            await _userManager.UpdateAsync(user);

            try
            {
                await _mail.SendAsync(
                    user.Email,
                    "NexusNight - Doğrulama Kodu",
                    $"Doğrulama kodunuz: {code}"
                );
                TempData["Ok"] = "Doğrulama kodu tekrar gönderildi.";
            }
            catch (Exception ex)
            {
                TempData["MailError"] = ex.Message;
            }

            return RedirectToAction("VerifyEmail", new { email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(dto);
            }

            if (user.ConfirmCode == null)
            {
                ModelState.AddModelError("", "Kod bulunamadı. Tekrar kayıt olmayı deneyin.");
                return View(dto);
            }

            if (user.ConfirmCode == null || user.ConfirmCode.ToString() != dto.Code)
            {
                ModelState.AddModelError("", "Doğrulama kodu hatalı.");
                return View(dto);
            }

            user.EmailConfirmed = true;
            user.ConfirmCode = null;
            await _userManager.UpdateAsync(user);

            TempData["Ok"] = "Email doğrulandı. Artık giriş yapabilirsin.";
            return RedirectToAction("UserLogin", "Login");
        }
    }
}