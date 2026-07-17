using System.Threading;
using System.Threading.Tasks;

namespace iOne.PartnerIntegration.Pti;

public interface IPtiConfigProvider
{
    Task<PtiOptions> GetOptionsAsync(CancellationToken cancellationToken = default);
}
