using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProLineOfBusinesses;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProLineOfBusinesses;

public class ProLineOfBusinessDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProLineOfBusiness:ParentId")]
    public Guid? ParentId { get; set; }

    [Display(Name = "ProLineOfBusiness:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProLineOfBusiness:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ProLineOfBusiness:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProLineOfBusiness:Status")]
    public ProLineOfBusinessStatus Status { get; set; }
}




