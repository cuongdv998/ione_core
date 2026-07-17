using System;
using System.Threading;
using System.Threading.Tasks;
using iOne.PartnerIntegration.Pti.Models;

namespace iOne.PartnerIntegration.Pti;

public interface IPtiInsurerCodebookResolver
{
    /// <summary>
    /// Resolves a single active mapping row for PTI JSON codebooks.
    /// When no row matches, returns <see cref="PtiCodebook"/> with both <c>code</c> and <c>name</c>
    /// set to the iOne <paramref name="ownCode"/> and logs a warning.
    /// </summary>
    Task<PtiCodebook> ResolveRequiredAsync(
        Guid insurerId,
        string businessName,
        string ownCode,
        CancellationToken cancellationToken = default);
}
