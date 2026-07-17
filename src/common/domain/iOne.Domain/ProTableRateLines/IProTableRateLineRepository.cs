using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ProTableRateLines;

public interface IProTableRateLineRepository : IRepository<ProTableRateLine, Guid>
{
    /// <summary>
    /// Finds a matching rate line based on the given conditions.
    /// Conditions are matched against the jsonb Condition field using the specified operators.
    /// When coverageCodes is provided, rate lines with condition->'coverages' overlapping the list will match.
    /// When multiple rate lines match, the one with the most condition attributes (top-level keys in Condition JSON) is chosen.
    /// </summary>
    /// <param name="tableRateId">The table rate ID to filter by.</param>
    /// <param name="coverageId">The coverage ID to filter by (optional).</param>
    /// <param name="conditions">Dictionary of attribute code to (Operator, Value) pairs.</param>
    /// <param name="coverageCodes">Optional list of coverage codes; when provided, rate line condition->'coverages' must overlap (any common element).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The best matching ProTableRateLine (most condition attributes) or null if no match found.</returns>
    Task<ProTableRateLine?> FindMatchingRateLineAsync(
        Guid tableRateId,
        Guid? coverageId,
        Dictionary<string, (string Operator, object Value)> conditions,
        CancellationToken cancellationToken = default);
}
