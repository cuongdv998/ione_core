using System.Threading;
using System.Threading.Tasks;
using iOne.ResPartnerMessageLoggings;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration;

public interface IPartnerMessageLoggingService : ITransientDependency
{
    Task LogAsync(ResPartnerMessageLogging entry, CancellationToken cancellationToken = default);
}
