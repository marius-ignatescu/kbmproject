using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using KBMGrpcService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace KBMGrpcService.Tests.Services
{
    //public class OrganizationServiceTests
    //{
    //    private AppDbContext GetInMemoryDbContext()
    //    {
    //        var options = new DbContextOptionsBuilder<AppDbContext>()
    //            .UseInMemoryDatabase(Guid.NewGuid().ToString())
    //            .Options;
    //        return new AppDbContext(options);
    //    }

    //    [Fact]
    //    public async Task CreateOrganization_ReturnsIdAndSaves()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        var request = new CreateOrganizationRequest { Name = "New Org" };
    //        var grpc = TestServerCallContext.Create();

    //        var response = await service.CreateOrganization(request, grpc);

    //        Assert.True(response.Id > 0);
    //        var saved = await context.Organizations.FindAsync(response.Id);
    //        Assert.NotNull(saved);
    //        Assert.Equal("New Org", saved.Name);
    //    }

    //    [Fact]
    //    public async Task UpdateOrganization_ChangesName()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        context.Organizations.Add(new Organization { OrganizationId = 1, Name = "Old Org" });
    //        await context.SaveChangesAsync();

    //        var request = new UpdateOrganizationRequest { Id = 1, Name = "Updated Org" };
    //        var grpc = TestServerCallContext.Create();

    //        var response = await service.UpdateOrganization(request, grpc);

    //        Assert.Equal("Updated Org", response.Name);
    //        var updated = await context.Organizations.FindAsync(1);
    //        Assert.Equal("Updated Org", updated?.Name);
    //    }

    //    [Fact]
    //    public async Task DeleteOrganization_SetsDeletedAt()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        context.Organizations.Add(new Organization { OrganizationId = 1, Name = "To Delete" });
    //        await context.SaveChangesAsync();

    //        var request = new DeleteOrganizationRequest { Id = 1 };
    //        var grpc = TestServerCallContext.Create();

    //        var response = await service.DeleteOrganization(request, grpc);
    //        Assert.NotNull(response);

    //        var deleted = await context.Organizations.FindAsync(1);
    //        Assert.NotNull(deleted?.DeletedAt);
    //    }

    //    [Fact]
    //    public async Task GetOrganizationById_ReturnsResponse()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        context.Organizations.Add(new Organization { OrganizationId = 1, Name = "Exist Org" });
    //        await context.SaveChangesAsync();

    //        var request = new GetOrganizationByIdRequest { Id = 1 };
    //        var grpc = TestServerCallContext.Create();

    //        var response = await service.GetOrganizationById(request, grpc);

    //        Assert.Equal("Exist Org", response.Name);
    //    }

    //    [Fact]
    //    public async Task QueryOrganizations_ReturnsMatchingNames()
    //    {
    //        using var context = GetInMemoryDbContext();
    //        var logger = new Mock<ILogger<OrganizationService>>();
    //        var service = new OrganizationService(context, logger.Object);

    //        context.Organizations.AddRange(
    //            new Organization { OrganizationId = 1, Name = "AlphaUser" },
    //            new Organization { OrganizationId = 2, Name = "BetaUser" },
    //            new Organization { OrganizationId = 3, Name = "Gamma" }
    //        );
    //        await context.SaveChangesAsync();

    //        var request = new QueryOrganizationsRequest { QueryString = "user", Page = 1, PageSize = 10 };
    //        var grpc = TestServerCallContext.Create();

    //        var response = await service.QueryOrganizations(request, grpc);

    //        Assert.Equal(2, response.Organizations.Count); // Here should be Alpha and Beta
    //    }
    //}
}
