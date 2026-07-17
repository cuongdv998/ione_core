using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarCategories;

namespace iOne.Master.ResCarCategories;

public class CreateResCarCategoryDto
{
    [Required(ErrorMessage = "ResCarCategory:CarBrandIdRequired")]
    [Display(Name = "ResCarCategory:CarBrandId")]
    public Guid CarBrandId { get; set; }

    [Display(Name = "ResCarCategory:CarModelId")]
    public Guid? CarModelId { get; set; }

    [Required(ErrorMessage = "ResCarCategory:MotorClassIdRequired")]
    [Display(Name = "ResCarCategory:MotorClassId")]
    public Guid MotorClassId { get; set; }

    [Display(Name = "ResCarCategory:CarLineId")]
    public Guid? CarLineId { get; set; }

    [Required(ErrorMessage = "ResCarCategory:CodeRequired")]
    [StringLength(50, ErrorMessage = "ResCarCategory:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResCarCategory:CodeInvalid")]
    [Display(Name = "ResCarCategory:Code")]
    public string Code { get; set; } = null!;

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
    public ResCarCategoryStatus Status { get; set; } = ResCarCategoryStatus.Active;
}

