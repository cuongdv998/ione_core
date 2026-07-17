using System.ComponentModel.DataAnnotations;
using iOne.AdminConfigs;

namespace iOne.Master.AdminConfigs;

public class UpdateAdminConfigDto
{
    // ⚠️ QUAN TRỌNG: Không có Code và SubCode properties - Code và SubCode không được phép sửa

    [Required(ErrorMessage = "Master::AdminConfig:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:NameMaxLength")]
    [Display(Name = "Master::AdminConfig:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:ValueRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:ValueMaxLength")]
    [Display(Name = "Master::AdminConfig:Value")]
    public string Value { get; set; } = null!;

    [StringLength(250, ErrorMessage = "Master::AdminConfig:DescriptionMaxLength")]
    [Display(Name = "Master::AdminConfig:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::AdminConfig:StatusRequired")]
    [Display(Name = "Master::AdminConfig:Status")]
    public AdminConfigStatus Status { get; set; }
}

