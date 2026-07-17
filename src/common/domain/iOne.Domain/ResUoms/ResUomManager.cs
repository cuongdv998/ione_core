using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResUoms;

public class ResUomManager : DomainService
{
    protected IResUomRepository Repository { get; }

    public ResUomManager(IResUomRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResUom uom)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(uom.Code))
        {
            throw new BusinessException("Master:ResUom:CodeExists")
                .WithData("Code", uom.Code);
        }

        await Repository.InsertAsync(uom);
    }

    public virtual async Task UpdateAsync(
        ResUom uom,
        string name,
        ResUomStatus status,
        decimal rounding,
        decimal? factor,
        ResUomType type)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code và classId - Code và ClassId không được phép sửa
        uom.UpdateName(name);
        uom.UpdateStatus(status);
        uom.UpdateRounding(rounding);
        uom.UpdateFactor(factor);
        uom.UpdateType(type);
        await Repository.UpdateAsync(uom);
    }

    public virtual async Task DeleteAsync(ResUom uom)
    {
        // Soft delete: Delete trước để trigger audit log, sau đó set Status = Deactive
        await Repository.DeleteAsync(uom);
        uom.UpdateStatus(ResUomStatus.Deactive);
        await Repository.UpdateAsync(uom);
    }

    public virtual async Task<ResUom> GetDetailAsync(Guid id)
    {
        return await Repository.GetAsync(id);
    }
}
