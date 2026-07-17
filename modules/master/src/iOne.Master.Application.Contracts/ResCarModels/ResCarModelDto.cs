using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarBrands;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarModels;

public class ResCarModelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCarModel:CarBrandId")]
    public Guid CarBrandId { get; set; }

    [Display(Name = "ResCarModel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCarModel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCarModel:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCarModel:Status")]
    public ResCarBrandStatus Status { get; set; }
}



