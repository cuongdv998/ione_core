using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

public class CreateResProvinceDto
{
    [Required(ErrorMessage = "ResProvince:CountryIdRequired")]
    [Display(Name = "ResProvince:CountryId")]
    public Guid CountryId { get; set; }

    [Required(ErrorMessage = "ResProvince:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResProvince:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResProvince:CodeInvalid")]
    [Display(Name = "ResProvince:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResProvince:NameRequired")]
    [StringLength(250, ErrorMessage = "ResProvince:NameMaxLength")]
    [Display(Name = "ResProvince:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResProvince:StatusRequired")]
    [Display(Name = "ResProvince:Status")]
    public ResProvinceStatus Status { get; set; } = ResProvinceStatus.Active;

    [StringLength(500, ErrorMessage = "ResProvince:DescriptionMaxLength")]
    [Display(Name = "ResProvince:Description")]
    public string? Description { get; set; }
}

