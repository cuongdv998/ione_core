using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResObjectItemTypes;
using iOne.ResObjectItemTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResObjectItemTypes;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResObjectItemTypePermissions.Default)]
public class ResObjectItemTypeAppService : CrudAppService<
    ResObjectItemType,
    ResObjectItemTypeDto,
    Guid,
    GetResObjectItemTypesInput,
    CreateResObjectItemTypeDto,
    UpdateResObjectItemTypeDto>, IResObjectItemTypeAppService
{
    protected ResObjectItemTypeManager Manager { get; }
    protected IResObjectItemTypeRepository ObjectItemTypeRepository { get; }

    public ResObjectItemTypeAppService(
        IResObjectItemTypeRepository repository,
        ResObjectItemTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        ObjectItemTypeRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResObjectItemTypePermissions.View;
        GetListPolicyName = ResObjectItemTypePermissions.View;
        CreatePolicyName = ResObjectItemTypePermissions.Create;
        UpdatePolicyName = ResObjectItemTypePermissions.Edit;
        DeletePolicyName = ResObjectItemTypePermissions.Delete;
    }

    public override async Task<ResObjectItemTypeDto> CreateAsync(CreateResObjectItemTypeDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await ObjectItemTypeRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResObjectItemType:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        var entity = new ResObjectItemType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResObjectItemType, ResObjectItemTypeDto>(entity);
    }

    public override async Task<ResObjectItemTypeDto> UpdateAsync(Guid id, UpdateResObjectItemTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResObjectItemType, ResObjectItemTypeDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ResObjectItemTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResObjectItemType>> CreateFilteredQueryAsync(GetResObjectItemTypesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }

    public virtual Task<byte[]> ExportTemplateAsync()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Nhóm vật phẩm");

        // Set header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Set headers matching the import format
        worksheet.Cell(1, 1).Value = "Mã";
        worksheet.Cell(1, 2).Value = "Tên";
        worksheet.Cell(1, 3).Value = "Mô tả";
        worksheet.Cell(1, 4).Value = "Trạng thái";

        // Add sample data row (row 2) with example values
        worksheet.Cell(2, 1).Value = "TYPE001";
        worksheet.Cell(2, 2).Value = "Tên nhóm vật phẩm";
        worksheet.Cell(2, 3).Value = "Mô tả";
        worksheet.Cell(2, 4).Value = "Hoạt động";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
