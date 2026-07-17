using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_risk_motor")]
public class PolicyRiskMotor : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    public virtual Guid? PolicyRiskObjectId { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? RiskObjectValue { get; private set; }

    [MaxLength(50)]
    public virtual string? MotorClassCode { get; private set; }

    [MaxLength(50)]
    public virtual string? CarLineCode { get; private set; }

    [MaxLength(50)]
    public virtual string? CarGroupCode { get; private set; }

    [MaxLength(50)]
    public virtual string? CarTypeCode { get; private set; }

    [MaxLength(50)]
    public virtual string? CarBrandCode { get; private set; }

    [MaxLength(50)]
    public virtual string? CarModelCode { get; private set; }

    [MaxLength(50)]
    public virtual string? CarCategoryCode { get; private set; }

    [MaxLength(15)]
    public virtual string? CarUsage { get; private set; }

    [Column(TypeName = "NUMERIC(6,3)")]
    public virtual decimal? CarOld { get; private set; }

    public virtual DateTime? CarProductionYear { get; private set; }

    [MaxLength(15)]
    public virtual string? CarPlate { get; private set; }

    [MaxLength(50)]
    public virtual string? CarPlateType { get; private set; }

    [MaxLength(15)]
    public virtual string? CarPlateClear { get; private set; }

    [Column(TypeName = "NUMERIC(2)")]
    public virtual decimal? CarSeatNumber { get; private set; }

    [MaxLength(25)]
    public virtual string? CarVin { get; private set; }

    [MaxLength(25)]
    public virtual string? CarEngineNumber { get; private set; }

    [Column(TypeName = "NUMERIC(6,3)")]
    public virtual decimal? CarPayloadCapacity { get; private set; }

    [MaxLength(15)]
    public virtual string? CarColor { get; private set; }

    [MaxLength(50)]
    public virtual string? CarOrigin { get; private set; }

    /// <summary>
    /// 'Y' or 'N' flag indicating the car is new.
    /// </summary>
    [MaxLength(1)]
    public virtual string? CarNew { get; private set; }

    // Navigation Properties
    public virtual PolicyRiskObject? PolicyRiskObject { get; set; }

    protected PolicyRiskMotor()
    {
        // For ORM
    }

    public PolicyRiskMotor(
        Guid id,
        Guid? policyRiskObjectId = null,
        decimal? riskObjectValue = null,
        string? motorClassCode = null,
        string? carLineCode = null,
        string? carGroupCode = null,
        string? carTypeCode = null,
        string? carBrandCode = null,
        string? carModelCode = null,
        string? carCategoryCode = null,
        string? carUsage = null,
        decimal? carOld = null,
        DateTime? carProductionYear = null,
        string? carPlate = null,
        string? carPlateType = null,
        string? carPlateClear = null,
        decimal? carSeatNumber = null,
        string? carVin = null,
        string? carEngineNumber = null,
        decimal? carPayloadCapacity = null,
        string? carColor = null,
        string? carOrigin = null,
        string? carNew = null)
        : base(id)
    {
        SetPolicyRiskObjectId(policyRiskObjectId);
        SetRiskObjectValue(riskObjectValue);
        SetMotorClassCode(motorClassCode);
        SetCarLineCode(carLineCode);
        SetCarGroupCode(carGroupCode);
        SetCarTypeCode(carTypeCode);
        SetCarBrandCode(carBrandCode);
        SetCarModelCode(carModelCode);
        SetCarCategoryCode(carCategoryCode);
        SetCarUsage(carUsage);
        SetCarOld(carOld);
        SetCarProductionYear(carProductionYear);
        SetCarPlate(carPlate);
        SetCarPlateType(carPlateType);
        SetCarPlateClear(carPlateClear);
        SetCarSeatNumber(carSeatNumber);
        SetCarVin(carVin);
        SetCarEngineNumber(carEngineNumber);
        SetCarPayloadCapacity(carPayloadCapacity);
        SetCarColor(carColor);
        SetCarOrigin(carOrigin);
        SetCarNew(carNew);
    }

    // Private setters with validation
    private void SetPolicyRiskObjectId(Guid? policyRiskObjectId)
    {
        PolicyRiskObjectId = policyRiskObjectId;
    }

    private void SetRiskObjectValue(decimal? riskObjectValue)
    {
        if (riskObjectValue.HasValue && riskObjectValue.Value < 0)
        {
            throw new ArgumentException("RiskObjectValue cannot be negative.", nameof(riskObjectValue));
        }
        RiskObjectValue = riskObjectValue;
    }

