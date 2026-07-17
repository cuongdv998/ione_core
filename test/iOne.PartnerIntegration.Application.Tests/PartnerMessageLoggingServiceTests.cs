using System;
using System.Threading;
using System.Threading.Tasks;
using iOne.PartnerIntegration.Logging;
using iOne.ResPartnerMessageLoggings;
using Moq;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace iOne.PartnerIntegration.Tests;

public class PartnerMessageLoggingServiceTests
{
    [Fact]
    public async Task LogAsync_Begins_New_UnitOfWork_And_Completes_After_Insert()
    {
        var entry = new ResPartnerMessageLogging(
            Guid.NewGuid(),
            "PTI",
            Guid.NewGuid(),
            "https://example.com/x",
            "POST",
            "{}",
            null,
            null,
            false,
            "error",
            42);

        var repo = new Mock<IRepository<ResPartnerMessageLogging, Guid>>();
        repo.Setup(r => r.InsertAsync(entry, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var innerUow = new Mock<IUnitOfWork>();
        innerUow.Setup(u => u.Dispose());
        innerUow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var uowManager = new Mock<IUnitOfWorkManager>();
        uowManager
            .Setup(m => m.Begin(It.IsAny<AbpUnitOfWorkOptions>(), true))
            .Returns(innerUow.Object);

        var sut = new PartnerMessageLoggingService(repo.Object, uowManager.Object);

        await sut.LogAsync(entry);

        uowManager.Verify(m => m.Begin(It.IsAny<AbpUnitOfWorkOptions>(), true), Times.Once);
        repo.Verify(r => r.InsertAsync(entry, false, It.IsAny<CancellationToken>()), Times.Once);
        innerUow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
