using System.ComponentModel.DataAnnotations;

namespace MakerSpot.ViewModels
{
    public class CollectionFormViewModel
    {
        public int? CollectionId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên Collection")]
        [MaxLength(100)]
        [Display(Name = "Tên Collection")]
        public string CollectionName { get; set; } = null!;

        [MaxLength(300)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Công khai")]
        public bool IsPublic { get; set; } = true;
    }
}
