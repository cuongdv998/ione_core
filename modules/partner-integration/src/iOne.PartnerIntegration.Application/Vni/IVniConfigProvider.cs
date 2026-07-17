using System.Threading;
using System.Threading.Tasks;

namespace iOne.PartnerIntegration.Vni;

public interface IVniConfigProvider
{
    Task<VniOptions> GetOptionsAsync(CancellationToken cancellationToken = default);
}
