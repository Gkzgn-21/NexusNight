using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Context;
using Project2EmailNight.Entities;
using Project2EmailNight.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project2EmailNight.Controllers
{
    public class DashboardController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Kontrol Paneli";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var email = user.Email;

            var vm = new DashboardViewModel();

            // Yuvarlak kart sayıları
            vm.InboxCount = _context.Messages.Count(x => x.ReceiverEmail == email && x.IsInbox && !x.IsDeleted);
            vm.SentCount = _context.Messages.Count(x => x.SenderEmail == email && x.IsSent && !x.IsDeleted);
            vm.DraftCount = _context.Messages.Count(x => x.SenderEmail == email && x.IsDraft && !x.IsDeleted);
            vm.TrashCount = _context.Messages.Count(x => x.ReceiverEmail == email && x.IsDeleted);
            vm.StarredCount = _context.Messages.Count(x => x.ReceiverEmail == email && x.IsInbox && x.IsStarred && !x.IsDeleted);

            // Toplam gelen (silinmiş dahil)
            vm.TotalInboxCount = _context.Messages.Count(x => x.ReceiverEmail == email && x.IsInbox);

            // Son 7 gün grafikleri
            var today = DateTime.Today;

            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                var start = day;
                var end = day.AddDays(1);
                var index = 6 - i;

                vm.DayLabels[index] = day.ToString("ddd"); // Pzt, Sal...

                vm.InboxLast7Days[index] = _context.Messages.Count(x =>
                    x.ReceiverEmail == email && x.IsInbox && !x.IsDeleted &&
                    x.SendDate >= start && x.SendDate < end);

                vm.SentLast7Days[index] = _context.Messages.Count(x =>
                    x.SenderEmail == email && x.IsSent && !x.IsDeleted &&
                    x.SendDate >= start && x.SendDate < end);
            }

            return View(vm);
        }
    }
}
