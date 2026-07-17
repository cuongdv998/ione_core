using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.AdminConfigs;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace iOne.PartnerIntegration.Vni;

public class VniConfigProvider : IVniConfigProvider, ITransientDependency
{
    private const string VniConfigCode = "VNI_CONFIG";

    private readonly IRepository<AdminConfig, System.Guid> _repository;

    public VniConfigProvider(IRepository<AdminConfig, System.Guid> repository)
    {
        _repository = repository;
    }

    public async Task<VniOptions> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var query = await _repository.GetQueryableAsync();
        var rows = await query
            .Where(x => x.Code == VniConfigCode && x.Status == AdminConfigStatus.Active)
            .ToListAsync(cancellationToken);

        var options = new VniOptions();
        foreach (var row in rows)
        {
            switch (row.SubCode.ToUpperInvariant())
            {
                case "BASEURL":
                    options.BaseUrl = row.Value ?? string.Empty;
                    break;
                case "USERNAME":
                    options.UserName = row.Value ?? string.Empty;
                    break;
                case "PASSWORD":
                    options.PassWord = row.Value ?? string.Empty;
                    break;
                case "DVISL":
                    options.DviSl = row.Value ?? string.Empty;
                    break;
                case "MACN":
                    options.MaCn = row.Value ?? string.Empty;
                    break;
            }
        }

        return options;
    }
}
