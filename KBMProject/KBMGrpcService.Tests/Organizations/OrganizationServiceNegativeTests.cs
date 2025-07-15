using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Protos;
using KBMGrpcService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace KBMGrpcService.Tests.Services
{
    //public class OrganizationServiceNegativeTests
    //{
    //    private AppDbContext GetInMemoryDbContext()
    //    {
    //        var options = new DbContextOptionsBuilder<AppDbContext>()
    //            .UseInMemoryDatabase(Guid.NewGuid().ToString())
    //            .Options;
    //        return new AppDbContext(options);
    //    }

    //    [Fact]
    //    public async Task GetOrganizationById_ThrowsRpcException_WhenNotFound()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        var request = new GetOrganizationByIdRequest { Id = 99 };
    //        var grpc = TestServerCallContext.Create();

    //        var ex = await Assert.ThrowsAsync<RpcException>(() => service.GetOrganizationById(request, grpc));
    //        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    //    }

    //    [Fact]
    //    public async Task UpdateOrganization_ThrowsRpcException_WhenNotFound()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        var request = new UpdateOrganizationRequest { Id = 999, Name = "New Name" };
    //        var grpc = TestServerCallContext.Create();

    //        var ex = await Assert.ThrowsAsync<RpcException>(() => service.UpdateOrganization(request, grpc));
    //        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    //    }

    //    [Fact]
    //    public async Task DeleteOrganization_ReturnFalse_WhenNotFound()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        var request = new DeleteOrganizationRequest { Id = 5000 };
    //        var grpc = TestServerCallContext.Create();

    //        var response = await service.DeleteOrganization(request, grpc);
    //        Assert.False(response.Success);
    //    }
    //}
}
