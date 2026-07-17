using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.AccountPaymentRequests;

public interface IAccountPaymentRequestRepository : IRepository<AccountPaymentRequest, Guid>
{
}
