using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResWards;

namespace iOne.Master.ResWards;

public class CreateResWardDto
{
    [Required(ErrorMessage = "Master::ResWard:ProvinceRequired")]
    [Display(Name = "Master::ResWard:Province")]
    public Guid ProvinceId { get; set; }

    [Required(ErrorMessage = "Master::ResWard:CodeRequired")]
    [StringLength(25, ErrorMessage = "Master::ResWard:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::ResWard:CodeInvalid")]
    [Display(Name = "Master::ResWard:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResWard:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResWard:NameMaxLength")]
    [Display(Name = "Master::ResWard:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResWard:StatusRequired")]
    [Display(Name = "Master::ResWard:Status")]
    public ResWardStatus Status { get; set; } = ResWardStatus.Active;

    [StringLength(500, ErrorMessage = "Master::ResWard:DescriptionMaxLength")]
    [Display(Name = "Master::ResWard:Description")]
    public string? Description { get; set; }
}

