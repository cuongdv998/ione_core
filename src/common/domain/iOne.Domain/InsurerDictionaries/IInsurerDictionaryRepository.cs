using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.InsurerDictionaries;

public interface IInsurerDictionaryRepository : IRepository<InsurerDictionary, Guid>
{
}
