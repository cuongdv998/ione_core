using System.ComponentModel.DataAnnotations;
using iOne.ResCarCategories;

namespace iOne.Master.ResCarCategories;

public class UpdateResCarCategoryDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có CarBrandId property - CarBrandId không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có CarModelId property - CarModelId không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có MotorClassId property - MotorClassId không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có CarLineId property - CarLineId không được phép sửa

    [Required(ErrorMessage = "ResCarCategory:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarCategory:NameMaxLength")]
    [Display(Name = "ResCarCategory:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResCarCategory:SeatNumberRequired")]
    [Range(0, 99, ErrorMessage = "ResCarCategory:SeatNumberRange")]
    [Display(Name = "ResCarCategory:SeatNumber")]
    public int SeatNumber { get; set; }

    [StringLength(500, ErrorMessage = "ResCarCategory:DescriptionMaxLength")]
    [Display(Name = "ResCarCategory:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarCategory:StatusRequired")]
    [Display(Name = "ResCarCategory:Status")]
    public ResCarCategoryStatus Status { get; set; }
}

