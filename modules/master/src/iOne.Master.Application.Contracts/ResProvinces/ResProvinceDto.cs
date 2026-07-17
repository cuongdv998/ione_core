using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResProvinces;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResProvinces;

public class ResProvinceDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResProvince:CountryId")]
    public Guid CountryId { get; set; }

    [Display(Name = "ResProvince:CountryName")]
    public string? CountryName { get; set; }

    [Display(Name = "ResProvince:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResProvince:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResProvince:Status")]
    public ResProvinceStatus Status { get; set; }

    [Display(Name = "ResProvince:Description")]
    public string? Description { get; set; }
}

