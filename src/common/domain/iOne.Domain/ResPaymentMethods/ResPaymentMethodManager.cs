using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResPaymentMethods;

public class ResPaymentMethodManager : DomainService
{
    protected IResPaymentMethodRepository Repository { get; }

    public ResPaymentMethodManager(IResPaymentMethodRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResPaymentMethod paymentMethod)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(paymentMethod.Code))
        {
            throw new BusinessException("Master:ResPaymentMethod:CodeExists")
                .WithData("Code", paymentMethod.Code);
        }

        await Repository.InsertAsync(paymentMethod);
    }

    public virtual async Task UpdateAsync(ResPaymentMethod paymentMethod, string name, ResPaymentMethodStatus status)
    {
        paymentMethod.UpdateName(name);
        paymentMethod.UpdateStatus(status);
        await Repository.UpdateAsync(paymentMethod);
    }
}
