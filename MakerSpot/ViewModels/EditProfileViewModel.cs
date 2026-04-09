using System.ComponentModel.DataAnnotations;

namespace MakerSpot.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [MaxLength(100)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = null!;

        [MaxLength(500)]
        [Display(Name = "Giới thiệu bản thân")]
        public string? Bio { get; set; }

        [Display(Name = "Ngừng hỗ trợ URL trực tiếp (Dùng tính năng Upload File mới)")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Upload Ảnh đại diện mới (Chỉ hỗ trợ .jpg, .png, < 2MB)")]
        [DataType(DataType.Upload)]
        public Microsoft.AspNetCore.Http.IFormFile? AvatarFile { get; set; }

        [MaxLength(255)]
        [Url(ErrorMessage = "URL Website không hợp lệ.")]
        [Display(Name = "Website")]
        public string? WebsiteUrl { get; set; }

        [MaxLength(255)]
        [Url(ErrorMessage = "URL Twitter không hợp lệ.")]
        [Display(Name = "Twitter / X")]
        public string? TwitterUrl { get; set; }

        [MaxLength(255)]
        [Url(ErrorMessage = "URL LinkedIn không hợp lệ.")]
        [Display(Name = "LinkedIn")]
        public string? LinkedinUrl { get; set; }
    }
}
