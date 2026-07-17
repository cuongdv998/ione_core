using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarBrands;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarBrands;

public class ResCarBrandDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCarBrand:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCarBrand:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCarBrand:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCarBrand:Status")]
    public ResCarBrandStatus Status { get; set; }
}



