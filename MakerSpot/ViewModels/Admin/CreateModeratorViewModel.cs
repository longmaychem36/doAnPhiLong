using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MakerSpot.ViewModels.Admin
{
    public class CreateModeratorViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ gồm chữ, số và dấu gạch dưới")]
        [MaxLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Chủ đề phụ trách (Bắt buộc)")]
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một chủ đề để phân công")]
        public List<int> SelectedTopicIds { get; set; } = new List<int>();

        public List<SelectListItem> AvailableTopics { get; set; } = new List<SelectListItem>();
    }
}
