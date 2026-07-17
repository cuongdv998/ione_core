using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResDamageLevels;

namespace iOne.Master.ResDamageLevels;

public class UpdateResDamageLevelDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResDamageLevel:ObjectTypeIdRequired")]
    [Display(Name = "ResDamageLevel:ObjectTypeId")]
    public Guid ObjectTypeId { get; set; }

    [Required(ErrorMessage = "ResDamageLevel:NameRequired")]
    [StringLength(250, ErrorMessage = "ResDamageLevel:NameMaxLength")]
    [Display(Name = "ResDamageLevel:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResDamageLevel:DescriptionMaxLength")]
    [Display(Name = "ResDamageLevel:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResDamageLevel:StatusRequired")]
    [Display(Name = "ResDamageLevel:Status")]
    public ResDamageLevelStatus Status { get; set; }
}

