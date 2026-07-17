using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iOne.Master;
using iOne.Master.Helpers;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResObjectItemDepreciations;
using iOne.ResObjectItemDepreciations;
using iOne.ResObjectTypeItems;
using iOne.ResCarGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;

namespace iOne.Master.ResObjectItemDepreciations;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResObjectItemDepreciationPermissions.Default)]
public class ResObjectItemDepreciationAppService : CrudAppService<
    ResObjectItemDepreciation,
    ResObjectItemDepreciationDto,
    Guid,
    GetResObjectItemDepreciationsInput,
    CreateResObjectItemDepreciationDto,
    UpdateResObjectItemDepreciationDto>, IResObjectItemDepreciationAppService
{
    protected ResObjectItemDepreciationManager Manager { get; }
    protected IResObjectItemDepreciationRepository DepreciationRepository { get; }
    protected IResObjectTypeItemRepository ObjectTypeItemRepository { get; }
    protected IResCarGroupRepository CarGroupRepository { get; }

    public ResObjectItemDepreciationAppService(
        IResObjectItemDepreciationRepository repository,
        ResObjectItemDepreciationManager manager,
        IResObjectTypeItemRepository objectTypeItemRepository,
        IResCarGroupRepository carGroupRepository)
        : base(repository)
    {
        Manager = manager;
        DepreciationRepository = repository;
        ObjectTypeItemRepository = objectTypeItemRepository;
        CarGroupRepository = carGroupRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResObjectItemDepreciationPermissions.View;
        GetListPolicyName = ResObjectItemDepreciationPermissions.View;
        CreatePolicyName = ResObjectItemDepreciationPermissions.Create;
        UpdatePolicyName = ResObjectItemDepreciationPermissions.Edit;
        DeletePolicyName = ResObjectItemDepreciationPermissions.Delete;
    }

    public override async Task<ResObjectItemDepreciationDto> CreateAsync(CreateResObjectItemDepreciationDto input)
    {
        // Validate ObjectTypeItemId exists
        var objectTypeItem = await ObjectTypeItemRepository.FindAsync(input.ObjectTypeItemId);
        if (objectTypeItem == null)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:ObjectTypeItemNotFound"].Value
                );
            //throw new BusinessException("Master:ResObjectItemDepreciation:ObjectTypeItemNotFound")
            //    .WithData("ObjectTypeItemId", input.ObjectTypeItemId);
        }

        // Validate CarGroupId exists
        var carGroup = await CarGroupRepository.FindAsync(input.CarGroupId);
        if (carGroup == null)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:CarGroupNotFound"].Value
                );
            //throw new BusinessException("Master:ResObjectItemDepreciation:CarGroupNotFound")
            //    .WithData("CarGroupId", input.CarGroupId);
        }

        // Validate date range
        if (input.ExpireDate.HasValue && input.ExpireDate.Value <= input.EffectDate)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:ExpireDateMustBeAfterEffectDate"].Value
                );
        }

        // Validate used time range
        if (input.UsedTimeFrom > input.UsedTimeTo)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:UsedTimeFromMustBeLessThanOrEqualTo"].Value
                );
        }

        var entity = new ResObjectItemDepreciation(
            GuidGenerator.Create(),
            input.ObjectTypeItemId,
            input.CarGroupId,
            input.UsedTimeFrom,
            input.UsedTimeTo,
            input.DepreciationPercent,
            input.EffectDate,
            input.ExpireDate,
            input.Status
        );

        await Manager.CreateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Load entity with navigation properties for mapping
        var query = await DepreciationRepository.GetQueryableAsync();
        var entityWithNav = await query
            .Include(x => x.ObjectTypeItemNavigation)
            .Include(x => x.CarGroupNavigation)
            .FirstOrDefaultAsync(x => x.Id == entity.Id);

        if (entityWithNav == null)
        {
            throw new EntityNotFoundException(typeof(ResObjectItemDepreciation), entity.Id);
        }

        return await MapToGetOutputDtoAsync(entityWithNav);
    }

    public override async Task<ResObjectItemDepreciationDto> UpdateAsync(Guid id, UpdateResObjectItemDepreciationDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // Validate ObjectTypeItemId exists
        var objectTypeItem = await ObjectTypeItemRepository.FindAsync(input.ObjectTypeItemId);
        if (objectTypeItem == null)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:ObjectTypeItemNotFound"].Value
                );
        }

        // Validate CarGroupId exists
        var carGroup = await CarGroupRepository.FindAsync(input.CarGroupId);
        if (carGroup == null)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:CarGroupNotFound"].Value
                );
        }

        // Validate date range
        if (input.ExpireDate.HasValue && input.ExpireDate.Value <= input.EffectDate)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:ExpireDateMustBeAfterEffectDate"].Value
                );
        }

        // Validate used time range
        if (input.UsedTimeFrom > input.UsedTimeTo)
        {
            throw new UserFriendlyException(
                    L["ResObjectItemDepreciation:UsedTimeFromMustBeLessThanOrEqualTo"].Value
                );
        }

        await Manager.UpdateAsync(
            entity,
            input.ObjectTypeItemId,
            input.CarGroupId,
            input.UsedTimeFrom,
            input.UsedTimeTo,
            input.DepreciationPercent,
            input.EffectDate,
            input.ExpireDate,
            input.Status
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        // Reload entity with navigation properties for mapping
        var query = await DepreciationRepository.GetQueryableAsync();
        var entityWithNav = await query
            .Include(x => x.ObjectTypeItemNavigation)
            .Include(x => x.CarGroupNavigation)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entityWithNav == null)
        {
            throw new EntityNotFoundException(typeof(ResObjectItemDepreciation), id);
        }

        return await MapToGetOutputDtoAsync(entityWithNav);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found
        var entity = await Repository.GetAsync(id);

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive
        entity!.UpdateStatus(ResObjectItemDepreciationStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public override async Task<ResObjectItemDepreciationDto> GetAsync(Guid id)
    {
        var query = await DepreciationRepository.GetQueryableAsync();
        var entity = await query
            .Include(x => x.ObjectTypeItemNavigation)
            .Include(x => x.CarGroupNavigation)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ResObjectItemDepreciation), id);
        }

        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<PagedResultDto<ResObjectItemDepreciationDto>> GetListAsync(GetResObjectItemDepreciationsInput input)
    {
        var query = await CreateFilteredQueryAsync(input);

        // Include navigation properties
        query = query
            .Include(x => x.ObjectTypeItemNavigation)
            .Include(x => x.CarGroupNavigation);

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        var entities = await AsyncExecuter.ToListAsync(query);
        var dtos = new List<ResObjectItemDepreciationDto>();

        foreach (var entity in entities)
        {
            dtos.Add(await MapToGetOutputDtoAsync(entity));
        }

        return new PagedResultDto<ResObjectItemDepreciationDto>(totalCount, dtos);
    }

    protected override IQueryable<ResObjectItemDepreciation> ApplySorting(IQueryable<ResObjectItemDepreciation> query, GetResObjectItemDepreciationsInput input)
    {
        // Map DTO field names to entity field names for sorting
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            var sorting = input.Sorting;

            // Map carGroupCode to CarGroupNavigation.Code (case-insensitive, whole word only)
            sorting = Regex.Replace(
                sorting,
                @"\bcarGroupCode\b",
                "CarGroupNavigation.Code",
                RegexOptions.IgnoreCase);

            // Map carGroupName to CarGroupNavigation.Name (case-insensitive, whole word only)
            sorting = Regex.Replace(
                sorting,
                @"\bcarGroupName\b",
                "CarGroupNavigation.Name",
                RegexOptions.IgnoreCase);

            // Create a new input with mapped sorting
            var mappedInput = new GetResObjectItemDepreciationsInput
            {
                Sorting = sorting,
                SkipCount = input.SkipCount,
                MaxResultCount = input.MaxResultCount,
                ObjectTypeItemId = input.ObjectTypeItemId,
                CarGroupId = input.CarGroupId,
                Status = input.Status,
                EffectDateFrom = input.EffectDateFrom,
                EffectDateTo = input.EffectDateTo
            };

            return base.ApplySorting(query, mappedInput);
        }

        return base.ApplySorting(query, input);
    }

    protected override async Task<ResObjectItemDepreciationDto> MapToGetOutputDtoAsync(ResObjectItemDepreciation entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null when mapping to DTO.");
        }

        var dto = ObjectMapper.Map<ResObjectItemDepreciation, ResObjectItemDepreciationDto>(entity);

        // Map CarGroup navigation properties
        if (entity.CarGroupNavigation != null)
        {
            dto.CarGroupCode = entity.CarGroupNavigation.Code;
            dto.CarGroupName = entity.CarGroupNavigation.Name;
        }

        return dto;
    }

    protected override async Task<IQueryable<ResObjectItemDepreciation>> CreateFilteredQueryAsync(GetResObjectItemDepreciationsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by ObjectTypeItemId
        if (input.ObjectTypeItemId.HasValue)
        {
            query = query.Where(x => x.ObjectTypeItemId == input.ObjectTypeItemId.Value);
        }

        // Filter by CarGroupId
        if (input.CarGroupId.HasValue)
        {
            query = query.Where(x => x.CarGroupId == input.CarGroupId.Value);
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        // Filter by EffectDate range
        if (input.EffectDateFrom.HasValue)
        {
            query = query.Where(x => x.EffectDate >= input.EffectDateFrom.Value);
        }

        if (input.EffectDateTo.HasValue)
        {
            query = query.Where(x => x.EffectDate <= input.EffectDateTo.Value);
        }

        return query;
    }

    public virtual async Task<ImportResObjectItemDepreciationResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportResObjectItemDepreciationResultDto();
        var errors = new List<ImportResObjectItemDepreciationErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["ResObjectItemDepreciation:ExcelFileInvalid"]);
        }

        // Read header row (row 1)
        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>();

        for (int col = 1; col <= 15; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                headers[headerValue.ToUpperInvariant()] = col;
            }
        }

        // Validate required headers
        var requiredHeaders = new[] { "OBJECTTYPEITEMCODE", "CARGROUPCODE", "USEDTIMEFROM", "USEDTIMETO", "DEPRECIATIONPERCENT", "EFFECTDATE", "MÃ HẠNG MỤC", "MÃ NHÓM XE", "THỜI GIAN SỬ DỤNG TỪ (NĂM)", "THỜI GIAN SỬ DỤNG ĐẾN (NĂM)", "PHẦN TRĂM KHẤU HAO (%)", "NGÀY HIỆU LỰC" };
        var hasRequired = (headers.ContainsKey("OBJECTTYPEITEMCODE") || headers.ContainsKey("MÃ HẠNG MỤC")) &&
                          (headers.ContainsKey("CARGROUPCODE") || headers.ContainsKey("MÃ NHÓM XE")) &&
                          (headers.ContainsKey("USEDTIMEFROM") || headers.ContainsKey("THỜI GIAN SỬ DỤNG TỪ (NĂM)")) &&
                          (headers.ContainsKey("USEDTIMETO") || headers.ContainsKey("THỜI GIAN SỬ DỤNG ĐẾN (NĂM)")) &&
                          (headers.ContainsKey("DEPRECIATIONPERCENT") || headers.ContainsKey("PHẦN TRĂM KHẤU HAO (%)")) &&
                          (headers.ContainsKey("EFFECTDATE") || headers.ContainsKey("NGÀY HIỆU LỰC"));

        if (!hasRequired)
        {
            throw new UserFriendlyException(L["ResObjectItemDepreciation:ExcelFileInvalid"]);
        }

        // Load lookup data for resolution
        var objectTypeItems = await ObjectTypeItemRepository.GetListAsync();
        var objectTypeItemMap = objectTypeItems.ToDictionary(x => x.Code.ToUpperInvariant(), x => x.Id);

        var carGroups = await CarGroupRepository.GetListAsync();
        var carGroupMap = carGroups.ToDictionary(x => x.Code.ToUpperInvariant(), x => x.Id);

        // Process data rows (starting from row 2)
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportResObjectItemDepreciationErrorDto>();

            // Get values
            var objectTypeItemCode = row.GetCellValue(headers.ContainsKey("MÃ HẠNG MỤC") ? headers["MÃ HẠNG MỤC"] : headers["OBJECTTYPEITEMCODE"]).Trim().ToUpperInvariant();
            var carGroupCode = row.GetCellValue(headers.ContainsKey("MÃ NHÓM XE") ? headers["MÃ NHÓM XE"] : headers["CARGROUPCODE"]).Trim().ToUpperInvariant();
            var usedTimeFromText = row.GetCellValue(headers.ContainsKey("THỜI GIAN SỬ DỤNG TỪ (NĂM)") ? headers["THỜI GIAN SỬ DỤNG TỪ (NĂM)"] : headers["USEDTIMEFROM"]).Trim();
            var usedTimeToText = row.GetCellValue(headers.ContainsKey("THỜI GIAN SỬ DỤNG ĐẾN (NĂM)") ? headers["THỜI GIAN SỬ DỤNG ĐẾN (NĂM)"] : headers["USEDTIMETO"]).Trim();
            var depreciationPercentText = row.GetCellValue(headers.ContainsKey("PHẦN TRĂM KHẤU HAO (%)") ? headers["PHẦN TRĂM KHẤU HAO (%)"] : headers["DEPRECIATIONPERCENT"]).Trim();
            var effectDateText = row.GetCellValue(headers.ContainsKey("NGÀY HIỆU LỰC") ? headers["NGÀY HIỆU LỰC"] : headers["EFFECTDATE"]).Trim();
            var expireDateCol = headers.ContainsKey("NGÀY HẾT HẠN") ? headers["NGÀY HẾT HẠN"] : (headers.ContainsKey("EXPIREDATE") ? headers["EXPIREDATE"] : -1);
            var expireDateText = expireDateCol != -1 ? row.GetCellValue(expireDateCol).Trim() : null;
            var statusCol = headers.ContainsKey("TRẠNG THÁI") ? headers["TRẠNG THÁI"] : (headers.ContainsKey("STATUS") ? headers["STATUS"] : -1);
            var statusText = statusCol != -1 ? row.GetCellValue(statusCol).Trim() : "Active";

            // Resolve ObjectTypeItemId from Code
            Guid objectTypeItemId = Guid.Empty;
            if (string.IsNullOrWhiteSpace(objectTypeItemCode))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "ObjectTypeItemCode",
                    Message = L["ResObjectItemDepreciation:ObjectTypeItemCodeRequired"],
                    Value = objectTypeItemCode
                });
            }
            else if (!objectTypeItemMap.TryGetValue(objectTypeItemCode, out objectTypeItemId))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "ObjectTypeItemCode",
                    Message = L["ResObjectItemDepreciation:ObjectTypeItemNotFound"],
                    Value = objectTypeItemCode
                });
            }

            // Resolve CarGroupId from Code
            Guid carGroupId = Guid.Empty;
            if (string.IsNullOrWhiteSpace(carGroupCode))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "CarGroupCode",
                    Message = L["ResObjectItemDepreciation:CarGroupCodeRequired"],
                    Value = carGroupCode
                });
            }
            else if (!carGroupMap.TryGetValue(carGroupCode, out carGroupId))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "CarGroupCode",
                    Message = L["ResObjectItemDepreciation:CarGroupNotFound"],
                    Value = carGroupCode
                });
            }

            // Parse and validate UsedTimeFrom
            double usedTimeFrom = 0;
            if (string.IsNullOrWhiteSpace(usedTimeFromText))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "UsedTimeFrom",
                    Message = L["ResObjectItemDepreciation:UsedTimeFromRequired"],
                    Value = usedTimeFromText
                });
            }
            else if (!double.TryParse(usedTimeFromText, out usedTimeFrom) || usedTimeFrom < 0)
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "UsedTimeFrom",
                    Message = L["ResObjectItemDepreciation:UsedTimeFromInvalid"],
                    Value = usedTimeFromText
                });
            }

            // Parse and validate UsedTimeTo
            double usedTimeTo = 0;
            if (string.IsNullOrWhiteSpace(usedTimeToText))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "UsedTimeTo",
                    Message = L["ResObjectItemDepreciation:UsedTimeToRequired"],
                    Value = usedTimeToText
                });
            }
            else if (!double.TryParse(usedTimeToText, out usedTimeTo) || usedTimeTo < 0)
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "UsedTimeTo",
                    Message = L["ResObjectItemDepreciation:UsedTimeToInvalid"],
                    Value = usedTimeToText
                });
            }

            // Validate used time range
            if (usedTimeFrom > usedTimeTo)
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "UsedTimeFrom",
                    Message = L["ResObjectItemDepreciation:UsedTimeFromMustBeLessThanOrEqualTo"],
                    Value = $"{usedTimeFrom} - {usedTimeTo}"
                });
            }

            // Parse and validate DepreciationPercent
            double depreciationPercent = 0;
            if (string.IsNullOrWhiteSpace(depreciationPercentText))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "DepreciationPercent",
                    Message = L["ResObjectItemDepreciation:DepreciationPercentRequired"],
                    Value = depreciationPercentText
                });
            }
            else if (!double.TryParse(depreciationPercentText, out depreciationPercent) || depreciationPercent < 0 || depreciationPercent > 100)
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "DepreciationPercent",
                    Message = L["ResObjectItemDepreciation:DepreciationPercentInvalid"],
                    Value = depreciationPercentText
                });
            }

            // Parse and validate EffectDate
            DateTime effectDate = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(effectDateText))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "EffectDate",
                    Message = L["ResObjectItemDepreciation:EffectDateRequired"],
                    Value = effectDateText
                });
            }
            else if (!DateTime.TryParse(effectDateText, out effectDate))
            {
                rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "EffectDate",
                    Message = L["ResObjectItemDepreciation:EffectDateInvalid"],
                    Value = effectDateText
                });
            }

            // Parse and validate ExpireDate
            DateTime? expireDate = null;
            if (!string.IsNullOrWhiteSpace(expireDateText))
            {
                if (DateTime.TryParse(expireDateText, out var parsedExpireDate))
                {
                    expireDate = parsedExpireDate;
                    if (expireDate.Value <= effectDate)
                    {
                        rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "ExpireDate",
                            Message = L["ResObjectItemDepreciation:ExpireDateMustBeAfterEffectDate"],
                            Value = expireDateText
                        });
                    }
                }
                else
                {
                    rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "ExpireDate",
                        Message = L["ResObjectItemDepreciation:ExpireDateInvalid"],
                        Value = expireDateText
                    });
                }
            }

            // Parse Status
            ResObjectItemDepreciationStatus status = ResObjectItemDepreciationStatus.Active;
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                if (string.Equals(statusText, "Hoạt động", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    status = ResObjectItemDepreciationStatus.Active;
                }
                else if (string.Equals(statusText, "Ngừng hoạt động", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Deactive", StringComparison.OrdinalIgnoreCase))
                {
                    status = ResObjectItemDepreciationStatus.Deactive;
                }
                else if (!Enum.TryParse<ResObjectItemDepreciationStatus>(statusText, true, out status))
                {
                    status = ResObjectItemDepreciationStatus.Active; // Default to Active if invalid
                }
            }

            // If no errors, try to create the entity
            if (rowErrors.Count == 0)
            {
                try
                {
                    // IDs are already resolved and validated against the dictionaries
                    if (objectTypeItemId != Guid.Empty && carGroupId != Guid.Empty)
                    {
                        var entity = new ResObjectItemDepreciation(
                            GuidGenerator.Create(),
                            objectTypeItemId,
                            carGroupId,
                            usedTimeFrom,
                            usedTimeTo,
                            depreciationPercent,
                            effectDate,
                            expireDate,
                            status
                        );

                        await Manager.CreateAsync(entity);
                        result.SuccessCount++;
                    }
                }
                catch (BusinessException ex)
                {
                    var message = ex.Message;

                    // Try to localize if the message is a localization key
                    if (message.Contains("ResObjectItemDepreciation:"))
                    {
                        var localizedMessage = L[message].Value;
                        message = localizedMessage;
                    }

                    rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = message,
                        Value = null
                    });
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportResObjectItemDepreciationErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = ex.Message,
                        Value = null
                    });
                }
            }

            if (rowErrors.Count > 0)
            {
                result.ErrorCount++;
                errors.AddRange(rowErrors);
            }

            rowNumber++;
        }

        result.Errors = errors;
        await CurrentUnitOfWork.SaveChangesAsync();

        return result;
    }

    public virtual async Task<byte[]> ExportTemplateAsync()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Khấu hao");

        // Set header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Set headers matching the import format
        worksheet.Cell(1, 1).Value = "Mã hạng mục";
        worksheet.Cell(1, 2).Value = "Mã nhóm xe";
        worksheet.Cell(1, 3).Value = "Thời gian sử dụng từ (năm)";
        worksheet.Cell(1, 4).Value = "Thời gian sử dụng đến (năm)";
        worksheet.Cell(1, 5).Value = "Phần trăm khấu hao (%)";
        worksheet.Cell(1, 6).Value = "Ngày hiệu lực";
        worksheet.Cell(1, 7).Value = "Ngày hết hạn";
        worksheet.Cell(1, 8).Value = "Trạng thái";

        // Add comment row with instructions (row 2)
        worksheet.Cell(2, 1).Value = "Nhập mã vật phẩm hợp lệ từ trang Tra cứu Hạng mục";
        worksheet.Cell(2, 2).Value = "Nhập mã nhóm xe hợp lệ từ trang Tra cứu Nhóm xe";
        worksheet.Cell(2, 3).Value = "0";
        worksheet.Cell(2, 4).Value = "5";
        worksheet.Cell(2, 5).Value = "10";
        worksheet.Cell(2, 6).Value = "2024-01-01";
        worksheet.Cell(2, 7).Value = "2025-12-31";
        worksheet.Cell(2, 8).Value = "Active";

        // Style the instruction row
        worksheet.Row(2).Style.Font.Italic = true;
        worksheet.Row(2).Style.Font.FontColor = XLColor.Gray;

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Add lookup sheet for ObjectTypeItems
        var lookupSheet = workbook.Worksheets.Add("Tra cứu Hạng mục");
        lookupSheet.Row(1).Style.Font.Bold = true;
        lookupSheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;

        lookupSheet.Cell(1, 1).Value = "MÃ";
        lookupSheet.Cell(1, 2).Value = "TÊN";
        lookupSheet.Cell(1, 3).Value = "TRẠNG THÁI";

        // Fetch available ObjectTypeItems (limit to 100 for performance)
        var objectTypeItemsLookup = await ObjectTypeItemRepository.GetQueryableAsync();
        var items = await objectTypeItemsLookup
            .Where(x => x.Status == ResObjectTypeItemStatus.Active)
            .OrderBy(x => x.Code)
            .Take(100)
            .ToListAsync();

        int lookupRow = 2;
        foreach (var item in items)
        {
            lookupSheet.Cell(lookupRow, 1).Value = item.Code;
            lookupSheet.Cell(lookupRow, 2).Value = item.Name;
            lookupSheet.Cell(lookupRow, 3).Value = item.Status.ToString();
            lookupRow++;
        }

        lookupSheet.Columns().AdjustToContents();

        // Add lookup sheet for CarGroups
        var carGroupLookupSheet = workbook.Worksheets.Add("Tra cứu Nhóm xe");
        carGroupLookupSheet.Row(1).Style.Font.Bold = true;
        carGroupLookupSheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGreen;

        carGroupLookupSheet.Cell(1, 1).Value = "MÃ";
        carGroupLookupSheet.Cell(1, 2).Value = "TÊN";
        carGroupLookupSheet.Cell(1, 3).Value = "TRẠNG THÁI";

        // Fetch available CarGroups (limit to 100 for performance)
        var carGroupsLookup = await CarGroupRepository.GetQueryableAsync();
        var carGroupItems = await carGroupsLookup
            .Where(x => x.Status == ResCarGroupStatus.Active)
            .OrderBy(x => x.Code)
            .Take(100)
            .ToListAsync();

        int carGroupLookupRow = 2;
        foreach (var carGroup in carGroupItems)
        {
            carGroupLookupSheet.Cell(carGroupLookupRow, 1).Value = carGroup.Code;
            carGroupLookupSheet.Cell(carGroupLookupRow, 2).Value = carGroup.Name;
            carGroupLookupSheet.Cell(carGroupLookupRow, 3).Value = carGroup.Status.ToString();
            carGroupLookupRow++;
        }

        carGroupLookupSheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
