using System;
using System.ComponentModel.DataAnnotations;
using iOne.Claims;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

public class ClaimDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Claim:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Claim:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "Claim:LobName")]
    public string? LobName { get; set; }

    [Display(Name = "Claim:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Display(Name = "Claim:InsurerName")]
    public string? InsurerName { get; set; }

    [Display(Name = "Claim:InsurerCode")]
    public string? InsurerCode { get; set; }

    [Display(Name = "Claim:ProcessClaimType")]
    public ProcessClaimType ProcessClaimType { get; set; }

    [Display(Name = "Claim:Status")]
    public ClaimStatus Status { get; set; }

    [Display(Name = "Claim:OpenDate")]
    public DateTime OpenDate { get; set; }

    [Display(Name = "Claim:NotifyDate")]
    public DateTime NotifyDate { get; set; }

    [Display(Name = "Claim:NotifierName")]
    public string NotifierName { get; set; } = null!;

    [Display(Name = "Claim:NotifierPhone")]
    public string NotifierPhone { get; set; } = null!;

    [Display(Name = "Claim:OpenEmployeeId")]
    public Guid OpenEmployeeId { get; set; }

    [Display(Name = "Claim:OpenEmployeeName")]
    public string? OpenEmployeeName { get; set; }

    [Display(Name = "Claim:CarPlate")]
    public string? CarPlate { get; set; }
}
