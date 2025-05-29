using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class UserLoginModel
    {
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 20 characters")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 20 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
