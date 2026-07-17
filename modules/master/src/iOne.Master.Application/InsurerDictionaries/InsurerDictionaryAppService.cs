using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iOne.AdminConfigs;
using iOne.Master;
using iOne.Master.Helpers;
using iOne.Master.InsurerDictionaries;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.InsurerDictionaries;
using iOne.ResPartners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.InsurerDictionaries;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(InsurerDictionaryPermissions.Default)]
public class InsurerDictionaryAppService : CrudAppService<
    InsurerDictionary,
    InsurerDictionaryDto,
    Guid,
    GetInsurerDictionariesInput,
    CreateInsurerDictionaryDto,
    UpdateInsurerDictionaryDto>, IInsurerDictionaryAppService
{
    protected InsurerDictionaryManager Manager { get; }
    protected IInsurerDictionaryRepository InsurerDictionaryRepository { get; }
    protected IAdminConfigRepository AdminConfigRepository { get; }
    protected IResPartnerRepository ResPartnerRepository { get; }

    private const string TemplateSheetName = "Danh mục Map bảo hiểm";
    private const string ColStt = "STT";
    private const string ColBusinessName = "Tên bảng dữ liệu";
    private const string ColInsurerCode = "Công ty bảo hiểm (Mã)";
    private const string ColOwnCode = "Mã nội bộ";
    private const string ColInsurerMappingCode = "Mã công ty bảo hiểm";
    private const string ColEffectDate = "Ngày hiệu lực (dd/MM/yyyy)";
    private const string ColExpireDate = "Ngày hết hạn (dd/MM/yyyy)";
    private const string ColError = "Lỗi";
    private static readonly string[] TemplateHeaders = { ColStt, ColBusinessName, ColInsurerCode, ColOwnCode, ColInsurerMappingCode, ColEffectDate, ColExpireDate };
    private static readonly CultureInfo DateCulture = new("vi-VN");
    private static readonly string[] DateFormats = { "dd/MM/yyyy", "yyyy-MM-dd", "d/M/yyyy", "dd-MM-yyyy" };

    public InsurerDictionaryAppService(
        IInsurerDictionaryRepository repository,
        InsurerDictionaryManager manager,
        IAdminConfigRepository adminConfigRepository,
        IResPartnerRepository resPartnerRepository)
        : base(repository)
    {
        Manager = manager;
        InsurerDictionaryRepository = repository;
        AdminConfigRepository = adminConfigRepository;
        ResPartnerRepository = resPartnerRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = InsurerDictionaryPermissions.View;
        GetListPolicyName = InsurerDictionaryPermissions.View;
        CreatePolicyName = InsurerDictionaryPermissions.Create;
        UpdatePolicyName = InsurerDictionaryPermissions.Edit;
        DeletePolicyName = InsurerDictionaryPermissions.Delete;
    }

    public override async Task<InsurerDictionaryDto> CreateAsync(CreateInsurerDictionaryDto input)
    {
        if (await HasOverlapAsync(
                input.BusinessName,
                input.InsurerId,
                input.OwnCode,
                input.EffectDate,
                input.ExpireDate,
                excludeId: null))
        {
            throw new UserFriendlyException(L["InsurerDictionary:OverlapEffectExpire"]);
        }

        var entity = new InsurerDictionary(
            GuidGenerator.Create(),
            input.BusinessName,
            input.InsurerId,
            input.OwnCode,
            input.InsurerCode,
            input.ExtraData,
            input.Status,
            input.EffectDate,
            input.ExpireDate
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<InsurerDictionary, InsurerDictionaryDto>(entity);
    }

    public override async Task<InsurerDictionaryDto> UpdateAsync(Guid id, UpdateInsurerDictionaryDto input)
    {
        var entity = await Repository.GetAsync(id);

        if (await HasOverlapAsync(
                entity.BusinessName,
                entity.InsurerId,
                entity.OwnCode,
                input.EffectDate,
                input.ExpireDate,
                excludeId: id))
        {
            throw new UserFriendlyException(L["InsurerDictionary:OverlapEffectExpire"]);
        }

        await Manager.UpdateAsync(
            entity,
            input.InsurerCode,
            input.ExtraData,
            input.Status,
            input.EffectDate,
            input.ExpireDate
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<InsurerDictionary, InsurerDictionaryDto>(entity!);
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
        entity!.UpdateStatus(InsurerDictionaryStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<InsurerDictionary>> CreateFilteredQueryAsync(GetInsurerDictionariesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by BusinessName (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.BusinessName))
        {
            query = query.Where(x => EF.Functions.ILike(x.BusinessName, $"%{input.BusinessName}%"));
        }

        if (input.InsurerId.HasValue)
        {
            query = query.Where(x => x.InsurerId == input.InsurerId.Value);
        }

        // Filter by OwnCode (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.OwnCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.OwnCode, $"%{input.OwnCode}%"));
        }

        // Filter by InsurerCode (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.InsurerCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.InsurerCode, $"%{input.InsurerCode}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }

    /// <summary>
    /// Checks if there is an overlapping record for (businessName, insurerId, ownCode) with [effectDate, expireDate].
    /// Overlap: existing.EffectDate &lt;= (expireDate ?? max) AND effectDate &lt;= (existing.ExpireDate ?? max). Null ExpireDate means no end.
    /// </summary>
    private async Task<bool> HasOverlapAsync(
        string businessName,
        Guid insurerId,
        string ownCode,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? excludeId)
    {
        var expireDateValue = expireDate ?? DateTime.MaxValue;
        var queryable = await ReadOnlyRepository.GetQueryableAsync();
        var overlapQuery = queryable
            .Where(x =>
                x.BusinessName == businessName
                && x.InsurerId == insurerId
                && x.OwnCode == ownCode
                && x.EffectDate <= expireDateValue
                && effectDate <= (x.ExpireDate ?? DateTime.MaxValue));

        if (excludeId.HasValue)
        {
            overlapQuery = overlapQuery.Where(x => x.Id != excludeId.Value);
        }

        return await AsyncExecuter.AnyAsync(overlapQuery);
    }

    public virtual async Task<byte[]> ExportTemplateAsync()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(TemplateSheetName);

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < TemplateHeaders.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = TemplateHeaders[i];
        }

        // Sample row with date format dd/MM/yyyy
        worksheet.Cell(2, 1).Value = 1;
        worksheet.Cell(2, 2).Value = "";
        worksheet.Cell(2, 3).Value = "";
        worksheet.Cell(2, 4).Value = "";
        worksheet.Cell(2, 5).Value = "";
        worksheet.Cell(2, 6).Value = "01/01/2024";
        worksheet.Cell(2, 7).Value = "31/12/2024";
        worksheet.Row(2).Style.Font.Italic = true;
        worksheet.Row(2).Style.Font.FontColor = XLColor.Gray;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public virtual async Task<ImportInsurerDictionaryResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportInsurerDictionaryResultDto();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            throw new UserFriendlyException(L["InsurerDictionary:ExcelFileInvalid"]);
        }

        // Build header index (row 1)
        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= 15; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                headers[headerValue] = col;
            }
        }

        // Require columns
        var required = new[] { ColBusinessName, ColInsurerCode, ColOwnCode, ColInsurerMappingCode, ColEffectDate };
        foreach (var h in required)
        {
            if (!headers.ContainsKey(h))
            {
                throw new UserFriendlyException(L["InsurerDictionary:ExcelMissingHeader", h]);
            }
        }

        int errorCol = TemplateHeaders.Length + 1;
        if (!headers.ContainsKey(ColError))
        {
            worksheet.Cell(1, errorCol).Value = ColError;
            worksheet.Cell(1, errorCol).Style.Font.Bold = true;
            worksheet.Cell(1, errorCol).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        else
        {
            errorCol = headers[ColError];
        }

        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            string? errorMessage = null;

            var businessName = row.GetCellValue(headers[ColBusinessName]).Trim();
            var insurerCodeInput = row.GetCellValue(headers[ColInsurerCode]).Trim();
            var ownCode = row.GetCellValue(headers[ColOwnCode]).Trim();
            var insurerMappingCode = row.GetCellValue(headers[ColInsurerMappingCode]).Trim();
            var effectDateText = row.GetCellValue(headers[ColEffectDate]).Trim();
            var expireDateText = headers.ContainsKey(ColExpireDate) ? row.GetCellValue(headers[ColExpireDate]).Trim() : "";

            // Validate required
            if (string.IsNullOrWhiteSpace(businessName))
                errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:BusinessNameRequired"].Value;
            if (string.IsNullOrWhiteSpace(insurerCodeInput))
                errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:InsurerIdRequired"].Value;
            if (string.IsNullOrWhiteSpace(ownCode))
                errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:OwnCodeRequired"].Value;
            if (string.IsNullOrWhiteSpace(insurerMappingCode))
                errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:InsurerCodeRequired"].Value;

            Guid? insurerId = null;
            if (!string.IsNullOrWhiteSpace(insurerCodeInput))
            {
                var partnerQuery = await ResPartnerRepository.GetQueryableAsync();
                var partner = await AsyncExecuter.FirstOrDefaultAsync(
                    partnerQuery
                        .Include(x => x.PartnerType)
                        .Where(x => x.Code == insurerCodeInput && x.PartnerType != null && x.PartnerType.Code == "INSURER"));
                if (partner != null)
                    insurerId = partner.Id;
                else
                    errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:InsurerNotFoundByCode", insurerCodeInput].Value;
            }

            if (string.IsNullOrWhiteSpace(errorMessage) && !string.IsNullOrWhiteSpace(businessName))
            {
                var config = await AdminConfigRepository.FindByCodeSubCodeAsync("TABLE_MAPPING_INSURER", businessName);
                if (config == null)
                    errorMessage = L["InsurerDictionary:BusinessNameInvalid", businessName].Value;
            }

            DateTime effectDate = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(effectDateText))
                errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:EffectDateRequired"].Value;
            else if (!DateTime.TryParseExact(effectDateText, DateFormats, DateCulture, DateTimeStyles.None, out effectDate))
                errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:EffectDateInvalid"].Value;

            DateTime? expireDate = null;
            if (!string.IsNullOrWhiteSpace(expireDateText))
            {
                if (DateTime.TryParseExact(expireDateText, DateFormats, DateCulture, DateTimeStyles.None, out var parsed))
                {
                    expireDate = parsed;
                    if (expireDate.Value < effectDate)
                        errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:ExpireDateMustBeAfterEffectDate"].Value;
                }
                else
                    errorMessage = (errorMessage != null ? errorMessage + "; " : "") + L["InsurerDictionary:ExpireDateInvalid"].Value;
            }

            if (string.IsNullOrWhiteSpace(errorMessage) && insurerId.HasValue)
            {
                try
                {
                    if (await HasOverlapAsync(businessName, insurerId.Value, ownCode, effectDate, expireDate, excludeId: null))
                        errorMessage = L["InsurerDictionary:OverlapEffectExpire"].Value;
                    else
                    {
                        var entity = new InsurerDictionary(
                            GuidGenerator.Create(),
                            businessName,
                            insurerId.Value,
                            ownCode,
                            insurerMappingCode,
                            null,
                            InsurerDictionaryStatus.Active,
                            effectDate,
                            expireDate);
                        await Manager.CreateAsync(entity);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }

            if (errorMessage != null)
            {
                result.ErrorCount++;
                worksheet.Cell(rowNumber, errorCol).Value = errorMessage;
            }

            rowNumber++;
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        result.FileName = $"InsurerDictionary_ImportResult_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        using var outStream = new MemoryStream();
        workbook.SaveAs(outStream);
        result.FileBase64 = Convert.ToBase64String(outStream.ToArray());

        return result;
    }
}
