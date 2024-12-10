using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Invalid Password lenght", MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Password do not Match")]
        public string ConfirmPassword { get; set; }
    }
}
