using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

public class UpdateResProvinceDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResProvince:CountryIdRequired")]
    [Display(Name = "ResProvince:CountryId")]
    public Guid CountryId { get; set; }

    [Required(ErrorMessage = "ResProvince:NameRequired")]
    [StringLength(250, ErrorMessage = "ResProvince:NameMaxLength")]
    [Display(Name = "ResProvince:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResProvince:StatusRequired")]
    [Display(Name = "ResProvince:Status")]
    public ResProvinceStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ResProvince:DescriptionMaxLength")]
    [Display(Name = "ResProvince:Description")]
    public string? Description { get; set; }
}

