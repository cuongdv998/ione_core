using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResOrganizationTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResOrganizationTypes;

public class ResOrganizationTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResOrganizationType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResOrganizationType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResOrganizationType:Status")]
    public ResOrganizationTypeStatus Status { get; set; }

    [Display(Name = "ResOrganizationType:Type")]
    public OrganizationTypeType Type { get; set; }
}

