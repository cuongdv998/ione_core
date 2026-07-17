using iOne.Samples;
using Xunit;

namespace iOne.EntityFrameworkCore.Applications;

[Collection(iOneTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<iOneEntityFrameworkCoreTestModule>
{

}
