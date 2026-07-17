using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResPartners;

public class ResPartnerManager : DomainService
{
    protected IResPartnerRepository Repository { get; }

    public ResPartnerManager(IResPartnerRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResPartner partner)
    {
        // Validate Code uniqueness (if Code is provided)
        if (!string.IsNullOrWhiteSpace(partner.Code))
        {
            if (await Repository.IsCodeExistsAsync(partner.Code))
            {
                throw new BusinessException("Partner:ResPartner:CodeExists")
                    .WithData("Code", partner.Code);
            }
        }

        await Repository.InsertAsync(partner);
    }

    public virtual async Task UpdateAsync(ResPartner partner)
    {
        // ⚠️ QUAN TRỌNG: Không validate Code uniqueness khi update vì Code immutable
        await Repository.UpdateAsync(partner);
    }

    /// <summary>
    /// Validate Agreements date ranges - cùng AgreementTermId không được overlap
    /// </summary>
    public virtual void ValidateAgreementsAsync(
        Guid partnerId,
        List<ResPartnerAgreement> newAgreements,
        List<ResPartnerAgreement> existingAgreements)
    {
        // Group by AgreementTermId
        var allAgreements = newAgreements.Concat(existingAgreements).ToList();
        var agreementsByTerm = allAgreements.GroupBy(a => a.AgreementTermId);

        foreach (var termGroup in agreementsByTerm)
        {
            var agreements = termGroup.ToList();

            // Check overlap for each pair
            for (int i = 0; i < agreements.Count; i++)
            {
                for (int j = i + 1; j < agreements.Count; j++)
                {
                    var agreement1 = agreements[i];
                    var agreement2 = agreements[j];

                    if (IsDateRangeOverlap(
                        agreement1.EffectDate,
                        agreement1.ExpireDate,
                        agreement2.EffectDate,
                        agreement2.ExpireDate))
                    {
                        throw new BusinessException("Partner:ResPartner:AgreementDateOverlap")
                            .WithData("AgreementTermId", termGroup.Key);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if two date ranges overlap
    /// </summary>
    private bool IsDateRangeOverlap(
        DateTime? start1,
        DateTime end1,
        DateTime? start2,
        DateTime end2)
    {
        // If either range has no start date, consider it starts from DateTime.MinValue
        var actualStart1 = start1 ?? DateTime.MinValue;
        var actualStart2 = start2 ?? DateTime.MinValue;

        // Two ranges overlap if: start1 <= end2 && start2 <= end1
        return actualStart1 <= end2 && actualStart2 <= end1;
    }

    /// <summary>
    /// Compute FullAddress từ Address + Ward Name + Province Name
    /// </summary>
    public virtual string ComputeFullAddress(string address, string wardName, string provinceName)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));
        }

        var parts = new List<string> { address.Trim() };

        if (!string.IsNullOrWhiteSpace(wardName))
        {
            parts.Add(wardName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(provinceName))
        {
            parts.Add(provinceName.Trim());
        }

        var fullAddress = string.Join(", ", parts);

        if (fullAddress.Length > 500)
        {
            throw new ArgumentException("Computed FullAddress exceeds 500 characters.", nameof(fullAddress));
        }

        return fullAddress;
    }
}

