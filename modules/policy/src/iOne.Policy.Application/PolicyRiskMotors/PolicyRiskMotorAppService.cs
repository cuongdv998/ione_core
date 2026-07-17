using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyRiskMotors;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyRiskMotors;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyRiskMotorPermissions.Default)]
public class PolicyRiskMotorAppService : CrudAppService<
    PolicyRiskMotor,
    PolicyRiskMotorDto,
    Guid,
    GetPolicyRiskMotorsInput,
    CreatePolicyRiskMotorDto,
    UpdatePolicyRiskMotorDto>, IPolicyRiskMotorAppService
{
    protected PolicyRiskMotorManager Manager { get; }
    protected IPolicyRiskMotorRepository PolicyRiskMotorRepository { get; }

    public PolicyRiskMotorAppService(
        IRepository<PolicyRiskMotor, Guid> repository,
        PolicyRiskMotorManager manager,
        IPolicyRiskMotorRepository policyRiskMotorRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyRiskMotorRepository = policyRiskMotorRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyRiskMotorPermissions.View;
        GetListPolicyName = PolicyRiskMotorPermissions.View;
        CreatePolicyName = PolicyRiskMotorPermissions.Create;
        UpdatePolicyName = PolicyRiskMotorPermissions.Edit;
        DeletePolicyName = PolicyRiskMotorPermissions.Delete;
    }

    public override async Task<PolicyRiskMotorDto> CreateAsync(CreatePolicyRiskMotorDto input)
    {
        var entity = new PolicyRiskMotor(
            GuidGenerator.Create(),
            input.PolicyRiskObjectId,
            input.RiskObjectValue,
            input.MotorClassCode,
            input.CarLineCode,
            input.CarGroupCode,
            input.CarTypeCode,
            input.CarBrandCode,
            input.CarModelCode,
            input.CarCategoryCode,
            input.CarUsage,
            input.CarOld,
            input.CarProductionYear,
            input.CarPlate,
            input.CarPlateType,
            input.CarPlateClear,
            input.CarSeatNumber,
            input.CarVin,
            input.CarEngineNumber,
            input.CarPayloadCapacity,
            input.CarColor,
            input.CarOrigin,
            input.CarNew
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyRiskMotor, PolicyRiskMotorDto>(entity);
    }

    public override async Task<PolicyRiskMotorDto> UpdateAsync(Guid id, UpdatePolicyRiskMotorDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdatePolicyRiskObjectId(input.PolicyRiskObjectId);
        entity.UpdateRiskObjectValue(input.RiskObjectValue);
        entity.UpdateMotorClassCode(input.MotorClassCode);
        entity.UpdateCarLineCode(input.CarLineCode);
        entity.UpdateCarGroupCode(input.CarGroupCode);
        entity.UpdateCarTypeCode(input.CarTypeCode);
        entity.UpdateCarBrandCode(input.CarBrandCode);
        entity.UpdateCarModelCode(input.CarModelCode);
        entity.UpdateCarCategoryCode(input.CarCategoryCode);
        entity.UpdateCarUsage(input.CarUsage);
        entity.UpdateCarOld(input.CarOld);
        entity.UpdateCarProductionYear(input.CarProductionYear);
        entity.UpdateCarPlate(input.CarPlate);
        entity.UpdateCarPlateType(input.CarPlateType);
        entity.UpdateCarPlateClear(input.CarPlateClear);
        entity.UpdateCarSeatNumber(input.CarSeatNumber);
        entity.UpdateCarVin(input.CarVin);
        entity.UpdateCarEngineNumber(input.CarEngineNumber);
        entity.UpdateCarPayloadCapacity(input.CarPayloadCapacity);
        entity.UpdateCarColor(input.CarColor);
        entity.UpdateCarOrigin(input.CarOrigin);
        entity.UpdateCarNew(input.CarNew);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyRiskMotor, PolicyRiskMotorDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<PolicyRiskMotor>> CreateFilteredQueryAsync(GetPolicyRiskMotorsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyRiskObjectId
        if (input.PolicyRiskObjectId.HasValue)
        {
            query = query.Where(x => x.PolicyRiskObjectId == input.PolicyRiskObjectId.Value);
        }

        // Filter by CarPlate (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.CarPlate))
        {
            query = query.Where(x => EF.Functions.ILike(x.CarPlate ?? "", $"%{input.CarPlate}%"));
        }

        // Filter by CarVin (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.CarVin))
        {
            query = query.Where(x => EF.Functions.ILike(x.CarVin ?? "", $"%{input.CarVin}%"));
        }

        return query;
    }
}