    private void SetMotorClassCode(string? motorClassCode)
    {
        if (motorClassCode != null && motorClassCode.Length > 50)
        {
            throw new ArgumentException("MotorClassCode cannot exceed 50 characters.", nameof(motorClassCode));
        }
        MotorClassCode = motorClassCode;
    }

    private void SetCarLineCode(string? carLineCode)
    {
        if (carLineCode != null && carLineCode.Length > 50)
        {
            throw new ArgumentException("CarLineCode cannot exceed 50 characters.", nameof(carLineCode));
        }
        CarLineCode = carLineCode;
    }

    private void SetCarGroupCode(string? carGroupCode)
    {
        if (carGroupCode != null && carGroupCode.Length > 50)
        {
            throw new ArgumentException("CarGroupCode cannot exceed 50 characters.", nameof(carGroupCode));
        }
        CarGroupCode = carGroupCode;
    }

    private void SetCarTypeCode(string? carTypeCode)
    {
        if (carTypeCode != null && carTypeCode.Length > 50)
        {
            throw new ArgumentException("CarTypeCode cannot exceed 50 characters.", nameof(carTypeCode));
        }
        CarTypeCode = carTypeCode;
    }

    private void SetCarBrandCode(string? carBrandCode)
    {
        if (carBrandCode != null && carBrandCode.Length > 50)
        {
            throw new ArgumentException("CarBrandCode cannot exceed 50 characters.", nameof(carBrandCode));
        }
        CarBrandCode = carBrandCode;
    }

    private void SetCarModelCode(string? carModelCode)
    {
        if (carModelCode != null && carModelCode.Length > 50)
        {
            throw new ArgumentException("CarModelCode cannot exceed 50 characters.", nameof(carModelCode));
        }
        CarModelCode = carModelCode;
    }

    private void SetCarCategoryCode(string? carCategoryCode)
    {
        if (carCategoryCode != null && carCategoryCode.Length > 50)
        {
            throw new ArgumentException("CarCategoryCode cannot exceed 50 characters.", nameof(carCategoryCode));
        }
        CarCategoryCode = carCategoryCode;
    }

    private void SetCarUsage(string? carUsage)
    {
        if (carUsage != null && carUsage.Length > 15)
        {
            throw new ArgumentException("CarUsage cannot exceed 15 characters.", nameof(carUsage));
        }
        CarUsage = carUsage;
    }

    private void SetCarOld(decimal? carOld)
    {
        if (carOld.HasValue && carOld.Value < 0)
        {
            throw new ArgumentException("CarOld cannot be negative.", nameof(carOld));
        }
        CarOld = carOld;
    }

    private void SetCarProductionYear(DateTime? carProductionYear)
    {
        CarProductionYear = carProductionYear;
    }

    private void SetCarPlate(string? carPlate)
    {
        if (carPlate != null && carPlate.Length > 15)
        {
            throw new ArgumentException("CarPlate cannot exceed 15 characters.", nameof(carPlate));
        }
        CarPlate = carPlate;
    }

    private void SetCarPlateType(string? carPlateType)
    {
        if (carPlateType != null && carPlateType.Length > 50)
        {
            throw new ArgumentException("CarPlateType cannot exceed 50 characters.", nameof(carPlateType));
        }
        CarPlateType = carPlateType;
    }

    private void SetCarPlateClear(string? carPlateClear)
    {
        if (carPlateClear != null && carPlateClear.Length > 15)
        {
            throw new ArgumentException("CarPlateClear cannot exceed 15 characters.", nameof(carPlateClear));
        }
        CarPlateClear = carPlateClear;
    }

    private void SetCarSeatNumber(decimal? carSeatNumber)
    {
        if (carSeatNumber.HasValue && carSeatNumber.Value < 0)
        {
            throw new ArgumentException("CarSeatNumber cannot be negative.", nameof(carSeatNumber));
        }
        CarSeatNumber = carSeatNumber;
    }

    private void SetCarVin(string? carVin)
    {
        if (carVin != null && carVin.Length > 25)
        {
            throw new ArgumentException("CarVin cannot exceed 25 characters.", nameof(carVin));
        }
        CarVin = carVin;
    }

