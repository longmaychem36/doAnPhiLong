using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.ViewModels
{
    public class SubmitProductViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên sản phẩm")]
        [RegularExpression(@"^[\S\s]*\S[\S\s]*$", ErrorMessage = "Tên sản phẩm không được chỉ chứa khoảng trắng")]
        [MaxLength(150, ErrorMessage = "Tên sản phẩm tối đa 150 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Slug")]
        [MaxLength(180, ErrorMessage = "Slug tối đa 180 ký tự")]
        [Display(Name = "Slug (URL thân thiện)")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug chỉ chứa chữ thường, số và dấu gạch ngang")]
        public string Slug { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Tagline")]
        [RegularExpression(@"^[\S\s]*\S[\S\s]*$", ErrorMessage = "Tagline không được chỉ chứa khoảng trắng")]
        [MaxLength(255, ErrorMessage = "Tagline tối đa 255 ký tự")]
        [Display(Name = "Tagline (Mô tả ngắn)")]
        public string Tagline { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Mô tả chi tiết")]
        [RegularExpression(@"^[\S\s]*\S[\S\s]*$", ErrorMessage = "Mô tả không được chỉ chứa khoảng trắng")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; } = null!;

        [Display(Name = "Ngừng hỗ trợ URL trực tiếp (Dùng tính năng Upload File mới)")]
        public string? LogoUrl { get; set; }

        [Display(Name = "Upload Ảnh Logo (*.png, *.jpg, tối đa 2MB)")]
        [DataType(DataType.Upload)]
        public Microsoft.AspNetCore.Http.IFormFile? LogoFile { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập URL Website")]
        [MaxLength(255, ErrorMessage = "URL Website tối đa 255 ký tự")]
        [Display(Name = "URL Website")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string WebsiteUrl { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "URL Demo tối đa 255 ký tự")]
        [Display(Name = "URL Demo (Tuỳ chọn)")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string? DemoUrl { get; set; }

        [Display(Name = "Ngày ra mắt")]
        [DataType(DataType.Date)]
        public DateTime? LaunchDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 chủ đề")]
        [Display(Name = "Chủ đề")]
        public List<int> SelectedTopicIds { get; set; } = new List<int>();

        [Display(Name = "Ảnh sản phẩm (tối đa 10 ảnh)")]
        [DataType(DataType.Upload)]
        public List<Microsoft.AspNetCore.Http.IFormFile>? ProductImages { get; set; }

        // For rendering dropdown/checkboxes
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableTopics { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }
}
