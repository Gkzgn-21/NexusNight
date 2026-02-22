using Microsoft.AspNetCore.Identity;

namespace Project2EmailNight.Entities
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public string? About { get; set; }
        public string? City { get; set; }
        public string? Website { get; set; }
        public int? ConfirmCode { get; set; }
    }
}