    private void SetCarEngineNumber(string? carEngineNumber)
    {
        if (carEngineNumber != null && carEngineNumber.Length > 25)
        {
            throw new ArgumentException("CarEngineNumber cannot exceed 25 characters.", nameof(carEngineNumber));
        }
        CarEngineNumber = carEngineNumber;
    }

    private void SetCarPayloadCapacity(decimal? carPayloadCapacity)
    {
        if (carPayloadCapacity.HasValue && carPayloadCapacity.Value < 0)
        {
            throw new ArgumentException("CarPayloadCapacity cannot be negative.", nameof(carPayloadCapacity));
        }
        CarPayloadCapacity = carPayloadCapacity;
    }

    private void SetCarColor(string? carColor)
    {
        if (carColor != null && carColor.Length > 15)
        {
            throw new ArgumentException("CarColor cannot exceed 15 characters.", nameof(carColor));
        }
        CarColor = carColor;
    }

    private void SetCarOrigin(string? carOrigin)
    {
        if (carOrigin != null && carOrigin.Length > 50)
        {
            throw new ArgumentException("CarOrigin cannot exceed 50 characters.", nameof(carOrigin));
        }

        CarOrigin = carOrigin;
    }

    private void SetCarNew(string? carNew)
    {
        if (string.IsNullOrWhiteSpace(carNew))
        {
            CarNew = null;
            return;
        }

        var normalized = carNew.Trim().ToUpperInvariant();
        if (normalized is not ("Y" or "N"))
        {
            throw new ArgumentException("CarNew must be 'Y' or 'N'.", nameof(carNew));
        }

        CarNew = normalized;
    }

    // Public update methods
    public virtual void UpdatePolicyRiskObjectId(Guid? policyRiskObjectId)
    {
        SetPolicyRiskObjectId(policyRiskObjectId);
    }

    public virtual void UpdateRiskObjectValue(decimal? riskObjectValue)
    {
        SetRiskObjectValue(riskObjectValue);
    }

    public virtual void UpdateMotorClassCode(string? motorClassCode)
    {
        SetMotorClassCode(motorClassCode);
    }

    public virtual void UpdateCarLineCode(string? carLineCode)
    {
        SetCarLineCode(carLineCode);
    }

    public virtual void UpdateCarGroupCode(string? carGroupCode)
    {
        SetCarGroupCode(carGroupCode);
    }

    public virtual void UpdateCarTypeCode(string? carTypeCode)
    {
        SetCarTypeCode(carTypeCode);
    }

    public virtual void UpdateCarBrandCode(string? carBrandCode)
    {
        SetCarBrandCode(carBrandCode);
    }

    public virtual void UpdateCarModelCode(string? carModelCode)
    {
        SetCarModelCode(carModelCode);
    }

    public virtual void UpdateCarCategoryCode(string? carCategoryCode)
    {
        SetCarCategoryCode(carCategoryCode);
    }

    public virtual void UpdateCarUsage(string? carUsage)
    {
        SetCarUsage(carUsage);
    }

    public virtual void UpdateCarOld(decimal? carOld)
    {
        SetCarOld(carOld);
    }

    public virtual void UpdateCarProductionYear(DateTime? carProductionYear)
    {
        SetCarProductionYear(carProductionYear);
    }

    public virtual void UpdateCarPlate(string? carPlate)
    {
        SetCarPlate(carPlate);
    }

    public virtual void UpdateCarPlateType(string? carPlateType)
    {
        SetCarPlateType(carPlateType);
    }

    public virtual void UpdateCarPlateClear(string? carPlateClear)
    {
        SetCarPlateClear(carPlateClear);
    }

    public virtual void UpdateCarSeatNumber(decimal? carSeatNumber)
    {
        SetCarSeatNumber(carSeatNumber);
    }

    public virtual void UpdateCarVin(string? carVin)
    {
        SetCarVin(carVin);
    }

    public virtual void UpdateCarEngineNumber(string? carEngineNumber)
    {
        SetCarEngineNumber(carEngineNumber);
    }

    public virtual void UpdateCarPayloadCapacity(decimal? carPayloadCapacity)
    {
        SetCarPayloadCapacity(carPayloadCapacity);
    }

    public virtual void UpdateCarColor(string? carColor)
    {
        SetCarColor(carColor);
    }

    public virtual void UpdateCarOrigin(string? carOrigin)
    {
        SetCarOrigin(carOrigin);
    }

    public virtual void UpdateCarNew(string? carNew)
    {
        SetCarNew(carNew);
    }
}
