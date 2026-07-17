using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResPartnerTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartnerTypes;

public class ResPartnerTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResPartnerType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResPartnerType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResPartnerType:Status")]
    public ResPartnerTypeStatus Status { get; set; }
}

