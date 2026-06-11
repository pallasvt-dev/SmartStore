using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail.")]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải đúng định dạng Gmail, ví dụ: ten@gmail.com.")]
        public string Email { get; set; } = string.Empty;
    }
}
