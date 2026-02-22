using System.ComponentModel.DataAnnotations;
namespace Project2EmailNight.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad zorunlu")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad zorunlu")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunlu")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email zorunlu")]
        [EmailAddress(ErrorMessage = "Email formatı hatalı")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunlu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrar zorunlu")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor")]
        public string PasswordConfirm { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Şartları kabul etmelisiniz")]
        public bool AcceptTerms { get; set; }
    }
}
