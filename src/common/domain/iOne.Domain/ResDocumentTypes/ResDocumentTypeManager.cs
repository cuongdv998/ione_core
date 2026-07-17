using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResDocumentTypes;

public class ResDocumentTypeManager : DomainService
{
    protected IResDocumentTypeRepository Repository { get; }

    public ResDocumentTypeManager(IResDocumentTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResDocumentType documentType)
    {
        // Check code uniqueness
        if (await Repository.FindByCodeAsync(documentType.Code) != null)
        {
            throw new BusinessException("Master:ResDocumentType:CodeExists")
                .WithData("Code", documentType.Code);
        }

        await Repository.InsertAsync(documentType);
    }

    public virtual async Task UpdateAsync(
        ResDocumentType documentType,
        string name,
        ResDocumentTypeStatus status,
        string? description = null,
        string? bucket = null,
        string? documentGroupCode = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        documentType.UpdateName(name);
        documentType.UpdateStatus(status);
        documentType.UpdateDescription(description);
        documentType.UpdateBucket(bucket);
        documentType.UpdateDocumentGroupCode(documentGroupCode);
        await Repository.UpdateAsync(documentType);
    }

    public virtual async Task<ResDocumentType> GetByCode(string code) => await Repository.FindByCodeAsync(code);

    public virtual async Task<List<ResDocumentType>> GetListByDocumentGroupCodeAsync(string documentGroupCode) =>
        await Repository.GetListByDocumentGroupCodeAsync(documentGroupCode);
}
