using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using iOne.InsurerDictionaries;
using iOne.PartnerIntegration.Pti;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Xunit;

namespace iOne.PartnerIntegration.Tests;

public class PtiInsurerCodebookResolverTests
{
    private static readonly Guid InsurerId = Guid.Parse("3a1ef3f1-961e-5f8a-c1c2-70f918be0e04");

    [Fact]
    public async Task ResolveRequiredAsync_Should_Map_InsurerCode_And_Plain_ExtraData_To_Codebook()
    {
        var row = new InsurerDictionary(
            Guid.NewGuid(),
            PtiInsurerDictionaryCategories.MotorVehicleStatus,
            InsurerId,
            "OLD",
            "OLD",
            "Xe đã qua sử dụng",
            InsurerDictionaryStatus.Active,
            DateTime.UtcNow.Date.AddYears(-1),
            null);

        var repo = new Mock<IRepository<InsurerDictionary, Guid>>();
        repo.Setup(r => r.GetListAsync(It.IsAny<Expression<Func<InsurerDictionary, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InsurerDictionary> { row });

        var clock = new Mock<IClock>();
        clock.Setup(c => c.Now).Returns(DateTime.UtcNow);

        var sut = new PtiInsurerCodebookResolver(repo.Object, clock.Object, NullLogger<PtiInsurerCodebookResolver>.Instance);

        var cb = await sut.ResolveRequiredAsync(InsurerId, PtiInsurerDictionaryCategories.MotorVehicleStatus, "OLD");

        Assert.Equal("OLD", cb.Code);
        Assert.Equal("Xe đã qua sử dụng", cb.Name);
    }

    [Fact]
    public async Task ResolveRequiredAsync_Should_Parse_Name_From_Json_ExtraData()
    {
        var row = new InsurerDictionary(
            Guid.NewGuid(),
            PtiInsurerDictionaryCategories.MotorVehicleStatus,
            InsurerId,
            "NEW",
            "NEW",
            "{\"name\":\"Xe chưa lưu hành\"}",
            InsurerDictionaryStatus.Active,
            DateTime.UtcNow.Date.AddYears(-1),
            null);

        var repo = new Mock<IRepository<InsurerDictionary, Guid>>();
        repo.Setup(r => r.GetListAsync(It.IsAny<Expression<Func<InsurerDictionary, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InsurerDictionary> { row });

        var clock = new Mock<IClock>();
        clock.Setup(c => c.Now).Returns(DateTime.UtcNow);

        var sut = new PtiInsurerCodebookResolver(repo.Object, clock.Object, NullLogger<PtiInsurerCodebookResolver>.Instance);

        var cb = await sut.ResolveRequiredAsync(InsurerId, PtiInsurerDictionaryCategories.MotorVehicleStatus, "NEW");

        Assert.Equal("NEW", cb.Code);
        Assert.Equal("Xe chưa lưu hành", cb.Name);
    }

    [Fact]
    public async Task ResolveRequiredAsync_Should_Use_OwnCode_For_Code_And_Name_When_No_Active_Row()
    {
        var repo = new Mock<IRepository<InsurerDictionary, Guid>>();
        repo.Setup(r => r.GetListAsync(It.IsAny<Expression<Func<InsurerDictionary, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InsurerDictionary>());

        var clock = new Mock<IClock>();
        clock.Setup(c => c.Now).Returns(DateTime.UtcNow);

        var sut = new PtiInsurerCodebookResolver(repo.Object, clock.Object, NullLogger<PtiInsurerCodebookResolver>.Instance);

        var cb = await sut.ResolveRequiredAsync(InsurerId, PtiInsurerDictionaryCategories.MotorVehicleStatus, "MISSING");

        Assert.Equal("MISSING", cb.Code);
        Assert.Equal("MISSING", cb.Name);
    }
}
