using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.AdminConfigs;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace iOne.PartnerIntegration.Pti;

public class PtiConfigProvider : IPtiConfigProvider, ITransientDependency
{
    private const string PtiConfigCode = "PTI_CONFIG";

    private readonly IRepository<AdminConfig, Guid> _repository;

    public PtiConfigProvider(IRepository<AdminConfig, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PtiOptions> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var query = await _repository.GetQueryableAsync();
        var rows = await query
            .Where(x => x.Code == PtiConfigCode && x.Status == AdminConfigStatus.Active)
            .ToListAsync(cancellationToken);

        var options = new PtiOptions();
        foreach (var row in rows)
        {
            switch (row.SubCode.ToUpperInvariant())
            {
                case "BASEURL":
                    options.BaseUrl = row.Value ?? string.Empty;
                    break;
                case "AUTHBASEURL":
                    options.AuthBaseUrl = row.Value ?? string.Empty;
                    break;
                case "USERNAME":
                    options.UserName = row.Value ?? string.Empty;
                    break;
                case "PASSWORD":
                    options.Password = row.Value ?? string.Empty;
                    break;
                case "PARTNERNAME":
                    options.PartnerName = row.Value ?? string.Empty;
                    break;
                case "CHANNELCODE":
                    options.ChannelCode = row.Value ?? "KENH004";
                    break;
                case "PRIVATEKEYPATH":
                    options.PrivateKeyPath = row.Value ?? string.Empty;
                    break;
                case "SIGNATUREMODE":
                    options.SignatureMode = row.Value ?? "EncryptBody";
                    break;
                case "ENABLESIGNING":
                    options.EnableSigning = !string.Equals(row.Value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
                    break;
                case "PGPENCRYPTBODYRESPONSEPASSPHRASE":
                    options.PgpEncryptBodyResponsePassphrase = row.Value;
                    break;
            }
        }

        return options;
    }
}
