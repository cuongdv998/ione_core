using Xunit;

namespace iOne.EntityFrameworkCore;

[CollectionDefinition(iOneTestConsts.CollectionDefinitionName)]
public class iOneEntityFrameworkCoreCollection : ICollectionFixture<iOneEntityFrameworkCoreFixture>
{

}
