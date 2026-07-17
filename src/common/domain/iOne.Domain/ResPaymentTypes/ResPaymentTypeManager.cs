using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResPaymentTypes;

public class ResPaymentTypeManager : DomainService
{
    protected IResPaymentTypeRepository Repository { get; }

    public ResPaymentTypeManager(IResPaymentTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResPaymentType paymentType)
    {
        if (await Repository.IsCodeExistsAsync(paymentType.Code))
        {
            throw new BusinessException("Master:ResPaymentType:CodeExists")
                .WithData("Code", paymentType.Code);
        }

        await Repository.InsertAsync(paymentType);
    }

    public virtual async Task UpdateAsync(ResPaymentType paymentType, string name, string? description, ResPaymentTypeStatus status)
    {
        paymentType.UpdateName(name);
        paymentType.UpdateDescription(description);
        paymentType.UpdateStatus(status);
        await Repository.UpdateAsync(paymentType);
    }
}
