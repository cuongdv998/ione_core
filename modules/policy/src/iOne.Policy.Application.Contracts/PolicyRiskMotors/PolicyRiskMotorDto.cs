using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyRiskMotors;

public class PolicyRiskMotorDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyRiskMotor:PolicyRiskObjectId")]
    public Guid? PolicyRiskObjectId { get; set; }

    [Display(Name = "PolicyRiskMotor:RiskObjectValue")]
    public decimal? RiskObjectValue { get; set; }

    [Display(Name = "PolicyRiskMotor:MotorClassCode")]
    public string? MotorClassCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarLineCode")]
    public string? CarLineCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarGroupCode")]
    public string? CarGroupCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarTypeCode")]
    public string? CarTypeCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarBrandCode")]
    public string? CarBrandCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarModelCode")]
    public string? CarModelCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarCategoryCode")]
    public string? CarCategoryCode { get; set; }

    [Display(Name = "PolicyRiskMotor:CarUsage")]
    public string? CarUsage { get; set; }

    [Display(Name = "PolicyRiskMotor:CarOld")]
    public decimal? CarOld { get; set; }

    [Display(Name = "PolicyRiskMotor:CarProductionYear")]
    public DateTime? CarProductionYear { get; set; }

    [Display(Name = "PolicyRiskMotor:CarPlate")]
    public string? CarPlate { get; set; }

    [Display(Name = "PolicyRiskMotor:CarPlateType")]
    public string? CarPlateType { get; set; }

    [Display(Name = "PolicyRiskMotor:CarPlateClear")]
    public string? CarPlateClear { get; set; }

    [Display(Name = "PolicyRiskMotor:CarSeatNumber")]
    public decimal? CarSeatNumber { get; set; }

    [Display(Name = "PolicyRiskMotor:CarVin")]
    public string? CarVin { get; set; }

    [Display(Name = "PolicyRiskMotor:CarEngineNumber")]
    public string? CarEngineNumber { get; set; }

    [Display(Name = "PolicyRiskMotor:CarPayloadCapacity")]
    public decimal? CarPayloadCapacity { get; set; }

    [Display(Name = "PolicyRiskMotor:CarColor")]
    public string? CarColor { get; set; }

    [Display(Name = "PolicyRiskMotor:CarOrigin")]
    public string? CarOrigin { get; set; }

    [Display(Name = "PolicyRiskMotor:CarNew")]
    public string? CarNew { get; set; }
}
