using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResTaskCategories;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResTaskCategories;

public class ResTaskCategoryDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResTaskCategory:BusinessType")]
    public ResTaskCategoryBusinessType BusinessType { get; set; }

    [Display(Name = "ResTaskCategory:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResTaskCategory:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResTaskCategory:Status")]
    public ResTaskCategoryStatus Status { get; set; }
}
