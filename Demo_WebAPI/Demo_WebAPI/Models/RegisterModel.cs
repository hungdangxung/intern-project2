using System.ComponentModel.DataAnnotations;

namespace Demo_WebAPI.Models
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
