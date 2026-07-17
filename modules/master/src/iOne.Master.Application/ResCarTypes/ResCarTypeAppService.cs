using ClosedXML.Excel;
using iOne.Master;
using iOne.Master.Helpers;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResCarModels;
using iOne.Master.ResCarTypes;
using iOne.ResCarBrands;
using iOne.ResCarTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarTypes;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResCarTypePermissions.Default)]
public class ResCarTypeAppService : CrudAppService<
    ResCarType,
    ResCarTypeDto,
    Guid,
    GetResCarTypesInput,
    CreateResCarTypeDto,
    UpdateResCarTypeDto>, IResCarTypeAppService
{
    protected ResCarTypeManager Manager { get; }
    protected IResCarTypeRepository CarTypeRepository { get; }

    public ResCarTypeAppService(
        IResCarTypeRepository repository,
        ResCarTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        CarTypeRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResCarTypePermissions.View;
        GetListPolicyName = ResCarTypePermissions.View;
        CreatePolicyName = ResCarTypePermissions.Create;
        UpdatePolicyName = ResCarTypePermissions.Edit;
        DeletePolicyName = ResCarTypePermissions.Delete;
    }

    public override async Task<ResCarTypeDto> CreateAsync(CreateResCarTypeDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await CarTypeRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResCarType:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResCarType:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResCarType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCarType, ResCarTypeDto>(entity);
    }

    public override async Task<ResCarTypeDto> UpdateAsync(Guid id, UpdateResCarTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCarType, ResCarTypeDto>(entity!);
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
        entity!.UpdateStatus(ResCarTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCarType>> CreateFilteredQueryAsync(GetResCarTypesInput input)
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

    public virtual async Task<ImportResCarTypeResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportResCarTypeResultDto();
        var errors = new List<ImportResCarTypeErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["ResCarType:ExcelFileInvalid"]);
        }

        // Read header row (row 1)
        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>();

        for (int col = 1; col <= 10; col++)
        {
            var headerValue = headerRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                headers[headerValue.ToUpperInvariant()] = col;
            }
        }

        // Validate required headers
        var requiredHeaders = new[] { "CODE", "NAME", "MÃ", "TÊN" };
        var hasRequired = (headers.ContainsKey("CODE") || headers.ContainsKey("MÃ")) &&
                          (headers.ContainsKey("NAME") || headers.ContainsKey("TÊN"));

        if (!hasRequired)
        {
            throw new UserFriendlyException(L["ResCarType:ExcelFileInvalid"]);
        }

        // Track codes within the import file to detect duplicates
        var importCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process data rows (starting from row 2)
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportResCarTypeErrorDto>();

            // Get values
            var code = row.GetCellValue(headers.ContainsKey("MÃ") ? headers["MÃ"] : headers["CODE"]).Trim().ToUpperInvariant();
            var name = row.GetCellValue(headers.ContainsKey("TÊN") ? headers["TÊN"] : headers["NAME"]).Trim();
            var descriptionCol = headers.ContainsKey("MÔ TẢ") ? headers["MÔ TẢ"] : (headers.ContainsKey("DESCRIPTION") ? headers["DESCRIPTION"] : -1);
            var description = descriptionCol != -1 ? row.GetCellValue(descriptionCol).Trim() : null;
            var statusCol = headers.ContainsKey("TRẠNG THÁI") ? headers["TRẠNG THÁI"] : (headers.ContainsKey("STATUS") ? headers["STATUS"] : -1);
            var statusText = statusCol != -1 ? row.GetCellValue(statusCol).Trim() : "Active";

            // Validate Code
            if (string.IsNullOrWhiteSpace(code))
            {
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarType:CodeRequired"],
                    Value = code
                });
            }
            else if (code.Length > 50)
            {
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarType:CodeMaxLength"],
                    Value = code
                });
            }
            else if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
            {
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarType:CodeInvalid"],
                    Value = code
                });
            }

            // Check for duplicates only if code format is valid
            bool codeIsValid = !string.IsNullOrWhiteSpace(code) &&
                               code.Length <= 50 &&
                               Regex.IsMatch(code, @"^[A-Z0-9_]+$");

            if (codeIsValid)
            {
                // Check for duplicate within the same import file
                if (importCodes.Contains(code))
                {
                    var message = L["Master:ResCarType:CodeDuplicateInFile"].Value.Replace("{0}", code);
                    rowErrors.Add(new ImportResCarTypeErrorDto
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
                    if (await CarTypeRepository.IsCodeExistsAsync(code))
                    {
                        var message = L["Master:ResCarType:CodeExists"].Value.Replace("{Code}", code);
                        rowErrors.Add(new ImportResCarTypeErrorDto
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
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCarType:NameRequired"],
                    Value = name
                });
            }
            else if (name.Length > 250)
            {
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCarType:NameMaxLength"],
                    Value = name
                });
            }

            // Validate Description
            if (description != null && description.Length > 500)
            {
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Description",
                    Message = L["Master:ResCarType:DescriptionMaxLength"],
                    Value = description
                });
            }

            // Parse Status
            ResCarTypeStatus status = ResCarTypeStatus.Active;
            bool statusIsValid = true;

            if (string.IsNullOrWhiteSpace(statusText) || string.Equals(statusText, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Hoạt động", StringComparison.OrdinalIgnoreCase))
            {
                status = ResCarTypeStatus.Active;
            }
            else if (string.Equals(statusText, "Deactive", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Ngừng hoạt động", StringComparison.OrdinalIgnoreCase))
            {
                status = ResCarTypeStatus.Deactive;
            }
            else
            {
                rowErrors.Add(new ImportResCarTypeErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Status",
                    Message = L["Master:ResCarBrand:StatusInvalid"],
                    Value = statusText
                });
                statusIsValid = false;
            }

            // If no errors, try to create the entity
            if (rowErrors.Count == 0 && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var entity = new ResCarType(
                        GuidGenerator.Create(),
                        code,
                        name,
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
                    if (message.Contains("Master:ResCarType:") || message.Contains("Master::ResCarType:"))
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

                    rowErrors.Add(new ImportResCarTypeErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = message,
                        Value = ex.Data.Contains("Code") ? ex.Data["Code"]?.ToString() : code
                    });
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportResCarTypeErrorDto
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
        var worksheet = workbook.Worksheets.Add("Loại xe");

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
        worksheet.Cell(2, 1).Value = "SEDAN";
        worksheet.Cell(2, 2).Value = "Sedan";
        worksheet.Cell(2, 3).Value = "Sample description";
        worksheet.Cell(2, 4).Value = "Active";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}

