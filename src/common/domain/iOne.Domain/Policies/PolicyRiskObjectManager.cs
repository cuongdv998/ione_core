using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyRiskObjectManager : DomainService
{
    protected IPolicyRiskObjectRepository Repository { get; }

    public PolicyRiskObjectManager(IPolicyRiskObjectRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyRiskObject policyRiskObject)
    {
        await Repository.InsertAsync(policyRiskObject);
    }

    public virtual async Task UpdateAsync(
        PolicyRiskObject policyRiskObject,
        string? repName = null,
        string? repIdNo = null,
        string? repPassport = null,
        string? repPhone = null,
        string? repEmail = null,
        Guid? repProvinceId = null,
        Guid? repWardId = null,
        string? repAddress = null,
        string? repFullAddress = null,
        Guid? riskObjectProvinceId = null,
        Guid? riskObjectWardId = null,
        string? riskObjectAddress = null,
        string? riskObjectFullAddress = null,
        double? riskObjectLat = null,
        double? riskObjectLong = null)
    {
        if (repName != null)
        {
            policyRiskObject.UpdateRepName(repName);
        }
        
        if (repIdNo != null)
        {
            policyRiskObject.UpdateRepIdNo(repIdNo);
        }
        
        if (repPassport != null)
        {
            policyRiskObject.UpdateRepPassport(repPassport);
        }
        
        if (repPhone != null)
        {
            policyRiskObject.UpdateRepPhone(repPhone);
        }
        
        if (repEmail != null)
        {
            policyRiskObject.UpdateRepEmail(repEmail);
        }
        
        if (repProvinceId.HasValue)
        {
            policyRiskObject.UpdateRepProvinceId(repProvinceId);
        }
        
        if (repWardId.HasValue)
        {
            policyRiskObject.UpdateRepWardId(repWardId);
        }
        
        if (repAddress != null)
        {
            policyRiskObject.UpdateRepAddress(repAddress);
        }
        
        if (repFullAddress != null)
        {
            policyRiskObject.UpdateRepFullAddress(repFullAddress);
        }
        
        if (riskObjectProvinceId.HasValue)
        {
            policyRiskObject.UpdateRiskObjectProvinceId(riskObjectProvinceId);
        }
        
        if (riskObjectWardId.HasValue)
        {
            policyRiskObject.UpdateRiskObjectWardId(riskObjectWardId);
        }
        
        if (riskObjectAddress != null)
        {
            policyRiskObject.UpdateRiskObjectAddress(riskObjectAddress);
        }
        
        if (riskObjectFullAddress != null)
        {
            policyRiskObject.UpdateRiskObjectFullAddress(riskObjectFullAddress);
        }
        
        if (riskObjectLat.HasValue)
        {
            policyRiskObject.UpdateRiskObjectLat(riskObjectLat);
        }
        
        if (riskObjectLong.HasValue)
        {
            policyRiskObject.UpdateRiskObjectLong(riskObjectLong);
        }
        
        await Repository.UpdateAsync(policyRiskObject);
    }
}
