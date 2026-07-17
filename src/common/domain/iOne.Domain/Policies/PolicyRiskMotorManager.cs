using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyRiskMotorManager : DomainService
{
    protected IPolicyRiskMotorRepository Repository { get; }

    public PolicyRiskMotorManager(IPolicyRiskMotorRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyRiskMotor policyRiskMotor)
    {
        await Repository.InsertAsync(policyRiskMotor);
    }

    public virtual async Task UpdateAsync(
        PolicyRiskMotor policyRiskMotor,
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
    {
        if (policyRiskObjectId.HasValue)
        {
            policyRiskMotor.UpdatePolicyRiskObjectId(policyRiskObjectId);
        }
        
        if (riskObjectValue.HasValue)
        {
            policyRiskMotor.UpdateRiskObjectValue(riskObjectValue);
        }
        
        if (motorClassCode != null)
        {
            policyRiskMotor.UpdateMotorClassCode(motorClassCode);
        }
        
        if (carLineCode != null)
        {
            policyRiskMotor.UpdateCarLineCode(carLineCode);
        }
        
        if (carGroupCode != null)
        {
            policyRiskMotor.UpdateCarGroupCode(carGroupCode);
        }
        
        if (carTypeCode != null)
        {
            policyRiskMotor.UpdateCarTypeCode(carTypeCode);
        }
        
        if (carBrandCode != null)
        {
            policyRiskMotor.UpdateCarBrandCode(carBrandCode);
        }
        
        if (carModelCode != null)
        {
            policyRiskMotor.UpdateCarModelCode(carModelCode);
        }
        
        if (carCategoryCode != null)
        {
            policyRiskMotor.UpdateCarCategoryCode(carCategoryCode);
        }
        
        if (carUsage != null)
        {
            policyRiskMotor.UpdateCarUsage(carUsage);
        }
        
        if (carOld.HasValue)
        {
            policyRiskMotor.UpdateCarOld(carOld);
        }
        
        if (carProductionYear.HasValue)
        {
            policyRiskMotor.UpdateCarProductionYear(carProductionYear);
        }
        
        if (carPlate != null)
        {
            policyRiskMotor.UpdateCarPlate(carPlate);
        }

        if (carPlateType != null)
        {
            policyRiskMotor.UpdateCarPlateType(carPlateType);
        }
        
        if (carPlateClear != null)
        {
            policyRiskMotor.UpdateCarPlateClear(carPlateClear);
        }
        
        if (carSeatNumber.HasValue)
        {
            policyRiskMotor.UpdateCarSeatNumber(carSeatNumber);
        }
        
        if (carVin != null)
        {
            policyRiskMotor.UpdateCarVin(carVin);
        }
        
        if (carEngineNumber != null)
        {
            policyRiskMotor.UpdateCarEngineNumber(carEngineNumber);
        }
        
        if (carPayloadCapacity.HasValue)
        {
            policyRiskMotor.UpdateCarPayloadCapacity(carPayloadCapacity);
        }
        
        if (carColor != null)
        {
            policyRiskMotor.UpdateCarColor(carColor);
        }

        if (carOrigin != null)
        {
            policyRiskMotor.UpdateCarOrigin(carOrigin);
        }

        if (carNew != null)
        {
            policyRiskMotor.UpdateCarNew(carNew);
        }
        
        await Repository.UpdateAsync(policyRiskMotor);
    }
}
