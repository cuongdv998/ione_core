using System;
using System.Threading;
using System.Threading.Tasks;
using iOne.ResPartnerMessageLoggings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace iOne.PartnerIntegration.Logging;

public class PartnerMessageLoggingService : IPartnerMessageLoggingService, ITransientDependency
{
    private readonly IRepository<ResPartnerMessageLogging, Guid> _repository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PartnerMessageLoggingService(
        IRepository<ResPartnerMessageLogging, Guid> repository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _repository = repository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task LogAsync(ResPartnerMessageLogging entry, CancellationToken cancellationToken = default)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        await _repository.InsertAsync(entry, cancellationToken: cancellationToken);
        await uow.CompleteAsync(cancellationToken);
    }
}
