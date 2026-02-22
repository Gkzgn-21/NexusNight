using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;
using Project2EmailNight.Models;


namespace Project2EmailNight.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var vm = new ProfileViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,

                City = user.City ?? "İstanbul",
                Website = user.Website ?? "project2emailnight",

                ImageUrl = user.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("UserLogin", "Login");

            if (!ModelState.IsValid)
                return View(vm);

            user.Name = vm.Name;
            user.Surname = vm.Surname;
            user.City = string.IsNullOrWhiteSpace(vm.City) ? "İstanbul" : vm.City;
            user.Website = string.IsNullOrWhiteSpace(vm.Website) ? "project2emailnight" : vm.Website;
            user.PhoneNumber = string.IsNullOrWhiteSpace(vm.Phone) ? "+90 542 000 00 00" : vm.Phone;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.ImageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Sadece JPG/PNG yükleyebilirsiniz.");
                    return View(vm);
                }

                var fileName = Guid.NewGuid().ToString("N") + ext;
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "userimages");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var path = Path.Combine(folder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                user.ImageUrl = "/userimages/" + fileName;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                    ModelState.AddModelError("", err.Description);

                return View(vm);
            }

            TempData["Success"] = "Profil güncellendi.";
            return RedirectToAction("Index");
        }

    }
}
