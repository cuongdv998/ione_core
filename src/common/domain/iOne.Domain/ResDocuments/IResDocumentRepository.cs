using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResDocuments;

public interface IResDocumentRepository : IRepository<ResDocument, Guid>
{
    // Add custom methods if needed
}

