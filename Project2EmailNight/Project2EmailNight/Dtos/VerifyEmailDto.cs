using System.ComponentModel.DataAnnotations;

namespace Project2EmailNight.Dtos
{
    public class VerifyEmailDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalı")]
        public string Code { get; set; }
    }
}
