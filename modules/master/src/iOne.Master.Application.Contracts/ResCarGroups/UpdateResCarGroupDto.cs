using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarGroups;

namespace iOne.Master.ResCarGroups;

public class UpdateResCarGroupDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Display(Name = "ResCarGroup:CarLineId")]
    public Guid? CarLineId { get; set; }

    [Required(ErrorMessage = "ResCarGroup:NameRequired")]
    [StringLength(250, ErrorMessage = "ResCarGroup:NameMaxLength")]
    [Display(Name = "ResCarGroup:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "ResCarGroup:DescriptionMaxLength")]
    [Display(Name = "ResCarGroup:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ResCarGroup:StatusRequired")]
    [Display(Name = "ResCarGroup:Status")]
    public ResCarGroupStatus Status { get; set; }
}
