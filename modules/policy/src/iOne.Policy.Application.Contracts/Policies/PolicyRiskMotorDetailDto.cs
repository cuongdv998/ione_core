using System;

namespace iOne.Policy.Policies;

public class PolicyRiskMotorDetailDto
{
    public Guid Id { get; set; } // policy_risk_motor.id

    public decimal? RiskObjectValue { get; set; }

    public string? MotorClassCode { get; set; }
    public Guid? MotorClassId { get; set; }
    
    public string? CarLineCode { get; set; }
    public Guid? CarLineId { get; set; }
    
    public string? CarGroupCode { get; set; }
    public Guid? CarGroupId { get; set; }
    
    public string? CarTypeCode { get; set; }
    public Guid? CarTypeId { get; set; }
    
    public string? CarBrandCode { get; set; }
    public Guid? CarBrandId { get; set; }
    
    public string? CarModelCode { get; set; }
    public Guid? CarModelId { get; set; }
    
    public string? CarCategoryCode { get; set; }
    public Guid? CarCategoryId { get; set; }

    public string? CarUsage { get; set; }
    public decimal? CarOld { get; set; }
    public DateTime? CarProductionYear { get; set; }
    public string? CarPlate { get; set; }
    public string? CarPlateType { get; set; }
    public string? CarPlateClear { get; set; }
    public decimal? CarSeatNumber { get; set; }
    public string? CarVin { get; set; }
    public string? CarEngineNumber { get; set; }
    public decimal? CarPayloadCapacity { get; set; }
    public string? CarColor { get; set; }

    public string? CarOrigin { get; set; }
    public string? CarNew { get; set; }
}

