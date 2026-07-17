using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResSequences;
using iOne.ResSequences; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResSequences;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResSequencePermissions.Default)]
public class ResSequenceAppService : CrudAppService<
    ResSequence,
    ResSequenceDto,
    Guid,
    GetResSequencesInput,
    CreateResSequenceDto,
    UpdateResSequenceDto>, IResSequenceAppService
{
    protected ResSequenceManager Manager { get; }

    public ResSequenceAppService(
        IResSequenceRepository repository,
        ResSequenceManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResSequencePermissions.View;
        GetListPolicyName = ResSequencePermissions.View;
        CreatePolicyName = ResSequencePermissions.Create;
        UpdatePolicyName = ResSequencePermissions.Edit;
        DeletePolicyName = ResSequencePermissions.Delete;
    }

    public override async Task<ResSequenceDto> CreateAsync(CreateResSequenceDto input)
    {
        var entity = new ResSequence(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Prefix,
            input.Suffix,
            input.Type,
            input.Padding,
            input.NumberNext,
            input.NumberIncrement,
            input.UseDateRange,
            input.DateRangeType,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResSequence, ResSequenceDto>(entity);
    }

    public override async Task<ResSequenceDto> UpdateAsync(Guid id, UpdateResSequenceDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: KHÔNG có Code - Code không được phép sửa
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Prefix,
            input.Suffix,
            input.Type,
            input.Padding,
            input.NumberNext,
            input.NumberIncrement,
            input.UseDateRange,
            input.DateRangeType,
            input.Status
        );

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResSequence, ResSequenceDto>(entity!);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // ⚠️ QUAN TRỌNG: Soft delete - xóa trước (ABP audit log) → cập nhật status về Deactive sau
        // Manager.DeleteAsync() sẽ xử lý: 1. Delete để trigger ABP audit log, 2. Update status về Deactive
        await Manager.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResSequence>> CreateFilteredQueryAsync(GetResSequencesInput input)
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

        // Filter by Type
        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }

    public virtual async Task<GetNextSequenceOutput> GetNextSequenceAsync(GetNextSequenceInput input)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(input.Code) && !input.Id.HasValue)
        {
            throw new UserFriendlyException(L["Master:ResSequence:CodeOrIdRequired"]);
        }

        // Load ResSequence entity
        ResSequence? sequence = null;
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var query = await Repository.GetQueryableAsync();
            sequence = await query.FirstOrDefaultAsync(x => x.Code == input.Code);
        }
        else if (input.Id.HasValue)
        {
            sequence = await Repository.GetAsync(input.Id.Value);
        }

        if (sequence == null)
        {
            throw new UserFriendlyException(L["Master:ResSequence:NotFound"]);
        }

        // Check if sequence is active
        if (sequence.Status != ResSequenceStatus.Active)
        {
            throw new UserFriendlyException(L["Master:ResSequence:NotActive"]);
        }

        // Reload entity to get latest NumberNext value (important for concurrent requests)
        // This ensures we always work with the most current database state
        var freshSequence = await Repository.GetAsync(sequence.Id);

        // Check if sequence is still active
        if (freshSequence.Status != ResSequenceStatus.Active)
        {
            throw new UserFriendlyException(L["Master:ResSequence:NotActive"]);
        }

        // Check if need to reset based on UseDateRange
        var currentNumber = freshSequence.NumberNext;
        var shouldReset = false;

        if (freshSequence.UseDateRange == ResSequenceUseDateRange.Yes && freshSequence.DateRangeType.HasValue)
        {
            shouldReset = ShouldResetSequence(freshSequence);
            if (shouldReset)
            {
                currentNumber = 0; // Reset to 0, will be incremented below
            }
        }

        // Generate the code
        var generatedCode = GenerateSequenceCode(freshSequence, currentNumber, input.Parameters);

        // Calculate next number
        var nextNumber = shouldReset 
            ? freshSequence.NumberIncrement 
            : currentNumber + freshSequence.NumberIncrement;

        // Update NumberNext
        freshSequence.UpdateNumberNext(nextNumber);

        await Repository.UpdateAsync(freshSequence);

        await CurrentUnitOfWork.SaveChangesAsync();

        return new GetNextSequenceOutput
        {
            GeneratedCode = generatedCode,
            NextNumber = nextNumber
        };
    }

    /// <summary>
    /// Check if sequence should be reset based on date range
    /// </summary>
    private bool ShouldResetSequence(ResSequence sequence)
    {
        if (sequence.UseDateRange != ResSequenceUseDateRange.Yes || !sequence.DateRangeType.HasValue)
        {
            return false;
        }

        var now = Clock.Now;
        var lastModified = sequence.LastModificationTime ?? sequence.CreationTime;

        switch (sequence.DateRangeType.Value)
        {
            case ResSequenceDateRangeType.Week:
                // Check if we're in a different week
                var currentWeek = GetWeekOfYear(now);
                var lastWeek = GetWeekOfYear(lastModified);
                return currentWeek != lastWeek || now.Year != lastModified.Year;

            case ResSequenceDateRangeType.Month:
                return now.Year != lastModified.Year || now.Month != lastModified.Month;

            case ResSequenceDateRangeType.Quarter:
                var currentQuarter = (now.Month - 1) / 3 + 1;
                var lastQuarter = (lastModified.Month - 1) / 3 + 1;
                return now.Year != lastModified.Year || currentQuarter != lastQuarter;

            case ResSequenceDateRangeType.Half:
                var currentHalf = now.Month <= 6 ? 1 : 2;
                var lastHalf = lastModified.Month <= 6 ? 1 : 2;
                return now.Year != lastModified.Year || currentHalf != lastHalf;

            case ResSequenceDateRangeType.Year:
                return now.Year != lastModified.Year;

            default:
                return false;
        }
    }

    /// <summary>
    /// Get week number of year
    /// </summary>
    private int GetWeekOfYear(DateTime date)
    {
        var calendar = CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    /// <summary>
    /// Generate sequence code based on configuration
    /// Format: Prefix + Number (with padding) + Suffix
    /// </summary>
    private string GenerateSequenceCode(ResSequence sequence, long number, Dictionary<string, string>? parameters = null)
    {
        var sb = new StringBuilder();

        // Process Prefix
        if (!string.IsNullOrWhiteSpace(sequence.Prefix))
        {
            var prefix = ProcessTemplate(sequence.Prefix, parameters);
            sb.Append(prefix);
        }

        // Generate number with padding
        var numberStr = number.ToString();
        if (sequence.Padding.HasValue && sequence.Padding.Value > 0)
        {
            numberStr = numberStr.PadLeft(sequence.Padding.Value, '0');
        }
        sb.Append(numberStr);

        // Process Suffix
        if (!string.IsNullOrWhiteSpace(sequence.Suffix))
        {
            var suffix = ProcessTemplate(sequence.Suffix, parameters);
            sb.Append(suffix);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Process template string (e.g., $date(format), ${date}(format), ${param_name}, etc.)
    /// Supports:
    /// - $date(format) and ${date}(format): date formatting
    /// - ${param_name}: dynamic parameters passed from input
    /// </summary>
    private string ProcessTemplate(string template, Dictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        var result = template;
        var now = Clock.Now;

        // Process $date(format) - supports common date formats
        // Pattern: $date(yyyyMMdd), $date(yyyy-MM-dd), $date(dd/MM/yyyy), etc.
        var datePattern1 = @"\$date\(([^)]+)\)";
        result = Regex.Replace(result, datePattern1, match =>
        {
            var format = match.Groups[1].Value;
            try
            {
                return now.ToString(format);
            }
            catch
            {
                // If format is invalid, return the original pattern
                return match.Value;
            }
        });

        // Process ${date}(format) - alternative syntax
        // Pattern: ${date}(ddMMyyyy), ${date}(yyyyMMdd), etc.
        var datePattern2 = @"\$\{date\}\(([^)]+)\)";
        result = Regex.Replace(result, datePattern2, match =>
        {
            var format = match.Groups[1].Value;
            try
            {
                return now.ToString(format);
            }
            catch
            {
                // If format is invalid, return the original pattern
                return match.Value;
            }
        });

        // Process ${param_name} - dynamic parameters
        // Pattern: ${customerCode}, ${orderId}, ${productCode}, etc.
        if (parameters != null && parameters.Count > 0)
        {
            var paramPattern = @"\$\{([^}]+)\}";
            result = Regex.Replace(result, paramPattern, match =>
            {
                var paramName = match.Groups[1].Value;
                
                // Skip if it's already processed as ${date}(format)
                if (paramName == "date")
                {
                    return match.Value;
                }

                // Try to get parameter value
                if (parameters.TryGetValue(paramName, out var paramValue))
                {
                    return paramValue ?? string.Empty;
                }

                // If parameter not found, return the original pattern
                return match.Value;
            });
        }

        return result;
    }
}

