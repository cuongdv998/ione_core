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
using iOne.Master.ResCarGroups;
using iOne.ResCarGroups; // For Entity, Manager, Repository
using iOne.ResCarLines;
using iOne.ResObjectItemDepreciations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Master.ResCarGroups;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
//[Authorize(ResCarGroupPermissions.Default)]
public class ResCarGroupAppService : CrudAppService<
    ResCarGroup,
    ResCarGroupDto,
    Guid,
    GetResCarGroupsInput,
    CreateResCarGroupDto,
    UpdateResCarGroupDto>, IResCarGroupAppService
{
    protected ResCarGroupManager Manager { get; }
    protected IResCarGroupRepository CarGroupRepository { get; }
    protected IResCarLineRepository CarLineRepository { get; }
    protected IResObjectItemDepreciationRepository DepreciationRepository { get; }

    public ResCarGroupAppService(
        IResCarGroupRepository repository,
        ResCarGroupManager manager,
        IResCarLineRepository carLineRepository,
        IResObjectItemDepreciationRepository depreciationRepository)
        : base(repository)
    {
        Manager = manager;
        CarGroupRepository = repository;
        CarLineRepository = carLineRepository;
        DepreciationRepository = depreciationRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResCarGroupPermissions.View;
        GetListPolicyName = ResCarGroupPermissions.View;
        CreatePolicyName = ResCarGroupPermissions.Create;
        UpdatePolicyName = ResCarGroupPermissions.Edit;
        DeletePolicyName = ResCarGroupPermissions.Delete;
    }

    public override async Task<ResCarGroupDto> CreateAsync(CreateResCarGroupDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await CarGroupRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResCarGroup:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResCarGroup:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        // Validate CarLineId if provided
        if (input.CarLineId.HasValue)
        {
            if (!await CarLineRepository.AnyAsync(x => x.Id == input.CarLineId.Value))
            {
                throw new UserFriendlyException(
                    L["ResCarGroup:CarLineNotFound"].Value
                );
                //throw new BusinessException("Master:ResCarGroup:CarLineNotFound");
            }
        }

        var entity = new ResCarGroup(
            GuidGenerator.Create(),
            input.CarLineId,
            input.Code,
            input.Name,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCarGroup, ResCarGroupDto>(entity);
    }

    public override async Task<ResCarGroupDto> UpdateAsync(Guid id, UpdateResCarGroupDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Validate CarLineId if provided
        if (input.CarLineId.HasValue)
        {
            if (!await CarLineRepository.AnyAsync(x => x.Id == input.CarLineId.Value))
            {
                throw new UserFriendlyException(
                    L["ResCarGroup:CarLineNotFound"].Value
                );
            }
        }

        // ⚠️ QUAN TRỌNG: Chỉ update CarLineId, Name, Description và Status, không update Code
        // Code is immutable, so no duplicate validation needed
        await Manager.UpdateAsync(entity, input.CarLineId, input.Name, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCarGroup, ResCarGroupDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Check if in use by Depreciation
        if (await DepreciationRepository.IsCarGroupInUseAsync(id))
        {
            throw new UserFriendlyException(L["ResCarGroup:InUseByDepreciation"]);
        }
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity!.UpdateStatus(ResCarGroupStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCarGroup>> CreateFilteredQueryAsync(GetResCarGroupsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by CarLineId
        if (input.CarLineId.HasValue)
        {
            query = query.Where(x => x.CarLineId == input.CarLineId.Value);
        }

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

    /// <summary>
    /// Get lookup list of active CarGroups for dropdown selection
    /// This method has lower permission requirement - only needs Default permission
    /// </summary>
    public virtual async Task<ListResultDto<ResCarGroupDto>> GetLookupListAsync()
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        
        // Only return active CarGroups, ordered by name
        query = query
            .Where(x => x.Status == ResCarGroupStatus.Active)
            .OrderBy(x => x.Name);

        var items = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<ResCarGroup>, List<ResCarGroupDto>>(items);

        return new ListResultDto<ResCarGroupDto>(dtos);
    }

    public virtual async Task<ImportResCarGroupResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportResCarGroupResultDto();
        var errors = new List<ImportResCarGroupErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["Master:ResCarGroup:ExcelFileInvalid"]);
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
            throw new UserFriendlyException(L["Master:ResCarGroup:ExcelFileInvalid"]);
        }

        // Track codes within the import file to detect duplicates
        var importCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process data rows (starting from row 2)
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportResCarGroupErrorDto>();

            // Get values
            var code = row.GetCellValue(headers.ContainsKey("MÃ") ? headers["MÃ"] : headers["CODE"]).Trim().ToUpperInvariant();
            var name = row.GetCellValue(headers.ContainsKey("TÊN") ? headers["TÊN"] : headers["NAME"]).Trim();
            var carLineCol = headers.ContainsKey("MÃ DÒNG XE") ? headers["MÃ DÒNG XE"] : (headers.ContainsKey("CARLINECODE") ? headers["CARLINECODE"] : -1);
            var carLineCode = carLineCol != -1 ? row.GetCellValue(carLineCol).Trim() : null;
            var descriptionCol = headers.ContainsKey("MÔ TẢ") ? headers["MÔ TẢ"] : (headers.ContainsKey("DESCRIPTION") ? headers["DESCRIPTION"] : -1);
            var description = descriptionCol != -1 ? row.GetCellValue(descriptionCol).Trim() : null;
            var statusCol = headers.ContainsKey("TRẠNG THÁI") ? headers["TRẠNG THÁI"] : (headers.ContainsKey("STATUS") ? headers["STATUS"] : -1);
            var statusText = statusCol != -1 ? row.GetCellValue(statusCol).Trim() : "Active";

            // Validate Code
            bool codeIsValid = true;
            if (string.IsNullOrWhiteSpace(code))
            {
                rowErrors.Add(new ImportResCarGroupErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarGroup:CodeRequired"],
                    Value = code
                });
                codeIsValid = false;
            }
            else if (code.Length > 50)
            {
                rowErrors.Add(new ImportResCarGroupErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarGroup:CodeMaxLength"],
                    Value = code
                });
                codeIsValid = false;
            }
            else if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
            {
                rowErrors.Add(new ImportResCarGroupErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarGroup:CodeInvalid"],
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
                    var message = L["Master:ResCarGroup:CodeDuplicateInFile"].Value.Replace("{0}", code);
                    rowErrors.Add(new ImportResCarGroupErrorDto
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
                    if (await CarGroupRepository.IsCodeExistsAsync(code))
                    {
                        var message = L["Master:ResCarGroup:CodeExists"].Value.Replace("{Code}", code);
                        rowErrors.Add(new ImportResCarGroupErrorDto
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
                rowErrors.Add(new ImportResCarGroupErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCarGroup:NameRequired"],
                    Value = name
                });
            }
            else if (name.Length > 250)
            {
                rowErrors.Add(new ImportResCarGroupErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCarGroup:NameMaxLength"],
                    Value = name
                });
            }

            // Validate Description
            if (description != null && description.Length > 500)
            {
                rowErrors.Add(new ImportResCarGroupErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Description",
                    Message = L["Master:ResCarGroup:DescriptionMaxLength"],
                    Value = description
                });
            }

            // Validate CarLineCode if provided
            Guid? carLineId = null;
            if (!string.IsNullOrWhiteSpace(carLineCode))
            {
                var carLine = await CarLineRepository.FirstOrDefaultAsync(x => x.Code == carLineCode.ToUpperInvariant());
                if (carLine == null)
                {
                    rowErrors.Add(new ImportResCarGroupErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "CarLineCode",
                        Message = L["Master:ResCarGroup:CarLineNotFound"],
                        Value = carLineCode
                    });
                }
                else
                {
                    carLineId = carLine.Id;
                }
            }

            // Parse Status
            ResCarGroupStatus status = ResCarGroupStatus.Active;
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                if (string.Equals(statusText, "Hoạt động", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    status = ResCarGroupStatus.Active;
                }
                else if (string.Equals(statusText, "Ngừng hoạt động", StringComparison.OrdinalIgnoreCase) || string.Equals(statusText, "Deactive", StringComparison.OrdinalIgnoreCase))
                {
                    status = ResCarGroupStatus.Deactive;
                }
                else if (!Enum.TryParse<ResCarGroupStatus>(statusText, true, out status))
                {
                    status = ResCarGroupStatus.Active; // Default to Active if invalid
                }
            }

            // If no errors, try to create the entity
            if (rowErrors.Count == 0 && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var entity = new ResCarGroup(
                        GuidGenerator.Create(),
                        carLineId,
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
                    if (message.Contains("Master:ResCarGroup:") || message.Contains("Master::ResCarGroup:"))
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
                    
                    rowErrors.Add(new ImportResCarGroupErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = message,
                        Value = ex.Data.Contains("Code") ? ex.Data["Code"]?.ToString() : code
                    });
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportResCarGroupErrorDto
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
        var worksheet = workbook.Worksheets.Add("Nhóm xe");

        // Set header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Set headers matching the import format
        worksheet.Cell(1, 1).Value = "Mã";
        worksheet.Cell(1, 2).Value = "Tên";
        worksheet.Cell(1, 3).Value = "Mã dòng xe";
        worksheet.Cell(1, 4).Value = "Mô tả";
        worksheet.Cell(1, 5).Value = "Trạng thái";

        // Add sample data row (row 2) with example values
        worksheet.Cell(2, 1).Value = "GROUP001";
        worksheet.Cell(2, 2).Value = "Sample Car Group";
        worksheet.Cell(2, 3).Value = "";
        worksheet.Cell(2, 4).Value = "Sample description";
        worksheet.Cell(2, 5).Value = "Active";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
