using ClosedXML.Excel;
using iOne.Master;
using iOne.Master.Helpers;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResCarCategories;
using iOne.Master.ResCarModels;
using iOne.ResCarBrands; // For CarBrand Repository
using iOne.ResCarCategories; // For Entity, Manager, Repository, Status enum
using iOne.ResCarLines; // For CarLine Repository
using iOne.ResCarModels; // For CarModel Repository
using iOne.ResMotorClasses; // For MotorClass Repository
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

namespace iOne.Master.ResCarCategories;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResCarCategoryPermissions.Default)]
public class ResCarCategoryAppService : CrudAppService<
    ResCarCategory,
    ResCarCategoryDto,
    Guid,
    GetResCarCategoriesInput,
    CreateResCarCategoryDto,
    UpdateResCarCategoryDto>, IResCarCategoryAppService
{
    protected ResCarCategoryManager Manager { get; }
    protected IResCarCategoryRepository CarCategoryRepository { get; }
    protected IResMotorClassRepository MotorClassRepository { get; }
    protected IResCarBrandRepository CarBrandRepository { get; }
    protected IResCarModelRepository CarModelRepository { get; }
    protected IResCarLineRepository CarLineRepository { get; }

    public ResCarCategoryAppService(
        IResCarCategoryRepository repository,
        ResCarCategoryManager manager,
        IResMotorClassRepository motorClassRepository,
        IResCarBrandRepository carBrandRepository,
        IResCarModelRepository carModelRepository,
        IResCarLineRepository carLineRepository)
        : base(repository)
    {
        Manager = manager;
        CarCategoryRepository = repository;
        MotorClassRepository = motorClassRepository;
        CarBrandRepository = carBrandRepository;
        CarModelRepository = carModelRepository;
        CarLineRepository = carLineRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResCarCategoryPermissions.View;
        GetListPolicyName = ResCarCategoryPermissions.View;
        CreatePolicyName = ResCarCategoryPermissions.Create;
        UpdatePolicyName = ResCarCategoryPermissions.Edit;
        DeletePolicyName = ResCarCategoryPermissions.Delete;
    }

    public override async Task<ResCarCategoryDto> CreateAsync(CreateResCarCategoryDto input)
    {
        // Validate duplicate code
        var normalizedCode = input.Code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            if (await CarCategoryRepository.IsCodeExistsAsync(normalizedCode))
            {
                throw new UserFriendlyException(
                    L["ResCarCategory:CodeExists"].Value.Replace("{Code}", normalizedCode)
                );
                //throw new BusinessException("Master:ResCarCategory:CodeExists")
                //    .WithData("Code", normalizedCode);
            }
        }

        var entity = new ResCarCategory(
            GuidGenerator.Create(),
            input.CarBrandId,
            input.CarModelId,
            input.MotorClassId,
            input.CarLineId,
            input.Code,
            input.Name,
            input.SeatNumber,
            input.Description,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCarCategory, ResCarCategoryDto>(entity);
    }

    public override async Task<ResCarCategoryDto> UpdateAsync(Guid id, UpdateResCarCategoryDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, SeatNumber, Description và Status, không update Code, CarBrandId, CarModelId, MotorClassId, CarLineId
        await Manager.UpdateAsync(entity, input.Name, input.SeatNumber, input.Description, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCarCategory, ResCarCategoryDto>(entity!);
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
        entity!.UpdateStatus(ResCarCategoryStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCarCategory>> CreateFilteredQueryAsync(GetResCarCategoriesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by CarBrandId
        if (input.CarBrandId.HasValue)
        {
            query = query.Where(x => x.CarBrandId == input.CarBrandId.Value);
        }

        // Filter by CarModelId
        if (input.CarModelId.HasValue)
        {
            query = query.Where(x => x.CarModelId == input.CarModelId.Value);
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

    public virtual async Task<ImportResCarCategoryResultDto> ImportExcelAsync(byte[] fileBytes)
    {
        var result = new ImportResCarCategoryResultDto();
        var errors = new List<ImportResCarCategoryErrorDto>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["Master:ResCarCategory:ExcelFileInvalid"]);
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

        // Get all entities for lookup
        var motorClasses = await MotorClassRepository.GetListAsync(includeDetails: true);
        var motorClassDict = motorClasses.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        var carBrands = await CarBrandRepository.GetListAsync(includeDetails: true);
        var carBrandDict = carBrands.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        var carModels = await CarModelRepository.GetListAsync(includeDetails: true);
        var carModelDict = carModels.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        var carLines = await CarLineRepository.GetListAsync(includeDetails: true);
        var carLineDict = carLines.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        // Validate required headers
        var requiredHeaders = new[] { "CODE", "NAME", "CARBRANDCODE", "MOTORCLASSCODE", "SEATNUMBER", "MÃ", "TÊN", "MÃ HÃNG XE", "MÃ PHÂN LOẠI ĐỘNG CƠ", "SỐ CHỖ NGỒI" };
        var hasRequired = (headers.ContainsKey("CODE") || headers.ContainsKey("MÃ")) &&
                          (headers.ContainsKey("NAME") || headers.ContainsKey("TÊN")) &&
                          (headers.ContainsKey("CARBRANDCODE") || headers.ContainsKey("MÃ HÃNG XE")) &&
                          (headers.ContainsKey("MOTORCLASSCODE") || headers.ContainsKey("MÃ PHÂN LOẠI ĐỘNG CƠ")) &&
                          (headers.ContainsKey("SEATNUMBER") || headers.ContainsKey("SỐ CHỖ NGỒI"));

        if (!hasRequired)
        {
            throw new UserFriendlyException(L["Master:ResCarCategory:ExcelFileInvalid"]);
        }

        // Track codes within the import file to detect duplicates
        var importCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process data rows (starting from row 2)
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportResCarCategoryErrorDto>();

            // Get values
            var code = row.GetCellValue(headers.ContainsKey("MÃ") ? headers["MÃ"] : headers["CODE"]).Trim().ToUpperInvariant();
            var name = row.GetCellValue(headers.ContainsKey("TÊN") ? headers["TÊN"] : headers["NAME"]).Trim();
            var carBrandCode = row.GetCellValue(headers.ContainsKey("MÃ HÃNG XE") ? headers["MÃ HÃNG XE"] : headers["CARBRANDCODE"]).Trim().ToUpperInvariant();
            var motorClassCode = row.GetCellValue(headers.ContainsKey("MÃ PHÂN LOẠI ĐỘNG CƠ") ? headers["MÃ PHÂN LOẠI ĐỘNG CƠ"] : headers["MOTORCLASSCODE"]).Trim().ToUpperInvariant();
            var seatNumberText = row.GetCellValue(headers.ContainsKey("SỐ CHỖ NGỒI") ? headers["SỐ CHỖ NGỒI"] : headers["SEATNUMBER"]).Trim();
            var carModelCol = headers.ContainsKey("MÃ MODEL XE") ? headers["MÃ MODEL XE"] : (headers.ContainsKey("CARMODELCODE") ? headers["CARMODELCODE"] : -1);
            var carModelCode = carModelCol != -1 ? row.GetCellValue(carModelCol).Trim().ToUpperInvariant() : null;
            var carLineCol = headers.ContainsKey("MÃ DÒNG XE") ? headers["MÃ DÒNG XE"] : (headers.ContainsKey("CARLINECODE") ? headers["CARLINECODE"] : -1);
            var carLineCode = carLineCol != -1 ? row.GetCellValue(carLineCol).Trim().ToUpperInvariant() : null;
            var descriptionCol = headers.ContainsKey("MÔ TẢ") ? headers["MÔ TẢ"] : (headers.ContainsKey("DESCRIPTION") ? headers["DESCRIPTION"] : -1);
            var description = descriptionCol != -1 ? row.GetCellValue(descriptionCol).Trim() : null;
            var statusCol = headers.ContainsKey("TRẠNG THÁI") ? headers["TRẠNG THÁI"] : (headers.ContainsKey("STATUS") ? headers["STATUS"] : -1);
            var statusText = statusCol != -1 ? row.GetCellValue(statusCol).Trim() : "Active";

            // Validate Code
            if (string.IsNullOrWhiteSpace(code))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarCategory:CodeRequired"],
                    Value = code
                });
            }
            else if (code.Length > 50)
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarCategory:CodeMaxLength"],
                    Value = code
                });
            }
            else if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Code",
                    Message = L["Master:ResCarCategory:CodeInvalid"],
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
                    var message = L["Master:ResCarCategory:CodeDuplicateInFile"].Value.Replace("{0}", code);
                    rowErrors.Add(new ImportResCarCategoryErrorDto
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
                    if (await CarCategoryRepository.IsCodeExistsAsync(code))
                    {
                        var message = L["Master:ResCarCategory:CodeExists"].Value.Replace("{Code}", code);
                        rowErrors.Add(new ImportResCarCategoryErrorDto
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
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCarCategory:NameRequired"],
                    Value = name
                });
            }
            else if (name.Length > 250)
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Name",
                    Message = L["Master:ResCarCategory:NameMaxLength"],
                    Value = name
                });
            }

            // Validate CarBrandCode
            Guid? carBrandId = null;
            if (string.IsNullOrWhiteSpace(carBrandCode))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "CarBrandCode",
                    Message = L["Master:ResCarCategory:CarBrandIdRequired"],
                    Value = carBrandCode
                });
            }
            else if (!carBrandDict.ContainsKey(carBrandCode))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "CarBrandCode",
                    Message = L["Master:ResCarCategory:CarBrandNotFound", carBrandCode],
                    Value = carBrandCode
                });
            }
            else
            {
                carBrandId = carBrandDict[carBrandCode].Id;
            }

            // Validate MotorClassCode
            Guid? motorClassId = null;
            if (string.IsNullOrWhiteSpace(motorClassCode))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "MotorClassCode",
                    Message = L["Master:ResCarCategory:MotorClassIdRequired"],
                    Value = motorClassCode
                });
            }
            else if (!motorClassDict.ContainsKey(motorClassCode))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "MotorClassCode",
                    Message = L["Master:ResCarCategory:MotorClassNotFound", motorClassCode],
                    Value = motorClassCode
                });
            }
            else
            {
                motorClassId = motorClassDict[motorClassCode].Id;
            }

            // Validate SeatNumber
            int seatNumber = 0;
            if (string.IsNullOrWhiteSpace(seatNumberText))
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "SeatNumber",
                    Message = L["Master:ResCarCategory:SeatNumberRequired"],
                    Value = seatNumberText
                });
            }
            else if (!int.TryParse(seatNumberText, out seatNumber) || seatNumber < 0 || seatNumber > 99)
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "SeatNumber",
                    Message = L["Master:ResCarCategory:SeatNumberRange"],
                    Value = seatNumberText
                });
            }

            // Validate optional CarModelCode
            Guid? carModelId = null;
            if (!string.IsNullOrWhiteSpace(carModelCode))
            {
                if (!carModelDict.ContainsKey(carModelCode))
                {
                    rowErrors.Add(new ImportResCarCategoryErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "CarModelCode",
                        Message = L["Master:ResCarCategory:CarModelNotFound", carModelCode],
                        Value = carModelCode
                    });
                }
                else
                {
                    carModelId = carModelDict[carModelCode].Id;
                }
            }

            // Validate optional CarLineCode
            Guid? carLineId = null;
            if (!string.IsNullOrWhiteSpace(carLineCode))
            {
                if (!carLineDict.ContainsKey(carLineCode))
                {
                    rowErrors.Add(new ImportResCarCategoryErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "CarLineCode",
                        Message = L["Master:ResCarCategory:CarLineNotFound", carLineCode],
                        Value = carLineCode
                    });
                }
                else
                {
                    carLineId = carLineDict[carLineCode].Id;
                }
            }

            // Validate Description
            if (description != null && description.Length > 500)
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Description",
                    Message = L["Master:ResCarCategory:DescriptionMaxLength"],
                    Value = description
                });
            }

            // Parse Status
            ResCarCategoryStatus status = ResCarCategoryStatus.Active;
            bool statusIsValid = true;

            if (string.IsNullOrWhiteSpace(statusText) || string.Equals(statusText, "Active", StringComparison.OrdinalIgnoreCase))
            {
                status = ResCarCategoryStatus.Active;
            }
            else if (string.Equals(statusText, "Deactive", StringComparison.OrdinalIgnoreCase))
            {
                status = ResCarCategoryStatus.Deactive;
            }
            else
            {
                rowErrors.Add(new ImportResCarCategoryErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Status",
                    Message = L["Master:ResCarBrand:StatusInvalid"],
                    Value = statusText
                });
                statusIsValid = false;
            }

            // If no errors, try to create the entity
            if (rowErrors.Count == 0 &&
                !string.IsNullOrWhiteSpace(code) &&
                !string.IsNullOrWhiteSpace(name) &&
                carBrandId.HasValue &&
                motorClassId.HasValue)
            {
                try
                {
                    var entity = new ResCarCategory(
                        GuidGenerator.Create(),
                        carBrandId.Value,
                        carModelId,
                        motorClassId.Value,
                        carLineId,
                        code,
                        name,
                        seatNumber,
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
                    if (message.Contains("Master:ResCarCategory:") || message.Contains("Master::ResCarCategory:"))
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

                    rowErrors.Add(new ImportResCarCategoryErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Message = message,
                        Value = ex.Data.Contains("Code") ? ex.Data["Code"]?.ToString() : code
                    });
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportResCarCategoryErrorDto
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
        var worksheet = workbook.Worksheets.Add("Phân khúc xe");

        // Set header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Set headers matching the import format
        worksheet.Cell(1, 1).Value = "Mã";
        worksheet.Cell(1, 2).Value = "Tên";
        worksheet.Cell(1, 3).Value = "Mã hãng xe";
        worksheet.Cell(1, 4).Value = "Mã phân loại động cơ";
        worksheet.Cell(1, 5).Value = "Số chỗ ngồi";
        worksheet.Cell(1, 6).Value = "Mã model xe";
        worksheet.Cell(1, 7).Value = "Mã dòng xe";
        worksheet.Cell(1, 8).Value = "Mô tả";
        worksheet.Cell(1, 9).Value = "Trạng thái";

        // Add sample data row (row 2) with example values
        worksheet.Cell(2, 1).Value = "SEDAN_4SEAT";
        worksheet.Cell(2, 2).Value = "Sedan 4 Seats";
        worksheet.Cell(2, 3).Value = "TOYOTA";
        worksheet.Cell(2, 4).Value = "MOTOR_CLASS_1";
        worksheet.Cell(2, 5).Value = "4";
        worksheet.Cell(2, 6).Value = "CAMRY";
        worksheet.Cell(2, 7).Value = "";
        worksheet.Cell(2, 8).Value = "Mô tả";
        worksheet.Cell(2, 9).Value = "Active";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
