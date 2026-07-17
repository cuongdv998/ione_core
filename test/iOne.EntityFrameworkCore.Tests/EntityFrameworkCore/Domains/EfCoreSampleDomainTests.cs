using iOne.Samples;
using Xunit;

namespace iOne.EntityFrameworkCore.Domains;

[Collection(iOneTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<iOneEntityFrameworkCoreTestModule>
{

}
