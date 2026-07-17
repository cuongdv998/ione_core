using System.ComponentModel.DataAnnotations;
using iOne.AdminConfigs;

namespace iOne.Master.AdminConfigs;

public class CreateAdminConfigDto
{
    [Required(ErrorMessage = "Master::AdminConfig:CodeRequired")]
    [StringLength(25, ErrorMessage = "Master::AdminConfig:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::AdminConfig:CodeInvalid")]
    [Display(Name = "Master::AdminConfig:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:NameMaxLength")]
    [Display(Name = "Master::AdminConfig:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:SubCodeRequired")]
    [StringLength(25, ErrorMessage = "Master::AdminConfig:SubCodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::AdminConfig:SubCodeInvalid")]
    [Display(Name = "Master::AdminConfig:SubCode")]
    public string SubCode { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:ValueRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:ValueMaxLength")]
    [Display(Name = "Master::AdminConfig:Value")]
    public string Value { get; set; } = null!;

    [StringLength(250, ErrorMessage = "Master::AdminConfig:DescriptionMaxLength")]
    [Display(Name = "Master::AdminConfig:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::AdminConfig:StatusRequired")]
    [Display(Name = "Master::AdminConfig:Status")]
    public AdminConfigStatus Status { get; set; } = AdminConfigStatus.Active;
}

