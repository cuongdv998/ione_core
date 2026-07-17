using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.PolicyRiskMotors;

public class CreatePolicyRiskMotorDto
{
    [Display(Name = "PolicyRiskMotor:PolicyRiskObjectId")]
    public Guid? PolicyRiskObjectId { get; set; }

    [Display(Name = "PolicyRiskMotor:RiskObjectValue")]
    public decimal? RiskObjectValue { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:MotorClassCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:MotorClassCode")]
    public string? MotorClassCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarLineCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarLineCode")]
    public string? CarLineCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarGroupCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarGroupCode")]
    public string? CarGroupCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarTypeCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarTypeCode")]
    public string? CarTypeCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarBrandCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarBrandCode")]
    public string? CarBrandCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarModelCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarModelCode")]
    public string? CarModelCode { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarCategoryCodeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarCategoryCode")]
    public string? CarCategoryCode { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarUsageMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarUsage")]
    public string? CarUsage { get; set; }

    [Display(Name = "PolicyRiskMotor:CarOld")]
    public decimal? CarOld { get; set; }

    [Display(Name = "PolicyRiskMotor:CarProductionYear")]
    public DateTime? CarProductionYear { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarPlateMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarPlate")]
    public string? CarPlate { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarPlateTypeMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarPlateType")]
    public string? CarPlateType { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarPlateClearMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarPlateClear")]
    public string? CarPlateClear { get; set; }

    [Display(Name = "PolicyRiskMotor:CarSeatNumber")]
    public decimal? CarSeatNumber { get; set; }

    [StringLength(25, ErrorMessage = "PolicyRiskMotor:CarVinMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarVin")]
    public string? CarVin { get; set; }

    [StringLength(25, ErrorMessage = "PolicyRiskMotor:CarEngineNumberMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarEngineNumber")]
    public string? CarEngineNumber { get; set; }

    [Display(Name = "PolicyRiskMotor:CarPayloadCapacity")]
    public decimal? CarPayloadCapacity { get; set; }

    [StringLength(15, ErrorMessage = "PolicyRiskMotor:CarColorMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarColor")]
    public string? CarColor { get; set; }

    [StringLength(50, ErrorMessage = "PolicyRiskMotor:CarOriginMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarOrigin")]
    public string? CarOrigin { get; set; }

    [StringLength(1, ErrorMessage = "PolicyRiskMotor:CarNewMaxLength")]
    [Display(Name = "PolicyRiskMotor:CarNew")]
    public string? CarNew { get; set; }
}
