using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProProductDistributions;

namespace iOne.Product.ProProductDistributions;

public class CreateProProductDistributionDto
{
    [Required(ErrorMessage = "ProProductDistribution:ProductIdRequired")]
    [Display(Name = "ProProductDistribution:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "ProProductDistribution:ChannelId")]
    public Guid? ChannelId { get; set; }

    [Display(Name = "ProProductDistribution:AppChannelId")]
    public Guid? AppChannelId { get; set; }

    [Display(Name = "ProProductDistribution:EmployeeRoleId")]
    public Guid? EmployeeRoleId { get; set; }

    [Required(ErrorMessage = "ProProductDistribution:StatusRequired")]
    [Display(Name = "ProProductDistribution:Status")]
    public ProProductDistributionStatus Status { get; set; } = ProProductDistributionStatus.Active;
}
