using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail.")]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải đúng định dạng Gmail, ví dụ: ten@gmail.com.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
