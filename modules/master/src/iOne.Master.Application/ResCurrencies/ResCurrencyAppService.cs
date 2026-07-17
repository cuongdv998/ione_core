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
using iOne.Master.ResCurrencies;
using iOne.ResCurrencies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCurrencies;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResCurrencyPermissions.Default)]
public class ResCurrencyAppService : CrudAppService<
    ResCurrency,
    ResCurrencyDto,
    Guid,
    GetResCurrenciesInput,
    CreateResCurrencyDto,
    UpdateResCurrencyDto>, IResCurrencyAppService
{
    protected ResCurrencyManager Manager { get; }
    protected IResCurrencyRepository CurrencyRepository { get; }

    public ResCurrencyAppService(
        IResCurrencyRepository repository,
        ResCurrencyManager manager)
        : base(repository)
    {
        Manager = manager;
        CurrencyRepository = repository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResCurrencyPermissions.View;
        GetListPolicyName = ResCurrencyPermissions.View;
        CreatePolicyName = ResCurrencyPermissions.Create;
        UpdatePolicyName = ResCurrencyPermissions.Edit;
        DeletePolicyName = ResCurrencyPermissions.Delete;
    }

    public override async Task<ResCurrencyDto> CreateAsync(CreateResCurrencyDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await CurrencyRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResCurrency:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResCurrency:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResCurrency(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCurrency, ResCurrencyDto>(entity);
    }

    public override async Task<ResCurrencyDto> UpdateAsync(Guid id, UpdateResCurrencyDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCurrency, ResCurrencyDto>(entity!);
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
        entity!.UpdateStatus(ResCurrencyStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCurrency>> CreateFilteredQueryAsync(GetResCurrenciesInput input)
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

    public virtual async Task<ImportResCurrencyResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportResCurrencyResultDto();
        var errors = new List<ImportResCurrencyErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["Master:ResCurrency:ExcelFileInvalid"]);
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
            throw new UserFriendlyException(L["Master:ResCurrency:ExcelFileInvalid"]);
        }

        // Track codes within the import file to detect duplicates
        var importCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process data rows (starting from row 2)
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportResCurrencyErrorDto>();

            // Get values
            var code = row.GetCellValue(headers.ContainsKey("MÃ") ? headers["MÃ"] : headers["CODE"]).Trim().ToUpperInvariant();
            var name = row.GetCellValue(headers.ContainsKey("TÊN") ? headers["TÊN"] : headers["NAME"]).Trim();
            var descriptionCol = headers.ContainsKey("MÔ TẢ") ? headers["MÔ TẢ"] : (headers.ContainsKey("DESCRIPTION") ? headers["DESCRIPTION"] : -1);
            var description = descriptionCol != -1 ? row.GetCellValue(descriptionCol).Trim() : null;
            var statusCol = headers.ContainsKey("TRẠNG THÁI") ? headers["TRẠNG THÁI"] : (headers.ContainsKey("STATUS") ? headers["STATUS"] : -1);
            var statusText = statusCol != -1 ? row.GetCellValue(statusCol).Trim() : "Active";

            // Validate Code
            bool codeIsValid = true;
            if (string.IsNullOrWhiteSpace(code))
            {
                rowErrors.Add(new ImportResCurrencyErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCurrency:CodeRequired"],
                    Value = code
                });
                codeIsValid = false;
            }
            else if (code.Length > 50)
            {
                rowErrors.Add(new ImportResCurrencyErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCurrency:CodeMaxLength"],
                    Value = code
                });
                codeIsValid = false;
            }
            else if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
            {
                rowErrors.Add(new ImportResCurrencyErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCurrency:CodeInvalid"],
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
                    var message = L["Master:ResCurrency:CodeDuplicateInFile"].Value.Replace("{0}", code);
                    rowErrors.Add(new ImportResCurrencyErrorDto
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
                    if (await CurrencyRepository.IsCodeExistsAsync(code))
                    {
                        var message = L["Master:ResCurrency:CodeExists"].Value.Replace("{Code}", code);
                        rowErrors.Add(new ImportResCurrencyErrorDto
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
                rowErrors.Add(new ImportResCurrencyErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCurrency:NameRequired"],
                    Value = name
                });
            }
            else if (name.Length > 250)
            {
                rowErrors.Add(new ImportResCurrencyErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCurrency:NameMaxLength"],
                    Value = name
                });
            }

            // Validate Description
            if (description != null && description.Length > 500)
            {
                rowErrors.Add(new ImportResCurrencyErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Description",
                    Message = L["Master:ResCurrency:DescriptionMaxLength"],
                    Value = description
                });
            }

            // Parse Status
            ResCurrencyStatus status = ResCurrencyStatus.Active;
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                if (!Enum.TryParse<ResCurrencyStatus>(statusText, true, out status))
                {
                    status = ResCurrencyStatus.Active; // Default to Active if invalid
                }
            }

            // If no errors, try to create the entity
            if (rowErrors.Count == 0 && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var entity = new ResCurrency(
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
                    if (message.Contains("Master:ResCurrency:") || message.Contains("Master::ResCurrency:"))
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
                    
                    rowErrors.Add(new ImportResCurrencyErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = message,
                        Value = ex.Data.Contains("Code") ? ex.Data["Code"]?.ToString() : code
                    });
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportResCurrencyErrorDto
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
        var worksheet = workbook.Worksheets.Add("Tiền tệ");

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
        worksheet.Cell(2, 1).Value = "USD";
        worksheet.Cell(2, 2).Value = "Đô la Mỹ";
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
