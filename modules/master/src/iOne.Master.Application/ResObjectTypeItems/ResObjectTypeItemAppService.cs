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
using iOne.Master.ResObjectTypeItems;
using iOne.ResObjectTypeItems; // For Entity, Manager, Repository
using iOne.ResObjectTypes;
using iOne.ResObjectItemTypes;
using iOne.ResUoms;
using iOne.ResObjectItemDepreciations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Master.ResObjectTypeItems;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResObjectTypeItemPermissions.Default)]
public class ResObjectTypeItemAppService : CrudAppService<
    ResObjectTypeItem,
    ResObjectTypeItemDto,
    Guid,
    GetResObjectTypeItemsInput,
    CreateResObjectTypeItemDto,
    UpdateResObjectTypeItemDto>, IResObjectTypeItemAppService
{
    protected ResObjectTypeItemManager Manager { get; }
    protected IResObjectTypeItemRepository ObjectTypeItemRepository { get; }
    protected IResObjectTypeRepository ObjectTypeRepository { get; }
    protected IResObjectItemTypeRepository ObjectItemTypeRepository { get; }
    protected IResUomRepository UomRepository { get; }
    protected IResObjectItemDepreciationRepository DepreciationRepository { get; }

    public ResObjectTypeItemAppService(
        IResObjectTypeItemRepository repository,
        ResObjectTypeItemManager manager,
        IResObjectTypeRepository objectTypeRepository,
        IResObjectItemTypeRepository objectItemTypeRepository,
        IResUomRepository uomRepository,
        IResObjectItemDepreciationRepository depreciationRepository)
        : base(repository)
    {
        Manager = manager;
        ObjectTypeItemRepository = repository;
        ObjectTypeRepository = objectTypeRepository;
        ObjectItemTypeRepository = objectItemTypeRepository;
        UomRepository = uomRepository;
        DepreciationRepository = depreciationRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResObjectTypeItemPermissions.View;
        GetListPolicyName = ResObjectTypeItemPermissions.View;
        CreatePolicyName = ResObjectTypeItemPermissions.Create;
        UpdatePolicyName = ResObjectTypeItemPermissions.Edit;
        DeletePolicyName = ResObjectTypeItemPermissions.Delete;
    }

    public override async Task<ResObjectTypeItemDto> CreateAsync(CreateResObjectTypeItemDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await ObjectTypeItemRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResObjectTypeItem:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
            }
        }

        // Validate foreign keys
        if (!await ObjectTypeRepository.AnyAsync(x => x.Id == input.ObjectTypeId))
        {
            throw new UserFriendlyException(
                    L["ResObjectTypeItem:ObjectTypeNotFound"].Value
                );
        }

        if (input.ObjectItemType.HasValue)
        {
            if (!await ObjectItemTypeRepository.AnyAsync(x => x.Id == input.ObjectItemType.Value))
            {
                throw new UserFriendlyException(
                    L["ResObjectTypeItem:ObjectItemTypeNotFound"].Value
                );
            }
        }

        if (!await UomRepository.AnyAsync(x => x.Id == input.UomId))
        {
            throw new UserFriendlyException(
                    L["ResObjectTypeItem:UomNotFound"].Value
                );
        }

        var entity = new ResObjectTypeItem(
            GuidGenerator.Create(),
            input.ObjectTypeId,
            input.ObjectItemType,
            input.Code,
            input.Name,
            input.UomId,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResObjectTypeItem, ResObjectTypeItemDto>(entity);
    }

    public override async Task<ResObjectTypeItemDto> UpdateAsync(Guid id, UpdateResObjectTypeItemDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Validate foreign keys
        if (!await ObjectTypeRepository.AnyAsync(x => x.Id == input.ObjectTypeId))
        {
            throw new UserFriendlyException(
                    L["ResObjectTypeItem:ObjectTypeNotFound"].Value
                );
        }

        if (input.ObjectItemType.HasValue)
        {
            if (!await ObjectItemTypeRepository.AnyAsync(x => x.Id == input.ObjectItemType.Value))
            {
                throw new UserFriendlyException(
                    L["ResObjectTypeItem:ObjectItemTypeNotFound"].Value
                );
            }
        }

        if (!await UomRepository.AnyAsync(x => x.Id == input.UomId))
        {
            throw new UserFriendlyException(
                    L["ResObjectTypeItem:UomNotFound"].Value
                );
        }

        // ⚠️ QUAN TRỌNG: Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.ObjectTypeId, input.ObjectItemType, input.Name, input.UomId, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResObjectTypeItem, ResObjectTypeItemDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Check if in use by Depreciation
        if (await DepreciationRepository.IsObjectTypeItemInUseAsync(id))
        {
            throw new UserFriendlyException(L["ResObjectTypeItem:InUseByDepreciation"]);
        }

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ResObjectTypeItemStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResObjectTypeItem>> CreateFilteredQueryAsync(GetResObjectTypeItemsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code.Trim()}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name.Trim()}%"));
        }

        // Filter by ObjectTypeId
        if (input.ObjectTypeId.HasValue)
        {
            query = query.Where(x => x.ObjectTypeId == input.ObjectTypeId.Value);
        }

        // Filter by ObjectItemType
        if (input.ObjectItemType.HasValue)
        {
            query = query.Where(x => x.ObjectItemType == input.ObjectItemType.Value);
        }

        // Filter by UomId
        if (input.UomId.HasValue)
        {
            query = query.Where(x => x.UomId == input.UomId.Value);
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }

    public virtual async Task<ImportResObjectTypeItemResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportResObjectTypeItemResultDto();
        var errors = new List<ImportResObjectTypeItemErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["Master:ResObjectTypeItem:ExcelFileInvalid"]);
        }

        // Read header row (row 1)
        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>();

        for (int col = 1; col <= 20; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                headers[headerValue.ToUpperInvariant()] = col;
            }
        }

        // Validate required headers
        var requiredHeaders = new[] { "CODE", "NAME", "OBJECTTYPECODE", "UOMCODE", "MÃ", "TÊN", "MÃ LOẠI ĐỐI TƯỢNG", "MÃ ĐƠN VỊ TÍNH" };
        var hasRequired = (headers.ContainsKey("CODE") || headers.ContainsKey("MÃ")) &&
                          (headers.ContainsKey("NAME") || headers.ContainsKey("TÊN")) &&
                          (headers.ContainsKey("OBJECTTYPECODE") || headers.ContainsKey("MÃ LOẠI ĐỐI TƯỢNG")) &&
                          (headers.ContainsKey("UOMCODE") || headers.ContainsKey("MÃ ĐƠN VỊ TÍNH"));

        if (!hasRequired)
        {
            throw new UserFriendlyException(L["Master:ResObjectTypeItem:ExcelFileInvalid"]);
        }

        // Track codes within the import file to detect duplicates
        var importCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Load reference data for validation
        var objectTypes = await ObjectTypeRepository.GetListAsync();
        var objectItemTypes = await ObjectItemTypeRepository.GetListAsync();
        var uoms = await UomRepository.GetListAsync();

        // Process data rows (starting from row 2)
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportResObjectTypeItemErrorDto>();

            // Get values
            var code = row.GetCellValue(headers.ContainsKey("MÃ") ? headers["MÃ"] : headers["CODE"]).Trim().ToUpperInvariant();
            var name = row.GetCellValue(headers.ContainsKey("TÊN") ? headers["TÊN"] : headers["NAME"]).Trim();
            var objectTypeCode = row.GetCellValue(headers.ContainsKey("MÃ LOẠI ĐỐI TƯỢNG") ? headers["MÃ LOẠI ĐỐI TƯỢNG"] : headers["OBJECTTYPECODE"]).Trim().ToUpperInvariant();
            var objectItemTypeCol = headers.ContainsKey("MÃ LOẠI HẠNG MỤC") ? headers["MÃ LOẠI HẠNG MỤC"] : (headers.ContainsKey("OBJECTITEMTYPECODE") ? headers["OBJECTITEMTYPECODE"] : -1);
            var objectItemTypeCode = objectItemTypeCol != -1 ? row.GetCellValue(objectItemTypeCol).Trim().ToUpperInvariant() : null;
            var uomCode = row.GetCellValue(headers.ContainsKey("MÃ ĐƠN VỊ TÍNH") ? headers["MÃ ĐƠN VỊ TÍNH"] : headers["UOMCODE"]).Trim().ToUpperInvariant();
            var descriptionCol = headers.ContainsKey("MÔ TẢ") ? headers["MÔ TẢ"] : (headers.ContainsKey("DESCRIPTION") ? headers["DESCRIPTION"] : -1);
            var description = descriptionCol != -1 ? row.GetCellValue(descriptionCol).Trim() : null;
            var statusCol = headers.ContainsKey("TRẠNG THÁI") ? headers["TRẠNG THÁI"] : (headers.ContainsKey("STATUS") ? headers["STATUS"] : -1);
            var statusText = statusCol != -1 ? row.GetCellValue(statusCol).Trim() : "Active";

            // Validate Code
            bool codeIsValid = true;
            if (string.IsNullOrWhiteSpace(code))
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResObjectTypeItem:CodeRequired"],
                    Value = code
                });
                codeIsValid = false;
            }
            else if (code.Length > 50)
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResObjectTypeItem:CodeMaxLength"],
                    Value = code
                });
                codeIsValid = false;
            }
            else if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResObjectTypeItem:CodeInvalid"],
                    Value = code
                });
                codeIsValid = false;
            }

            // Check for duplicates only if code format is valid
            if (codeIsValid && !string.IsNullOrWhiteSpace(code))
            {
                // Check for duplicate within the same import file
                if (importCodes.Contains(code))
                {
                    var message = L["Master:ResObjectTypeItem:CodeDuplicateInFile"].Value.Replace("{0}", code);
                    rowErrors.Add(new ImportResObjectTypeItemErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Code",
                        Message = message,
                        Value = code
                    });
                }
                else
                {
                    // Check for duplicate in database
                    if (await ObjectTypeItemRepository.IsCodeExistsAsync(code))
                    {
                        var message = L["Master:ResObjectTypeItem:CodeExists"].Value.Replace("{Code}", code);
                        rowErrors.Add(new ImportResObjectTypeItemErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "Code",
                            Message = message,
                            Value = code
                        });
                    }
                    else
                    {
                        // Add to import codes set if no duplicate errors
                        importCodes.Add(code);
                    }
                }
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(name))
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResObjectTypeItem:NameRequired"],
                    Value = name
                });
            }
            else if (name.Length > 250)
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResObjectTypeItem:NameMaxLength"],
                    Value = name
                });
            }

            // Validate Description
            if (description != null && description.Length > 500)
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Description",
                    Message = L["Master:ResObjectTypeItem:DescriptionMaxLength"],
                    Value = description
                });
            }

            // Validate ObjectTypeCode
            Guid? objectTypeId = null;
            if (string.IsNullOrWhiteSpace(objectTypeCode))
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "ObjectTypeCode",
                    Message = L["Master:ResObjectTypeItem:ObjectTypeCodeRequired"],
                    Value = objectTypeCode
                });
            }
            else
            {
                var objectType = objectTypes.FirstOrDefault(x => x.Code == objectTypeCode);
                if (objectType == null)
                {
                    rowErrors.Add(new ImportResObjectTypeItemErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "ObjectTypeCode",
                        Message = L["Master:ResObjectTypeItem:ObjectTypeNotFound"],
                        Value = objectTypeCode
                    });
                }
                else
                {
                    objectTypeId = objectType.Id;
                }
            }

            // Validate ObjectItemTypeCode (optional)
            Guid? objectItemTypeId = null;
            if (!string.IsNullOrWhiteSpace(objectItemTypeCode))
            {
                var objectItemType = objectItemTypes.FirstOrDefault(x => x.Code == objectItemTypeCode);
                if (objectItemType == null)
                {
                    rowErrors.Add(new ImportResObjectTypeItemErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "ObjectItemTypeCode",
                        Message = L["Master:ResObjectTypeItem:ObjectItemTypeNotFound"],
                        Value = objectItemTypeCode
                    });
                }
                else
                {
                    objectItemTypeId = objectItemType.Id;
                }
            }

            // Validate UomCode
            Guid? uomId = null;
            if (string.IsNullOrWhiteSpace(uomCode))
            {
                rowErrors.Add(new ImportResObjectTypeItemErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "UomCode",
                    Message = L["Master:ResObjectTypeItem:UomCodeRequired"],
                    Value = uomCode
                });
            }
            else
            {
                var uom = uoms.FirstOrDefault(x => x.Code == uomCode);
                if (uom == null)
                {
                    rowErrors.Add(new ImportResObjectTypeItemErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "UomCode",
                        Message = L["Master:ResObjectTypeItem:UomNotFound"],
                        Value = uomCode
                    });
                }
                else
                {
                    uomId = uom.Id;
                }
            }

            // Parse Status
            ResObjectTypeItemStatus status = ResObjectTypeItemStatus.Active;
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                if (string.Equals(statusText, "Hoạt động", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    status = ResObjectTypeItemStatus.Active;
                }
                else if (string.Equals(statusText, "Ngừng hoạt động", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Deactive", StringComparison.OrdinalIgnoreCase))
                {
                    status = ResObjectTypeItemStatus.Deactive;
                }
                else if (!Enum.TryParse<ResObjectTypeItemStatus>(statusText, true, out status))
                {
                    status = ResObjectTypeItemStatus.Active; // Default to Active if invalid
                }
            }

            // If no errors, try to create the entity
            if (rowErrors.Count == 0 && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name)
                && objectTypeId.HasValue && uomId.HasValue)
            {
                try
                {
                    var entity = new ResObjectTypeItem(
                        GuidGenerator.Create(),
                        objectTypeId.Value,
                        objectItemType: objectItemTypeId != null ? objectItemTypeId.Value : null,
                        code,
                        name,
                        uomId.Value,
                        string.IsNullOrWhiteSpace(description) ? null : description,
                        status
                    );

                    await Manager.CreateAsync(entity);
                    result.SuccessCount++;
                }
                catch (BusinessException ex)
                {
                    // Get the localized message and replace placeholders
                    var message = ex.Message;

                    // If message contains localization key format, try to localize it
                    if (message.Contains("Master:ResObjectTypeItem:") || message.Contains("Master::ResObjectTypeItem:"))
                    {
                        var localizedMessage = L[message.Replace("Master::", "Master:")].Value;

                        // Replace {Code} placeholder if Code is in exception data
                        if (ex.Data.Contains("Code") && localizedMessage.Contains("{Code}"))
                        {
                            var codeValue = ex.Data["Code"]?.ToString() ?? code;
                            message = localizedMessage.Replace("{Code}", codeValue);
                        }
                        else
                        {
                            message = localizedMessage;
                        }
                    }
                    else if (message.Contains("{Code}"))
                    {
                        // Replace {Code} placeholder if Code is in exception data
                        if (ex.Data.Contains("Code"))
                        {
                            var codeValue = ex.Data["Code"]?.ToString() ?? code;
                            message = message.Replace("{Code}", codeValue);
                        }
                    }

                    rowErrors.Add(new ImportResObjectTypeItemErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = message,
                        Value = ex.Data.Contains("Code") ? ex.Data["Code"]?.ToString() : code
                    });
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportResObjectTypeItemErrorDto
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

    public virtual Task<byte[]> ExportTemplateAsync()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Loại vật phẩm");

        // Set header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Set headers matching the import format
        worksheet.Cell(1, 1).Value = "Mã";
        worksheet.Cell(1, 2).Value = "Tên";
        worksheet.Cell(1, 3).Value = "Mã loại đối tượng";
        worksheet.Cell(1, 4).Value = "Mã loại hạng mục";
        worksheet.Cell(1, 5).Value = "Mã đơn vị tính";
        worksheet.Cell(1, 6).Value = "Mô tả";
        worksheet.Cell(1, 7).Value = "Trạng thái";

        // Add sample data row (row 2) with example values
        worksheet.Cell(2, 1).Value = "SAMPLE_ITEM";
        worksheet.Cell(2, 2).Value = "Sample Object Type Item";
        worksheet.Cell(2, 3).Value = "OBJ_TYPE_CODE";
        worksheet.Cell(2, 4).Value = "ITEM_TYPE_CODE";
        worksheet.Cell(2, 5).Value = "UOM_CODE";
        worksheet.Cell(2, 6).Value = "Sample description";
        worksheet.Cell(2, 7).Value = "Active";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
