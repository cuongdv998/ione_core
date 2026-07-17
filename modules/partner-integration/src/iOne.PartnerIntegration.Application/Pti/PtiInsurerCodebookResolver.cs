using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iOne.InsurerDictionaries;
using iOne.PartnerIntegration.Pti.Models;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace iOne.PartnerIntegration.Pti;

public class PtiInsurerCodebookResolver : IPtiInsurerCodebookResolver, ITransientDependency
{
    private readonly IRepository<InsurerDictionary, Guid> _repository;
    private readonly IClock _clock;
    private readonly ILogger<PtiInsurerCodebookResolver> _logger;

    public PtiInsurerCodebookResolver(
        IRepository<InsurerDictionary, Guid> repository,
        IClock clock,
        ILogger<PtiInsurerCodebookResolver> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PtiCodebook> ResolveRequiredAsync(
        Guid insurerId,
        string businessName,
        string ownCode,
        CancellationToken cancellationToken = default)
    {
        if (insurerId == Guid.Empty)
            throw new BusinessException("Policy:Partner:PtiInsurerIdMissing");

        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));

        var trimmedOwn = ownCode?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedOwn))
            throw new ArgumentException("Own code is required.", nameof(ownCode));

        var asOfDate = _clock.Now.Date;

        var candidates = await _repository.GetListAsync(
            x =>
                x.InsurerId == insurerId &&
                x.BusinessName == businessName &&
                x.OwnCode == trimmedOwn &&
                x.Status == InsurerDictionaryStatus.Active &&
                x.EffectDate <= asOfDate &&
                (x.ExpireDate == null || x.ExpireDate.Value.Date >= asOfDate),
            cancellationToken: cancellationToken);

        var row = candidates
            .OrderByDescending(x => x.EffectDate)
            .FirstOrDefault();

        if (row == null)
        {
            _logger.LogWarning(
                "PTI insurer_dictionary: no active mapping for InsurerId={InsurerId}, BusinessName={BusinessName}, OwnCode={OwnCode}. Sending codebook with code and name equal to own code.",
                insurerId,
                businessName,
                trimmedOwn);

            return new PtiCodebook
            {
                Code = trimmedOwn,
                Name = trimmedOwn
            };
        }

        var displayName = ResolveDisplayName(row.ExtraData, row.InsurerCode);

        return new PtiCodebook
        {
            Code = row.InsurerCode,
            Name = displayName
        };
    }

    private static string ResolveDisplayName(string? extraData, string insurerCode)
    {
        if (string.IsNullOrWhiteSpace(extraData))
            return insurerCode;

        var trimmed = extraData.Trim();
        var fromJson = TryGetNameFromJson(trimmed);
        if (!string.IsNullOrWhiteSpace(fromJson))
            return fromJson!;

        return trimmed;
    }

    private static string? TryGetNameFromJson(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (doc.RootElement.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                return n.GetString();

            if (doc.RootElement.TryGetProperty("displayName", out var d) && d.ValueKind == JsonValueKind.String)
                return d.GetString();
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
