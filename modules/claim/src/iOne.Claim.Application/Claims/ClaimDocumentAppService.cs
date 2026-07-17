using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using iOne.Claim.Permissions;
using iOne.ClaimAdjustAtLocations;
using iOne.ClaimDocuments;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using iOne.WorkTasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Claim.Claims;

[Authorize(ClaimPermissions.Default)]
public class ClaimDocumentAppService : ApplicationService, IClaimDocumentAppService
{
    private const string OnsiteImageGroupCode = "CAR_ASSESSMENT_IMAGE";

    protected IRepository<ClaimDocument, Guid> ClaimDocumentRepository { get; }
    protected IRepository<ResDocument, Guid> ResDocumentRepository { get; }
    protected IRepository<ResDocumentType, Guid> ResDocumentTypeRepository { get; }
    protected IRepository<WorkTask, Guid> WorkTaskRepository { get; }
    protected IRepository<ClaimAdjustAtLocation, Guid> ClaimAdjustAtLocationRepository { get; }

    public ClaimDocumentAppService(
        IRepository<ClaimDocument, Guid> claimDocumentRepository,
        IRepository<ResDocument, Guid> resDocumentRepository,
        IRepository<ResDocumentType, Guid> resDocumentTypeRepository,
        IRepository<WorkTask, Guid> workTaskRepository,
        IRepository<ClaimAdjustAtLocation, Guid> claimAdjustAtLocationRepository)
    {
        ClaimDocumentRepository = claimDocumentRepository;
        ResDocumentRepository = resDocumentRepository;
        ResDocumentTypeRepository = resDocumentTypeRepository;
        WorkTaskRepository = workTaskRepository;
        ClaimAdjustAtLocationRepository = claimAdjustAtLocationRepository;
    }

