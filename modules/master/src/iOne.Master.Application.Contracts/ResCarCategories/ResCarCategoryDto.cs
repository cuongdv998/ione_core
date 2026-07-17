using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarCategories;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarCategories;

public class ResCarCategoryDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCarCategory:CarBrandId")]
    public Guid CarBrandId { get; set; }

    [Display(Name = "ResCarCategory:CarModelId")]
    public Guid? CarModelId { get; set; }

    [Display(Name = "ResCarCategory:MotorClassId")]
    public Guid MotorClassId { get; set; }

    [Display(Name = "ResCarCategory:CarLineId")]
    public Guid? CarLineId { get; set; }

    [Display(Name = "ResCarCategory:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCarCategory:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCarCategory:SeatNumber")]
    public int SeatNumber { get; set; }

    [Display(Name = "ResCarCategory:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCarCategory:Status")]
    public ResCarCategoryStatus Status { get; set; }
}

