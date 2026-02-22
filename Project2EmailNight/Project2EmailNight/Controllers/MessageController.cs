using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Context;
using Project2EmailNight.Entities;
using Project2EmailNight.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;

namespace Project2EmailNight.Controllers
{
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IActionResult> Inbox()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            var messageList = _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && x.IsInbox == true && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(messageList);
        }

        public async Task<IActionResult> Sendbox()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            var list = _context.Messages
                .Where(x => x.SenderEmail == user.Email && x.IsSent == true && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(list);
        }

        [HttpGet]
        public IActionResult CreateMessage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(Message message)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            AppUser? receiver = null;
            if (!string.IsNullOrWhiteSpace(message.ReceiverEmail))
                receiver = await _userManager.FindByEmailAsync(message.ReceiverEmail);


            message.SenderEmail = user.Email;
            message.SenderName = user.Name;
            message.SenderSurname = user.Surname;

            message.ReceiverName = receiver?.Name;
            message.ReceiverSurname = receiver?.Surname;

            message.SendDate = DateTime.Now;

            message.IsInbox = false;
            message.IsSent = true;
            message.IsDraft = false;
            message.IsStarred = false;
            message.IsRead = true;
            message.IsDeleted = false;

            var inboxMessage = new Message
            {
                SenderEmail = user.Email,
                SenderName = user.Name,
                SenderSurname = user.Surname,

                ReceiverEmail = message.ReceiverEmail,
                ReceiverName = receiver?.Name,
                ReceiverSurname = receiver?.Surname,

                Subject = message.Subject,
                Body = message.Body,
                Category = message.Category,
                SendDate = DateTime.Now,

                IsInbox = true,
                IsSent = false,
                IsDraft = false,
                IsStarred = false,
                IsRead = false,
                IsDeleted = false
            };

            _context.Messages.Add(message);
            _context.Messages.Add(inboxMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Sendbox");
        }

        [HttpGet]
        public async Task<IActionResult> Category(string name)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            var key = NormalizeTr(name);
            var baseList = _context.Messages
                .Where(x =>
                    x.ReceiverEmail == user.Email &&
                    x.IsInbox == true &&
                    x.IsDeleted == false
                )
                .OrderByDescending(x => x.SendDate)
                .ToList();

            var list = baseList.Where(x =>
            {
                var cat = NormalizeTr(x.Category);

                if (key == "birincil" || key == "primary")
                    return string.IsNullOrWhiteSpace(x.Category) || cat == "birincil" || cat == "primary";

                if (key == "is" || key == "work")
                    return cat == "is" || cat == "work" || cat == "i̇s"; // ekstra güvenlik

                if (key == "egitim" || key == "education")
                    return cat == "egitim" || cat == "education";

                return cat == key;
            }).ToList();

            ViewData["Title"] = $"Kategori: {name}";
            return View(list);
        }

        private static string NormalizeTr(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            s = s.Trim().ToLowerInvariant();

            s = s.Replace("ı", "i")
                 .Replace("İ", "i")
                 .Replace("ş", "s")
                 .Replace("ğ", "g")
                 .Replace("ü", "u")
                 .Replace("ö", "o")
                 .Replace("ç", "c");

            s = s.Replace(" ", "");

            return s;
        }

        [HttpPost]
        public async Task<IActionResult> SaveDraft(Message message)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            message.SenderEmail = user.Email;
            message.SenderName = user.Name;
            message.SenderSurname = user.Surname;

            AppUser? receiver = null;
            if (!string.IsNullOrWhiteSpace(message.ReceiverEmail))
                receiver = await _userManager.FindByEmailAsync(message.ReceiverEmail);
            message.ReceiverName = receiver?.Name;
            message.ReceiverSurname = receiver?.Surname;

            message.SendDate = DateTime.Now;

            message.IsInbox = false;
            message.IsSent = false;
            message.IsDraft = true;
            message.IsStarred = false;
            message.IsRead = true;
            message.IsDeleted = false;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Draftbox");
        }

        [HttpGet]
        public IActionResult MessageDetail(int id)
        {
            var message = _context.Messages.FirstOrDefault(x => x.MessageId == id);
            if (message == null) return NotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                _context.SaveChanges();
            }

            return View(message);
        }

        [HttpGet]
        public IActionResult ToggleStar(int id)
        {
            var message = _context.Messages.FirstOrDefault(x => x.MessageId == id);
            if (message == null) return NotFound();

            message.IsStarred = !message.IsStarred;
            _context.SaveChanges();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
                return Redirect(referer);

            return RedirectToAction("Inbox");
        }

        public async Task<IActionResult> Starred()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            var starredMessages = _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && x.IsInbox == true && x.IsStarred == true && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(starredMessages);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var message = _context.Messages.FirstOrDefault(x => x.MessageId == id);
            if (message == null) return NotFound();

            message.IsDeleted = true;
            _context.SaveChanges();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
                return Redirect(referer);

            return RedirectToAction("Inbox");
        }

        public async Task<IActionResult> Trash()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            var trashMessages = _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && x.IsDeleted == true)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(trashMessages);
        }

        [HttpGet]
        public IActionResult Restore(int id)
        {
            var message = _context.Messages.FirstOrDefault(x => x.MessageId == id);
            if (message == null) return NotFound();

            message.IsDeleted = false;
            _context.SaveChanges();

            return RedirectToAction("Trash");
        }

        public async Task<IActionResult> Draftbox()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            var drafts = _context.Messages
                .Where(x => x.SenderEmail == user.Email && x.IsDraft == true && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(drafts);
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var original = _context.Messages.FirstOrDefault(x => x.MessageId == id);
            if (original == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            if (original.ReceiverEmail != user.Email) return Unauthorized();

            var vm = new ReplyViewModel
            {
                OriginalMessageId = id,
                ToEmail = original.SenderEmail,
                Subject = original.Subject != null && original.Subject.StartsWith("Re:")
                    ? original.Subject
                    : "Re: " + original.Subject,
                Body = "\n\n---\nÖnceki mesaj:\n" + HtmlToPlainText(original.Body)
            };

            return View(vm);
        }

        private static string HtmlToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";

            html = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
            html = Regex.Replace(html, @"</p\s*>", "\n", RegexOptions.IgnoreCase);

            var text = Regex.Replace(html, "<.*?>", string.Empty);
            text = WebUtility.HtmlDecode(text);

            return text.Replace("\r", "").Trim();
        }

        [HttpPost]
        public async Task<IActionResult> Reply(ReplyViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("UserLogin", "Login");

            AppUser? receiver = null;
            if (!string.IsNullOrWhiteSpace(vm.ToEmail))
                receiver = await _userManager.FindByEmailAsync(vm.ToEmail);


            var sent = new Message
            {
                SenderEmail = user.Email,
                SenderName = user.Name,
                SenderSurname = user.Surname,

                ReceiverEmail = vm.ToEmail,
                ReceiverName = receiver?.Name,
                ReceiverSurname = receiver?.Surname,

                Subject = vm.Subject,
                Body = vm.Body,
                Category = "Yanıt",
                SendDate = DateTime.Now,

                IsInbox = false,
                IsSent = true,
                IsDraft = false,
                IsDeleted = false,
                IsStarred = false,
                IsRead = true
            };

            var inbox = new Message
            {
                SenderEmail = user.Email,
                SenderName = user.Name,
                SenderSurname = user.Surname,

                ReceiverEmail = vm.ToEmail,
                ReceiverName = receiver?.Name,
                ReceiverSurname = receiver?.Surname,

                Subject = vm.Subject,
                Body = vm.Body,
                Category = "Yanıt",
                SendDate = DateTime.Now,

                IsInbox = true,
                IsSent = false,
                IsDraft = false,
                IsStarred = false,
                IsRead = false,
                IsDeleted = false
            };

            _context.Messages.Add(sent);
            _context.Messages.Add(inbox);
            await _context.SaveChangesAsync();

            return RedirectToAction("Sendbox");
        }
    }
}