    public virtual async Task<PagedResultDto<ClaimDocumentDto>> GetListAsync(GetClaimDocumentsInput input)
    {
        if (input.ClaimId == Guid.Empty)
        {
            throw new UserFriendlyException(L["Claim:ClaimIdRequired"]);
        }

        var query = await ClaimDocumentRepository.GetQueryableAsync();
        query = query.Where(d => d.ClaimId == input.ClaimId);

        if (input.AdjustAtLocationId.HasValue)
        {
            query = query.Where(d => d.AdjustAtLocationId == input.AdjustAtLocationId);
        }

        query = query.OrderByDescending(d => d.CreationTime);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var docs = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 50));

        var documentIds = docs.Where(x => x.DocumentId.HasValue).Select(x => x.DocumentId!.Value).Distinct().ToList();
        var resDocsQuery = await ResDocumentRepository.GetQueryableAsync();
        var resDocs = await AsyncExecuter.ToListAsync(resDocsQuery.Where(r => documentIds.Contains(r.Id)));
        var resDocById = resDocs.ToDictionary(r => r.Id, r => r);

        var resultItems = docs.Select(d =>
        {
            resDocById.TryGetValue(d.DocumentId ?? Guid.Empty, out var rd);
            return new ClaimDocumentDto
            {
                Id = d.Id,
                ClaimId = d.ClaimId,
                ClaimFolderId = d.ClaimFolderId,
                AdjustAtLocationId = d.AdjustAtLocationId,
                DocumentId = d.DocumentId ?? Guid.Empty,
                FileName = rd?.FileName ?? string.Empty,
                Url = rd?.Url,
                ThumbnailUrl = rd?.ThumbnailUrl,
                MimeType = rd?.MimeType ?? string.Empty,
                Note = d.Note,
                Complete = d.Complete,
                IsCopy = d.IsCopy,
                IssueDate = d.IssueDate,
                DocumentGroupCode = rd?.GroupCode
            };
        }).ToList();

        return new PagedResultDto<ClaimDocumentDto>(totalCount, resultItems);
    }

    public virtual async Task<List<ClaimDocumentDto>> GetOnsiteImagesAsync(GetClaimOnsiteImagesInput input)
    {
        if (input.ClaimId == Guid.Empty)
        {
            throw new UserFriendlyException(L["Claim:ClaimIdRequired"]);
        }

        var adjust = await ResolveOnsiteAdjustAsync(input.ClaimId, input.WorkTaskId);
        if (adjust == null)
        {
            return new List<ClaimDocumentDto>();
        }

        var claimDocs = await AsyncExecuter.ToListAsync(
            (await ClaimDocumentRepository.GetQueryableAsync())
            .Where(x =>
                x.ClaimId == input.ClaimId &&
                x.AdjustAtLocationId == adjust.Id &&
                x.DocumentId.HasValue)
            .OrderByDescending(x => x.CreationTime));

        var documentIds = claimDocs
            .Where(x => x.DocumentId.HasValue)
            .Select(x => x.DocumentId!.Value)
            .Distinct()
            .ToList();
        if (documentIds.Count == 0)
        {
            return new List<ClaimDocumentDto>();
        }

        var resDocs = await AsyncExecuter.ToListAsync(
            (await ResDocumentRepository.GetQueryableAsync()).Where(x => documentIds.Contains(x.Id)));
        var resDocById = resDocs.ToDictionary(x => x.Id, x => x);

        var typeIds = claimDocs
            .Where(x => x.DocumentTypeId.HasValue)
            .Select(x => x.DocumentTypeId!.Value)
            .Concat(resDocs.Select(x => x.DocumentTypeId))
            .Distinct()
            .ToList();
        var typeById = typeIds.Count == 0
            ? new Dictionary<Guid, ResDocumentType>()
            : (await AsyncExecuter.ToListAsync(
                (await ResDocumentTypeRepository.GetQueryableAsync()).Where(x => typeIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => x);

        return claimDocs
            .Select(claimDoc =>
            {
                if (!claimDoc.DocumentId.HasValue || !resDocById.TryGetValue(claimDoc.DocumentId.Value, out var resDoc))
                {
                    return null;
                }

                var documentTypeId = claimDoc.DocumentTypeId ?? resDoc.DocumentTypeId;
                typeById.TryGetValue(documentTypeId, out var docType);
                if (!IsOnsiteImageDocument(docType, resDoc))
                {
                    return null;
                }

                return new ClaimDocumentDto
                {
                    Id = claimDoc.Id,
                    ClaimId = claimDoc.ClaimId,
                    ClaimFolderId = claimDoc.ClaimFolderId,
                    AdjustAtLocationId = claimDoc.AdjustAtLocationId,
                    DocumentId = claimDoc.DocumentId ?? Guid.Empty,
                    FileName = resDoc.FileName ?? string.Empty,
                    Url = resDoc.Url,
                    ThumbnailUrl = resDoc.ThumbnailUrl,
                    MimeType = resDoc.MimeType ?? string.Empty,
                    Note = claimDoc.Note,
                    Complete = claimDoc.Complete,
                    IsCopy = claimDoc.IsCopy,
                    IssueDate = claimDoc.IssueDate,
                    DocumentGroupCode = resDoc.GroupCode
                };
            })
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();
    }

    private async Task<ClaimAdjustAtLocation?> ResolveOnsiteAdjustAsync(Guid claimId, Guid? workTaskId)
    {
        if (workTaskId.HasValue && workTaskId.Value != Guid.Empty)
        {
            var workTask = await WorkTaskRepository.FirstOrDefaultAsync(x => x.Id == workTaskId.Value);
            if (workTask != null && string.Equals(workTask.BusinessCode, "CLAIM_ONSITE_ASSESSMENT", StringComparison.OrdinalIgnoreCase))
            {
                var adjust = await ClaimAdjustAtLocationRepository.FirstOrDefaultAsync(x => x.Id == workTask.BusinessKey);
                if (adjust != null && adjust.ClaimId == claimId)
                {
                    return adjust;
                }
            }
        }

        return await AsyncExecuter.FirstOrDefaultAsync(
            (await ClaimAdjustAtLocationRepository.GetQueryableAsync())
            .Where(x => x.ClaimId == claimId)
            .OrderByDescending(x => x.CreationTime));
    }

    private static bool IsOnsiteImageDocument(ResDocumentType? docType, ResDocument resDoc)
    {
        return string.Equals(docType?.Code, OnsiteImageGroupCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(docType?.DocumentGroupCode, OnsiteImageGroupCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(resDoc.GroupCode, OnsiteImageGroupCode, StringComparison.OrdinalIgnoreCase);
    }
}
