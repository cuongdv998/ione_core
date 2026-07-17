using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iOne.Product;
using iOne.Product.ProTableRateLines;
using iOne.Product.Localization;
using iOne.Product.Permissions;
using iOne.ProAttributes;
using iOne.ProCoverages;
using iOne.ProTableRates;
using iOne.ProTableRateLines;
using iOne.ProTableRateVariables; // For Entity, Manager, Repository
using iOne.ResChannels;
using iOne.ResPartners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace iOne.Product.ProTableRateLines;

[RemoteService(Name = ProductRemoteServiceConsts.RemoteServiceName)]
[Authorize(ProTableRatePermissions.Default)]
public class ProTableRateLineAppService : CrudAppService<
    ProTableRateLine,
    ProTableRateLineDto,
    Guid,
    GetProTableRateLinesInput,
    CreateProTableRateLineDto,
    UpdateProTableRateLineDto>, IProTableRateLineAppService
{
    protected ProTableRateLineManager Manager { get; }
    protected IProTableRateRepository TableRateRepository { get; }
    protected IProCoverageRepository CoverageRepository { get; }
    protected IResChannelRepository ChannelRepository { get; }
    protected IResPartnerRepository PartnerRepository { get; }
    protected IRepository<ProAttribute, Guid> AttributeRepository { get; }

    public ProTableRateLineAppService(
        IProTableRateLineRepository repository,
        ProTableRateLineManager manager,
        IProTableRateRepository tableRateRepository,
        IProCoverageRepository coverageRepository,
        IResChannelRepository channelRepository,
        IResPartnerRepository partnerRepository,
        IRepository<ProAttribute, Guid> attributeRepository)
        : base(repository)
    {
        Manager = manager;
        TableRateRepository = tableRateRepository;
        CoverageRepository = coverageRepository;
        ChannelRepository = channelRepository;
        PartnerRepository = partnerRepository;
        AttributeRepository = attributeRepository;
        LocalizationResource = typeof(ProductResource);
        GetPolicyName = ProTableRatePermissions.View;
        GetListPolicyName = ProTableRatePermissions.View;
        CreatePolicyName = ProTableRatePermissions.Create;
        UpdatePolicyName = ProTableRatePermissions.Edit;
        DeletePolicyName = ProTableRatePermissions.Delete;
    }

    /// <summary>
    /// Helper method to compare nullable decimals for equality.
    /// Both null = equal, both not null and equal = equal, otherwise not equal.
    /// </summary>
    private static bool AreRatesEqual(decimal? rate1, decimal? rate2)
    {
        if (!rate1.HasValue && !rate2.HasValue) return true;
        if (rate1.HasValue && rate2.HasValue) return rate1.Value == rate2.Value;
        return false;
    }

    /// <summary>
    /// Helper method to compare nullable Guids for equality.
    /// Both null = equal, both not null and equal = equal, otherwise not equal.
    /// </summary>
    private static bool AreGuidsEqual(Guid? guid1, Guid? guid2)
    {
        if (!guid1.HasValue && !guid2.HasValue) return true;
        if (guid1.HasValue && guid2.HasValue) return guid1.Value == guid2.Value;
        return false;
    }

    /// <summary>
    /// Helper method to compare condition JSON strings for equality.
    /// Parses both JSON strings and compares the dictionaries/objects.
    /// Handles arrays for IN_RANGE, IN, NOT_IN operators.
    /// </summary>
    private static bool AreConditionsEqual(string? condition1, string? condition2)
    {
        // Both null or empty = equal
        if (string.IsNullOrWhiteSpace(condition1) && string.IsNullOrWhiteSpace(condition2))
            return true;

        // One null/empty, one not = not equal
        if (string.IsNullOrWhiteSpace(condition1) || string.IsNullOrWhiteSpace(condition2))
            return false;

        try
        {
            // Parse both JSON strings to dictionaries
            var dict1 = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(condition1);
            var dict2 = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(condition2);

            // Both null = equal
            if (dict1 == null && dict2 == null) return true;

            // One null, one not = not equal
            if (dict1 == null || dict2 == null) return false;

            // Different number of keys = not equal
            if (dict1.Count != dict2.Count) return false;

            // Compare each key-value pair
            foreach (var kvp in dict1)
            {
                if (!dict2.ContainsKey(kvp.Key))
                    return false;

                var value1 = kvp.Value;
                var value2 = dict2[kvp.Key];

                // Compare values based on their JSON value kind
                if (!AreJsonValuesEqual(value1, value2))
                    return false;
            }

            return true;
        }
        catch
        {
            // If JSON parsing fails, fall back to string comparison
            return condition1 == condition2;
        }
    }

    /// <summary>
    /// Helper method to compare JsonElement values for equality.
    /// Handles different data types including arrays.
    /// </summary>
    private static bool AreJsonValuesEqual(JsonElement value1, JsonElement value2)
    {
        // Different value kinds = not equal
        if (value1.ValueKind != value2.ValueKind)
            return false;

        return value1.ValueKind switch
        {
            JsonValueKind.String => value1.GetString() == value2.GetString(),
            JsonValueKind.Number => value1.GetRawText() == value2.GetRawText(), // Compare raw text to handle precision
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Null => true,
            JsonValueKind.Array => AreJsonArraysEqual(value1, value2),
            JsonValueKind.Object => AreJsonObjectsEqual(value1, value2),
            _ => false
        };
    }

    /// <summary>
    /// Helper method to compare JsonElement arrays for equality.
    /// Arrays must have same length and same elements in same order.
    /// </summary>
    private static bool AreJsonArraysEqual(JsonElement array1, JsonElement array2)
    {
        if (array1.GetArrayLength() != array2.GetArrayLength())
            return false;

        var enumerator1 = array1.EnumerateArray();
        var enumerator2 = array2.EnumerateArray();

        while (enumerator1.MoveNext() && enumerator2.MoveNext())
        {
            if (!AreJsonValuesEqual(enumerator1.Current, enumerator2.Current))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Helper method to compare JsonElement objects for equality.
    /// Objects must have same keys and same values.
    /// </summary>
    private static bool AreJsonObjectsEqual(JsonElement obj1, JsonElement obj2)
    {
        if (obj1.GetPropertyCount() != obj2.GetPropertyCount())
            return false;

        foreach (var prop in obj1.EnumerateObject())
        {
            if (!obj2.TryGetProperty(prop.Name, out var value2))
                return false;

            if (!AreJsonValuesEqual(prop.Value, value2))
                return false;
        }

        return true;
    }

    public override async Task<ProTableRateLineDto> CreateAsync(CreateProTableRateLineDto input)
    {
        var entity = new ProTableRateLine(
            GuidGenerator.Create(),
            input.TableRateId,
            input.Name,
            input.Condition,
            input.EffectDate,
            input.CoverageId,
            input.ChannelId,
            input.PartnerId,
            input.MinimumRate,
            input.BaseRate,
            input.FlatRate,
            input.MaxDiscount,
            input.LoadingRate,
            input.Loading,
            input.ExpireDate
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ProTableRateLine, ProTableRateLineDto>(entity);
    }

    public override async Task<ProTableRateLineDto> UpdateAsync(Guid id, UpdateProTableRateLineDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Condition,
            input.EffectDate,
            input.CoverageId,
            input.ChannelId,
            input.PartnerId,
            input.MinimumRate,
            input.BaseRate,
            input.FlatRate,
            input.MaxDiscount,
            input.LoadingRate,
            input.Loading,
            input.ExpireDate);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ProTableRateLine, ProTableRateLineDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ProTableRateLine>> CreateFilteredQueryAsync(GetProTableRateLinesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by TableRateId
        if (input.TableRateId.HasValue)
        {
            query = query.Where(x => x.TableRateId == input.TableRateId.Value);
        }

        // Filter by CoverageId
        if (input.CoverageId.HasValue)
        {
            query = query.Where(x => x.CoverageId == input.CoverageId.Value);
        }

        // Filter by ChannelId
        if (input.ChannelId.HasValue)
        {
            query = query.Where(x => x.ChannelId == input.ChannelId.Value);
        }

        // Filter by PartnerId
        if (input.PartnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == input.PartnerId.Value);
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        return query;
    }

    public virtual async Task<byte[]> ExportTemplateAsync(Guid tableRateId)
    {
        // Load table rate with variables and attributes
        var tableRateQuery = await TableRateRepository.GetQueryableAsync();
        var tableRate = await tableRateQuery
            .Include(x => x.Variables)
                .ThenInclude(v => v.Attribute)
            .FirstOrDefaultAsync(x => x.Id == tableRateId);

        if (tableRate == null)
        {
            throw new EntityNotFoundException(typeof(ProTableRate), tableRateId);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ProTableRateLine");

        // Fixed columns before dynamic variable columns
        int col = 1;
        worksheet.Cell(1, col++).Value = "STT";
        worksheet.Cell(1, col++).Value = "Mã phạm vi";
        worksheet.Cell(1, col++).Value = "Tên phạm vi";
        worksheet.Cell(1, col++).Value = "Mã kênh";
        worksheet.Cell(1, col++).Value = "Mã đối tác";
        worksheet.Cell(1, col++).Value = "Tên phí";
        worksheet.Cell(1, col++).Value = "Ngày hiệu lực";
        worksheet.Cell(1, col++).Value = "Ngày hết hạn";
        worksheet.Cell(1, col++).Value = "Phí tối thiểu";
        worksheet.Cell(1, col++).Value = "Phí theo %";
        worksheet.Cell(1, col++).Value = "Phí cố định";
        worksheet.Cell(1, col++).Value = "Tăng/giảm phí";
        worksheet.Cell(1, col++).Value = "Giảm giá tối đa";

        // Dynamic columns from variables
        var yellowColor = XLColor.FromHtml("#FFFF00");
        var variableColumns = new List<(int Col, ProTableRateVariable Variable)>();
        
        if (tableRate.Variables != null && tableRate.Variables.Any())
        {
            foreach (var variable in tableRate.Variables.OrderBy(v => v.Attribute?.Name ?? ""))
            {
                var attribute = variable.Attribute;
                if (attribute != null)
                {
                    var headerText = $"{attribute.Code} - {attribute.Name}/{attribute.Code}";
                    worksheet.Cell(1, col).Value = headerText;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = yellowColor;
                    variableColumns.Add((col, variable));
                    col++;
                }
            }
        }

        // Style header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        // Note: Yellow background for dynamic columns is already set above, so we only set gray for non-yellow cells
        // Set gray background for all fixed columns (first 13 columns)
        for (int c = 1; c <= 13; c++)
        {
            worksheet.Cell(1, c).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // // Add sample data rows (2 rows as per requirements)
        // // Row 2
        int row = 2;
        // worksheet.Cell(row, 1).Value = 1;
        // worksheet.Cell(row, 2).Value = "COV001";
        // worksheet.Cell(row, 3).Value = "Bảo hiểm thân vỏ";
        // worksheet.Cell(row, 4).Value = "CH001";
        // worksheet.Cell(row, 5).Value = "PT001";
        // worksheet.Cell(row, 6).Value = "Phí cơ bản";
        // var effectDate1 = DateTime.Now;
        // var expireDate1 = DateTime.Now.AddYears(1);
        // worksheet.Cell(row, 7).Value = effectDate1;
        // worksheet.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";
        // worksheet.Cell(row, 8).Value = expireDate1;
        // worksheet.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy";
        // worksheet.Cell(row, 9).Value = 5.5;
        // worksheet.Cell(row, 10).Value = 100000;
        // worksheet.Cell(row, 11).Value = 10;
        //
        // // Apply yellow background to dynamic column sample data
        // foreach (var (colNum, _) in variableColumns)
        // {
        //     worksheet.Cell(row, colNum).Style.Fill.BackgroundColor = yellowColor;
        // }
        //
        // // Row 3 (second sample row)
        // row = 3;
        // worksheet.Cell(row, 1).Value = 2;
        // worksheet.Cell(row, 2).Value = "COV002";
        // worksheet.Cell(row, 3).Value = "Bảo hiểm TNDS";
        // worksheet.Cell(row, 4).Value = "CH002";
        // worksheet.Cell(row, 5).Value = "PT002";
        // worksheet.Cell(row, 6).Value = "Phí bổ sung";
        // var effectDate2 = DateTime.Now.AddMonths(1);
        // var expireDate2 = DateTime.Now.AddYears(1).AddMonths(1);
        // worksheet.Cell(row, 7).Value = effectDate2;
        // worksheet.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";
        // worksheet.Cell(row, 8).Value = expireDate2;
        // worksheet.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy";
        // worksheet.Cell(row, 9).Value = 3.0;
        // worksheet.Cell(row, 10).Value = 50000;
        // worksheet.Cell(row, 11).Value = 5;

        // Apply yellow background to dynamic column sample data
        foreach (var (colNum, _) in variableColumns)
        {
            worksheet.Cell(row, colNum).Style.Fill.BackgroundColor = yellowColor;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public virtual async Task<ImportProTableRateLineResultDto> ImportExcelAsync(byte[] fileBytes, Guid tableRateId)
    {
        var result = new ImportProTableRateLineResultDto();
        var errors = new List<ImportProTableRateLineErrorDto>();

        // Load table rate with variables and attributes
        var tableRateQuery = await TableRateRepository.GetQueryableAsync();
        var tableRate = await tableRateQuery
            .Include(x => x.Variables)
                .ThenInclude(v => v.Attribute)
            .FirstOrDefaultAsync(x => x.Id == tableRateId);

        if (tableRate == null)
        {
            throw new EntityNotFoundException(typeof(ProTableRate), tableRateId);
        }

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new UserFriendlyException(L["Product:ProTableRateLine:ExcelFileInvalid"]);
        }

        // Read header row - dynamically detect all columns
        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>();
        int maxCol = 100; // Reasonable max - will stop when we hit consecutive empty columns
        
        for (int c = 1; c <= maxCol; c++)
        {
            var cell = headerRow.Cell(c);
            if (cell.IsEmpty())
            {
                // If we've passed the fixed columns (13) and hit empty, check a few more before stopping
                if (c > 13)
                {
                    // Check next 5 columns - if all empty, stop
                    bool allEmpty = true;
                    for (int check = c; check < c + 5 && check <= maxCol; check++)
                    {
                        if (!headerRow.Cell(check).IsEmpty())
                        {
                            allEmpty = false;
                            break;
                        }
                    }
                    if (allEmpty) break;
                }
                continue;
            }
            
            var headerValue = cell.DataType == XLDataType.DateTime 
                ? cell.GetDateTime().ToString("dd/MM/yyyy")
                : cell.DataType == XLDataType.Number
                    ? cell.GetDouble().ToString()
                    : cell.GetString();
            headerValue = headerValue?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                headers[headerValue.ToUpperInvariant()] = c;
            }
        }

        // Validate required fixed headers
        var requiredHeaders = new[] { "MÃ PHẠM VI", "NGÀY HIỆU LỰC", "NGÀY HẾT HẠN" };
        foreach (var requiredHeader in requiredHeaders)
        {
            if (!headers.ContainsKey(requiredHeader))
            {
                throw new UserFriendlyException(L["Product:ProTableRateLine:ExcelMissingHeader", requiredHeader]);
            }
        }

        // Build attribute column mapping
        var attributeColumnMap = new Dictionary<string, (int Col, ProTableRateVariable Variable)>();
        if (tableRate.Variables != null && tableRate.Variables.Any())
        {
            foreach (var variable in tableRate.Variables)
            {
                if (variable.Attribute != null)
                {
                    var attributeCode = variable.Attribute.Code.ToUpperInvariant();
                    var attributeName = variable.Attribute.Name;
                    // Try to find column: "Code - Name/Code" (export/template), then "Name/Code", Code, Name
                    var possibleKeys = new[]
                    {
                        $"{attributeCode} - {attributeName}/{attributeCode}".ToUpperInvariant(),
                        $"{attributeName}/{attributeCode}".ToUpperInvariant(),
                        attributeCode,
                        attributeName.ToUpperInvariant()
                    };

                    foreach (var key in possibleKeys)
                    {
                        if (headers.ContainsKey(key))
                        {
                            attributeColumnMap[attributeCode] = (headers[key], variable);
                            break;
                        }
                    }
                }
            }
        }

        // Load lookup data
        var coverages = await CoverageRepository.GetListAsync(includeDetails: true);
        var coverageDict = coverages.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        var channels = await ChannelRepository.GetListAsync(includeDetails: true);
        var channelDict = channels.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        var partners = await PartnerRepository.GetListAsync(includeDetails: true);
        var partnerDict = partners.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        var attributes = await AttributeRepository.GetListAsync(includeDetails: true);
        var attributeDict = attributes.ToDictionary(x => x.Code.ToUpperInvariant(), x => x);

        // Get existing lines for date overlap checking
        var existingLinesQuery = await Repository.GetQueryableAsync();
        var existingLines = await existingLinesQuery
            .Where(x => x.TableRateId == tableRateId && !x.IsDeleted)
            .ToListAsync();

        // Track import rows for in-file overlap checking (all rows, not just successful ones)
        var importRows = new List<(Guid? CoverageId, Guid? ChannelId, Guid? PartnerId, string Condition, DateTime EffectDate, DateTime? ExpireDate, int RowNumber)>();

        // Process data rows
        int rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            result.TotalRows++;
            var row = worksheet.Row(rowNumber);
            var rowErrors = new List<ImportProTableRateLineErrorDto>();

            // Helper method to get cell value
            string GetCellValue(int col)
            {
                var cell = row.Cell(col);
                if (cell.DataType == XLDataType.DateTime)
                    return cell.GetDateTime().ToString("dd/MM/yyyy");
                if (cell.DataType == XLDataType.Number)
                    return cell.GetDouble().ToString(CultureInfo.InvariantCulture);
                return cell.GetString() ?? "";
            }

            // Parse fixed columns
            var coverageCode = headers.ContainsKey("MÃ PHẠM VI") ? GetCellValue(headers["MÃ PHẠM VI"]).Trim() : "";
            var coverageName = headers.ContainsKey("TÊN PHẠM VI") ? GetCellValue(headers["TÊN PHẠM VI"]).Trim() : "";
            var channelCode = headers.ContainsKey("MÃ KÊNH") ? GetCellValue(headers["MÃ KÊNH"]).Trim() : "";
            var partnerCode = headers.ContainsKey("MÃ ĐỐI TÁC") ? GetCellValue(headers["MÃ ĐỐI TÁC"]).Trim() : "";
            var name = headers.ContainsKey("TÊN PHÍ") ? GetCellValue(headers["TÊN PHÍ"]).Trim() : "";
            var effectDateText = GetCellValue(headers["NGÀY HIỆU LỰC"]).Trim();
            var expireDateText = GetCellValue(headers["NGÀY HẾT HẠN"]).Trim();
            var minimumRateText = headers.ContainsKey("PHÍ TỐI THIỂU") ? GetCellValue(headers["PHÍ TỐI THIỂU"]).Trim() : "";
            var baseRateText = headers.ContainsKey("PHÍ THEO %") ? GetCellValue(headers["PHÍ THEO %"]).Trim() : "";
            var flatRateText = headers.ContainsKey("PHÍ CỐ ĐỊNH") ? GetCellValue(headers["PHÍ CỐ ĐỊNH"]).Trim() : "";
            var loadingText = headers.ContainsKey("TĂNG/GIẢM PHÍ") ? GetCellValue(headers["TĂNG/GIẢM PHÍ"]).Trim() : "";
            var maxDiscountText = headers.ContainsKey("GIẢM GIÁ TỐI ĐA") ? GetCellValue(headers["GIẢM GIÁ TỐI ĐA"]).Trim() : "";

            // Validate coverage code
            Guid? coverageId = null;
            if (string.IsNullOrWhiteSpace(coverageCode))
            {
                rowErrors.Add(new ImportProTableRateLineErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Mã phạm vi",
                    Message = L["Product:ProTableRateLine:CoverageCodeRequired"],
                    Value = coverageCode
                });
            }
            else
            {
                var upperCode = coverageCode.ToUpperInvariant();
                if (!coverageDict.ContainsKey(upperCode))
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Mã phạm vi",
                        Message = L["Product:ProTableRateLine:CoverageCodeNotFound", coverageCode],
                        Value = coverageCode
                    });
                }
                else
                {
                    coverageId = coverageDict[upperCode].Id;
                }
            }

            // Validate dates
            DateTime effectDate = default;
            DateTime? expireDate = null;

            if (string.IsNullOrWhiteSpace(effectDateText))
            {
                rowErrors.Add(new ImportProTableRateLineErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Ngày hiệu lực",
                    Message = L["Product:ProTableRateLine:EffectDateRequired"],
                    Value = effectDateText
                });
            }
            else if (!DateTime.TryParseExact(effectDateText, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy" }, null, System.Globalization.DateTimeStyles.None, out effectDate))
            {
                rowErrors.Add(new ImportProTableRateLineErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Ngày hiệu lực",
                    Message = L["Product:ProTableRateLine:InvalidDateFormat"],
                    Value = effectDateText
                });
            }

            if (string.IsNullOrWhiteSpace(expireDateText))
            {
                rowErrors.Add(new ImportProTableRateLineErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Ngày hết hạn",
                    Message = L["Product:ProTableRateLine:ExpireDateRequired"],
                    Value = expireDateText
                });
            }
            else if (!DateTime.TryParseExact(expireDateText, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy" }, null, System.Globalization.DateTimeStyles.None, out var parsedExpireDate))
            {
                rowErrors.Add(new ImportProTableRateLineErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "Ngày hết hạn",
                    Message = L["Product:ProTableRateLine:InvalidDateFormat"],
                    Value = expireDateText
                });
            }
            else
            {
                expireDate = parsedExpireDate;
                if (effectDate != default && expireDate <= effectDate)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Ngày hết hạn",
                        Message = L["Product:ProTableRateLine:ExpireDateMustBeGreater"],
                        Value = expireDateText
                    });
                }
            }

            // Validate numeric fields
            decimal? minimumRate = null;
            if (!string.IsNullOrWhiteSpace(minimumRateText))
            {
                if (!decimal.TryParse(minimumRateText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedMinimumRate) || parsedMinimumRate < 0)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Phí tối thiểu",
                        Message = L["Product:ProTableRateLine:MinimumRateInvalid"],
                        Value = minimumRateText
                    });
                }
                else
                {
                    minimumRate = parsedMinimumRate;
                }
            }

            decimal? baseRate = null;
            if (!string.IsNullOrWhiteSpace(baseRateText))
            {
                if (!decimal.TryParse(baseRateText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedBaseRate) || parsedBaseRate < 0 || parsedBaseRate > 100)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Phí theo %",
                        Message = L["Product:ProTableRateLine:BaseRateInvalid"],
                        Value = baseRateText
                    });
                }
                else
                {
                    baseRate = parsedBaseRate;
                }
            }

            decimal? flatRate = null;
            if (!string.IsNullOrWhiteSpace(flatRateText))
            {
                if (!decimal.TryParse(flatRateText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedFlatRate) || parsedFlatRate < 0)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Phí cố định",
                        Message = L["Product:ProTableRateLine:FlatRateInvalid"],
                        Value = flatRateText
                    });
                }
                else
                {
                    flatRate = parsedFlatRate;
                }
            }

            decimal? loading = null;
            if (!string.IsNullOrWhiteSpace(loadingText))
            {
                if (!decimal.TryParse(loadingText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedLoading))
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Tăng/giảm phí",
                        Message = L["Product:ProTableRateLine:LoadingInvalid"],
                        Value = loadingText
                    });
                }
                else
                {
                    loading = parsedLoading;
                }
            }

            decimal? maxDiscount = null;
            if (!string.IsNullOrWhiteSpace(maxDiscountText))
            {
                if (!decimal.TryParse(maxDiscountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedMaxDiscount) || parsedMaxDiscount < 0)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Giảm giá tối đa",
                        Message = L["Product:ProTableRateLine:MaxDiscountInvalid"],
                        Value = maxDiscountText
                    });
                }
                else
                {
                    maxDiscount = parsedMaxDiscount;
                }
            }

            // Validate channel code
            Guid? channelId = null;
            if (!string.IsNullOrWhiteSpace(channelCode))
            {
                var upperCode = channelCode.ToUpperInvariant();
                if (!channelDict.ContainsKey(upperCode))
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Mã kênh",
                        Message = L["Product:ProTableRateLine:ChannelCodeNotFound", channelCode],
                        Value = channelCode
                    });
                }
                else
                {
                    channelId = channelDict[upperCode].Id;
                }
            }

            // Validate partner code
            Guid? partnerId = null;
            if (!string.IsNullOrWhiteSpace(partnerCode))
            {
                var upperCode = partnerCode.ToUpperInvariant();
                if (!partnerDict.ContainsKey(upperCode))
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Mã đối tác",
                        Message = L["Product:ProTableRateLine:PartnerCodeNotFound", partnerCode],
                        Value = partnerCode
                    });
                }
                else
                {
                    partnerId = partnerDict[upperCode].Id;
                }
            }

            // Parse and validate attribute values
            var conditionObject = new Dictionary<string, object>();
            foreach (var (attrCode, (colNum, variable)) in attributeColumnMap)
            {
                var attrValue = GetCellValue(colNum).Trim();
                if (!string.IsNullOrWhiteSpace(attrValue))
                {
                    var attribute = attributeDict.ContainsKey(attrCode) ? attributeDict[attrCode] : null;
                    if (attribute != null)
                    {
                        object? parsedValue = null;
                        bool isValid = false;
                        var operatorType = variable.Operator?.ToUpperInvariant() ?? "";

                        // Handle operators that require arrays
                        if (operatorType == "IN_RANGE")
                        {
                            // Parse comma-separated range format: "10,20" or "88,99"
                            var parts = attrValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                var minStr = parts[0].Trim();
                                var maxStr = parts[1].Trim();

                                // Validate that min and max strings are not empty after trimming
                                if (string.IsNullOrWhiteSpace(minStr) || string.IsNullOrWhiteSpace(maxStr))
                                {
                                    rowErrors.Add(new ImportProTableRateLineErrorDto
                                    {
                                        RowNumber = rowNumber,
                                        Field = attribute.Name,
                                        Message = L["Product:ProTableRateLine:InvalidRangeFormat", attribute.Name],
                                        Value = attrValue
                                    });
                                }
                                else if (attribute.DataType == ProAttributeDataType.Int)
                                {
                                    if (int.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var min) && 
                                        int.TryParse(maxStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var max))
                                    {
                                        if (min <= max)
                                        {
                                            parsedValue = new[] { min, max };
                                            isValid = true;
                                        }
                                        else
                                        {
                                            rowErrors.Add(new ImportProTableRateLineErrorDto
                                            {
                                                RowNumber = rowNumber,
                                                Field = attribute.Name,
                                                Message = L["Product:ProTableRateLine:InvalidRangeMinMax", attribute.Name],
                                                Value = attrValue
                                            });
                                        }
                                    }
                                    else
                                    {
                                        // Specific error when parsing fails
                                        rowErrors.Add(new ImportProTableRateLineErrorDto
                                        {
                                            RowNumber = rowNumber,
                                            Field = attribute.Name,
                                            Message = L["Product:ProTableRateLine:InvalidRangeFormat", attribute.Name],
                                            Value = attrValue
                                        });
                                    }
                                }
                                else if (attribute.DataType == ProAttributeDataType.Float)
                                {
                                    if (decimal.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var min) && 
                                        decimal.TryParse(maxStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var max))
                                    {
                                        if (min <= max)
                                        {
                                            parsedValue = new[] { min, max };
                                            isValid = true;
                                        }
                                        else
                                        {
                                            rowErrors.Add(new ImportProTableRateLineErrorDto
                                            {
                                                RowNumber = rowNumber,
                                                Field = attribute.Name,
                                                Message = L["Product:ProTableRateLine:InvalidRangeMinMax", attribute.Name],
                                                Value = attrValue
                                            });
                                        }
                                    }
                                    else
                                    {
                                        // Specific error when parsing fails
                                        rowErrors.Add(new ImportProTableRateLineErrorDto
                                        {
                                            RowNumber = rowNumber,
                                            Field = attribute.Name,
                                            Message = L["Product:ProTableRateLine:InvalidRangeFormat", attribute.Name],
                                            Value = attrValue
                                        });
                                    }
                                }
                                else
                                {
                                    // Data type not supported for IN_RANGE
                                    rowErrors.Add(new ImportProTableRateLineErrorDto
                                    {
                                        RowNumber = rowNumber,
                                        Field = attribute.Name,
                                        Message = L["Product:ProTableRateLine:InvalidRangeFormat", attribute.Name],
                                        Value = attrValue
                                    });
                                }
                            }
                            else
                            {
                                rowErrors.Add(new ImportProTableRateLineErrorDto
                                {
                                    RowNumber = rowNumber,
                                    Field = attribute.Name,
                                    Message = L["Product:ProTableRateLine:InvalidRangeFormat", attribute.Name],
                                    Value = attrValue
                                });
                            }
                        }
                        else if (operatorType == "IN" || operatorType == "NOT_IN")
                        {
                            // Parse comma-separated values: "value1,value2,value3"
                            var parts = attrValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            var values = new List<object>();
                            bool allValid = true;

                            foreach (var part in parts)
                            {
                                var trimmed = part.Trim();
                                switch (attribute.DataType)
                                {
                                    case ProAttributeDataType.String:
                                        values.Add(trimmed);
                                        break;
                                    case ProAttributeDataType.Int:
                                        if (int.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                                            values.Add(intVal);
                                        else
                                            allValid = false;
                                        break;
                                    case ProAttributeDataType.Float:
                                        if (decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatVal))
                                            values.Add(floatVal);
                                        else
                                            allValid = false;
                                        break;
                                    default:
                                        allValid = false;
                                        break;
                                }
                            }

                            if (allValid && values.Count > 0)
                            {
                                parsedValue = values.ToArray();
                                isValid = true;
                            }
                            else
                            {
                                rowErrors.Add(new ImportProTableRateLineErrorDto
                                {
                                    RowNumber = rowNumber,
                                    Field = attribute.Name,
                                    Message = L["Product:ProTableRateLine:InvalidAttributeValue", attribute.Name, attrValue],
                                    Value = attrValue
                                });
                            }
                        }
                        else
                        {
                            // Handle other operators (EQUAL, GREATER_OR_EQUAL, LESS_OR_EQUAL, CONTAINS, NOT_CONTAINS) - existing logic
                            switch (attribute.DataType)
                            {
                                case ProAttributeDataType.String:
                                    parsedValue = attrValue;
                                    isValid = true;
                                    break;
                                case ProAttributeDataType.Int:
                                    if (int.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                                    {
                                        parsedValue = intVal;
                                        isValid = true;
                                    }
                                    break;
                                case ProAttributeDataType.Float:
                                    if (decimal.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatVal))
                                    {
                                        parsedValue = floatVal;
                                        isValid = true;
                                    }
                                    break;
                                case ProAttributeDataType.Boolean:
                                    if (bool.TryParse(attrValue, out var boolVal))
                                    {
                                        parsedValue = boolVal;
                                        isValid = true;
                                    }
                                    else if (attrValue.Equals("1", StringComparison.OrdinalIgnoreCase) || 
                                             attrValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                             attrValue.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                    {
                                        parsedValue = true;
                                        isValid = true;
                                    }
                                    else if (attrValue.Equals("0", StringComparison.OrdinalIgnoreCase) || 
                                             attrValue.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                                             attrValue.Equals("no", StringComparison.OrdinalIgnoreCase))
                                    {
                                        parsedValue = false;
                                        isValid = true;
                                    }
                                    break;
                            }
                        }

                        if (!isValid && !rowErrors.Any(e => e.RowNumber == rowNumber && e.Field == attribute.Name))
                        {
                            rowErrors.Add(new ImportProTableRateLineErrorDto
                            {
                                RowNumber = rowNumber,
                                Field = attribute.Name,
                                Message = L["Product:ProTableRateLine:InvalidAttributeValue", attribute.Name, attrValue],
                                Value = attrValue
                            });
                        }
                        else if (parsedValue != null && isValid)
                        {
                            // Use original case from attribute.Code, not the uppercased attrCode
                            conditionObject[attribute.Code] = parsedValue;
                        }
                    }
                }
            }

            // Check date overlap if coverage and dates are valid
            // Overlap is only rejected when CoverageId, ChannelId, PartnerId, AND Condition are all the same
            if (coverageId.HasValue && effectDate != default && expireDate.HasValue)
            {
                // Serialize condition object to JSON for comparison
                var conditionJson = JsonSerializer.Serialize(conditionObject);

                // Check against existing database records
                var hasOverlap = existingLines.Any(existing =>
                    existing.CoverageId == coverageId.Value &&
                    AreGuidsEqual(existing.ChannelId, channelId) &&
                    AreGuidsEqual(existing.PartnerId, partnerId) &&
                    AreConditionsEqual(existing.Condition, conditionJson) &&
                    existing.EffectDate <= expireDate.Value &&
                    (existing.ExpireDate == null || existing.ExpireDate >= effectDate));

                if (hasOverlap)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Ngày hiệu lực",
                        Message = L["Product:ProTableRateLine:DateOverlapWithExisting"],
                        Value = $"{effectDateText} - {expireDateText}"
                    });
                }

                // Check against other rows in import file (all rows processed so far)
                var overlapRow = importRows.FirstOrDefault(ir =>
                    ir.CoverageId.HasValue &&
                    ir.CoverageId.Value == coverageId.Value &&
                    AreGuidsEqual(ir.ChannelId, channelId) &&
                    AreGuidsEqual(ir.PartnerId, partnerId) &&
                    AreConditionsEqual(ir.Condition, conditionJson) &&
                    ir.EffectDate != default &&
                    ir.EffectDate <= expireDate.Value &&
                    (ir.ExpireDate == null || ir.ExpireDate >= effectDate));

                if (overlapRow.CoverageId.HasValue && overlapRow.RowNumber > 0)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Ngày hiệu lực",
                        Message = L["Product:ProTableRateLine:DateOverlapWithRow", overlapRow.RowNumber],
                        Value = $"{effectDateText} - {expireDateText}"
                    });
                }

                // Track this row for overlap checking (even if it has errors, for checking future rows)
                importRows.Add((coverageId, channelId, partnerId, conditionJson, effectDate, expireDate, rowNumber));
            }

            // If no errors, create the entity
            if (rowErrors.Count == 0 && coverageId.HasValue && effectDate != default && expireDate.HasValue)
            {
                try
                {
                    var conditionJson = JsonSerializer.Serialize(conditionObject);
                    var lineName = string.IsNullOrWhiteSpace(name) ? $"Phí {coverageName}" : name;

                    var entity = new ProTableRateLine(
                        GuidGenerator.Create(),
                        tableRateId,
                        lineName,
                        conditionJson,
                        effectDate,
                        coverageId.Value,
                        channelId,
                        partnerId,
                        minimumRate,
                        baseRate,
                        flatRate,
                        maxDiscount,
                        null, // LoadingRate
                        loading, // Loading (tăng/giảm phí)
                        expireDate
                    );

                    await Manager.CreateAsync(entity);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ImportProTableRateLineErrorDto
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

        // Generate result file
        result.ResultFileBytes = await GenerateResultFileAsync(worksheet, headers, attributeColumnMap, errors, rowNumber);

        await CurrentUnitOfWork.SaveChangesAsync();
        return result;
    }

    private async Task<byte[]> GenerateResultFileAsync(
        IXLWorksheet worksheet,
        Dictionary<string, int> headers,
        Dictionary<string, (int Col, ProTableRateVariable Variable)> attributeColumnMap,
        List<ImportProTableRateLineErrorDto> errors,
        int lastRowNumber)
    {
        using var resultWorkbook = new XLWorkbook();
        var resultWorksheet = resultWorkbook.Worksheets.Add("Kết quả");

        // Get all columns in original order (including dynamic attribute columns)
        var allColumns = headers.OrderBy(h => h.Value).ToList();
        int resultColIndex = 1;

        // Copy headers in original order
        foreach (var header in allColumns)
        {
            var sourceCell = worksheet.Cell(1, header.Value);
            var targetCell = resultWorksheet.Cell(1, resultColIndex);
            targetCell.Value = sourceCell.Value;
            
            // Preserve cell formatting if needed
            if (sourceCell.HasFormula)
            {
                targetCell.FormulaA1 = sourceCell.FormulaA1;
            }
            
            resultColIndex++;
        }
        
        // Add "Kết quả" column header
        resultWorksheet.Cell(1, resultColIndex).Value = "Kết quả";

        // Style header row
        var headerRow = resultWorksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Copy data rows and add result column
        var errorDict = errors.GroupBy(e => e.RowNumber).ToDictionary(g => g.Key, g => g.ToList());
        var successColor = XLColor.FromHtml("#90EE90");
        var errorColor = XLColor.FromHtml("#FFB6C1");

        for (int row = 2; row < lastRowNumber; row++)
        {
            // Skip empty rows
            if (worksheet.Row(row).IsEmpty())
            {
                continue;
            }

            resultColIndex = 1;
            
            // Copy all column values in original order
            foreach (var header in allColumns)
            {
                var sourceCell = worksheet.Cell(row, header.Value);
                var targetCell = resultWorksheet.Cell(row, resultColIndex);
                
                // Copy value based on data type
                if (sourceCell.DataType == XLDataType.DateTime)
                {
                    targetCell.Value = sourceCell.GetDateTime();
                    targetCell.Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else if (sourceCell.DataType == XLDataType.Number)
                {
                    targetCell.Value = sourceCell.GetDouble();
                }
                else if (sourceCell.DataType == XLDataType.Boolean)
                {
                    targetCell.Value = sourceCell.GetBoolean();
                }
                else
                {
                    targetCell.Value = sourceCell.GetString();
                }
                
                resultColIndex++;
            }

            // Add result column
            var resultCell = resultWorksheet.Cell(row, resultColIndex);
            if (errorDict.ContainsKey(row))
            {
                var errorMessages = errorDict[row].Select(e => e.Message).ToList();
                resultCell.Value = string.Join("; ", errorMessages);
                resultCell.Style.Fill.BackgroundColor = errorColor;
            }
            else
            {
                resultCell.Value = "Thành công";
                resultCell.Style.Fill.BackgroundColor = successColor;
            }
        }

        resultWorksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        resultWorkbook.SaveAs(stream);
        return stream.ToArray();
    }
}
