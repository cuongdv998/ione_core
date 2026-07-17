using System;
using System.Threading.Tasks;
using iOne.ProLineOfBusinesses;
using iOne.ResPartners;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProTableRates;

public class ProTableRateManager : DomainService
{
    protected IProTableRateRepository Repository { get; }
    protected IProLineOfBusinessRepository LobRepository { get; }
    protected IResPartnerRepository PartnerRepository { get; }

    public ProTableRateManager(
        IProTableRateRepository repository,
        IProLineOfBusinessRepository lobRepository,
        IResPartnerRepository partnerRepository)
    {
        Repository = repository;
        LobRepository = lobRepository;
        PartnerRepository = partnerRepository;
    }

    public virtual async Task CreateAsync(ProTableRate tableRate)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(tableRate.Code))
        {
            throw new BusinessException("ProTableRate:CodeExists")
                .WithData("Code", tableRate.Code);
        }

        // Validate LobId
        var lob = await LobRepository.FindAsync(tableRate.LobId);
        if (lob == null)
        {
            throw new BusinessException("ProTableRate:LobNotFound")
                .WithData("LobId", tableRate.LobId);
        }

        // Validate InsurerId if provided
        if (tableRate.InsurerId.HasValue)
        {
            var insurer = await PartnerRepository.FindAsync(tableRate.InsurerId.Value);
            if (insurer == null)
            {
                throw new BusinessException("ProTableRate:InsurerNotFound")
                    .WithData("InsurerId", tableRate.InsurerId.Value);
            }
        }

        await Repository.InsertAsync(tableRate);
    }

    public virtual async Task UpdateAsync(
        ProTableRate tableRate,
        string name,
        ProTableRateStatus status,
        Guid lobId,
        Guid? insurerId = null,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Validate LobId
        var lob = await LobRepository.FindAsync(lobId);
        if (lob == null)
        {
            throw new BusinessException("ProTableRate:LobNotFound")
                .WithData("LobId", lobId);
        }

        // Validate InsurerId if provided
        if (insurerId.HasValue)
        {
            var insurer = await PartnerRepository.FindAsync(insurerId.Value);
            if (insurer == null)
            {
                throw new BusinessException("ProTableRate:InsurerNotFound")
                    .WithData("InsurerId", insurerId.Value);
            }
        }

        tableRate.UpdateName(name);
        tableRate.UpdateStatus(status);
        tableRate.UpdateDescription(description);
        tableRate.UpdateLobId(lobId);
        tableRate.UpdateInsurerId(insurerId);
        await Repository.UpdateAsync(tableRate);
    }
}
