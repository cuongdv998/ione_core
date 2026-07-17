using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using iOne.ResProvinces;

namespace iOne.ResWards;

public class ResWardManager : DomainService
{
    protected IResWardRepository Repository { get; }
    protected IResProvinceRepository ProvinceRepository { get; }

    public ResWardManager(
        IResWardRepository repository,
        IResProvinceRepository provinceRepository)
    {
        Repository = repository;
        ProvinceRepository = provinceRepository;
    }

    public virtual async Task CreateAsync(ResWard ward)
    {
        // Check province exists
        var province = await ProvinceRepository.FindAsync(ward.ProvinceId);
        if (province == null)
        {
            throw new BusinessException("Master:ResWard:ProvinceNotFound")
                .WithData("ProvinceId", ward.ProvinceId);
        }

        // Check code uniqueness
        if (await Repository.FindByCodeAsync(ward.Code) != null)
        {
            throw new BusinessException("Master:ResWard:CodeExists")
                .WithData("Code", ward.Code);
        }

        await Repository.InsertAsync(ward);
    }

    public virtual async Task UpdateAsync(
        ResWard ward,
        Guid provinceId,
        string name,
        ResWardStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Check province exists (if changed)
        if (ward.ProvinceId != provinceId)
        {
            var province = await ProvinceRepository.FindAsync(provinceId);
            if (province == null)
            {
                throw new BusinessException("Master:ResWard:ProvinceNotFound")
                    .WithData("ProvinceId", provinceId);
            }
        }

        ward.UpdateProvinceId(provinceId);
        ward.UpdateName(name);
        ward.UpdateStatus(status);
        ward.UpdateDescription(description);
        await Repository.UpdateAsync(ward);
    }
}

