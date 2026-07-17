using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductDistributions;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductDistributions;

public class ProProductDistributionDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductDistribution:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "ProProductDistribution:ChannelId")]
    public Guid? ChannelId { get; set; }

    [Display(Name = "ProProductDistribution:AppChannelId")]
    public Guid? AppChannelId { get; set; }

    [Display(Name = "ProProductDistribution:EmployeeRoleId")]
    public Guid? EmployeeRoleId { get; set; }

    [Display(Name = "ProProductDistribution:Status")]
    public ProProductDistributionStatus Status { get; set; }
}
