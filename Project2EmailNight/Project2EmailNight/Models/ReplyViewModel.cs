using System.ComponentModel.DataAnnotations;

namespace Project2EmailNight.Models
{
    public class ReplyViewModel
    {
        public int OriginalMessageId { get; set; }

        [Required, EmailAddress]
        public string? ToEmail { get; set; }

        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Body { get; set; }
    }

}

