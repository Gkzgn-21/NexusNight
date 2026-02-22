using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Project2EmailNight.Models
{
    public class ProfileViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string City { get; set; }
        public string? Phone { get; set; }
        public string Website { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? NewPassword { get; set; }
    }
}