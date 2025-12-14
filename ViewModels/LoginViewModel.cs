using System.ComponentModel.DataAnnotations;

namespace Kino.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введіть Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}