using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResUomClasses;

public class ResUomClassManager : DomainService
{
    protected IResUomClassRepository Repository { get; }

    public ResUomClassManager(IResUomClassRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResUomClass uomClass)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(uomClass.Code))
        {
            throw new BusinessException("Master:ResUomClass:CodeExists")
                .WithData("Code", uomClass.Code);
        }

        await Repository.InsertAsync(uomClass);
    }

    public virtual async Task UpdateAsync(ResUomClass uomClass, string name, ResUomClassStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        uomClass.UpdateName(name);
        uomClass.UpdateStatus(status);
        await Repository.UpdateAsync(uomClass);
    }

    public virtual async Task DeleteAsync(ResUomClass uomClass)
    {
        // Soft delete: Delete trước để trigger audit log, sau đó set Status = Deactive
        await Repository.DeleteAsync(uomClass);
        uomClass.UpdateStatus(ResUomClassStatus.Deactive);
        await Repository.UpdateAsync(uomClass);
    }

    public virtual async Task<ResUomClass> GetDetailAsync(Guid id)
    {
        return await Repository.GetAsync(id);
    }

    public virtual async Task<List<ResUomClass>> GetListAsync()
    {
        return await Repository.GetListAsync();
    }
}

