using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResBusinessAuthorities;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResBusinessAuthorities;

public class ResBusinessAuthorityDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResBusinessAuthority:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResBusinessAuthority:BusinessCode")]
    public string BusinessCode { get; set; } = null!;

    [Display(Name = "ResBusinessAuthority:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResBusinessAuthority:Status")]
    public ResBusinessAuthorityStatus Status { get; set; }
}
