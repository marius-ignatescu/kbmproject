using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Domain.Organizations.Validation;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Tests.Organizations
{
    public class OrganizationValidatorTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task ValidateCreateOrganizationRequest_Throws_WhenNameIsEmpty()
        {
            using var context = GetInMemoryDbContext();
            var request = new CreateOrganizationRequest { Name = "" };

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                OrganizationValidator.ValidateCreateOrganizationRequest(request, context));

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        }

        [Fact]
        public async Task ValidateCreateOrganizationRequest_Throws_WhenNameExists()
        {
            using var context = GetInMemoryDbContext();
            context.Organizations.Add(new Organization { Name = "ExistingOrg" });
            await context.SaveChangesAsync();

            var request = new CreateOrganizationRequest { Name = "ExistingOrg" };

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                OrganizationValidator.ValidateCreateOrganizationRequest(request, context));

            Assert.Equal(StatusCode.AlreadyExists, ex.StatusCode);
        }

        [Fact]
        public async Task ValidateOrganizationUpdateRequest_Throws_WhenNameExists()
        {
            using var context = GetInMemoryDbContext();

            var org1 = new Organization { OrganizationId = 1, Name = "ExistingOrg" };
            var org2 = new Organization { OrganizationId = 2, Name = "CurrentOrg" };

            context.Organizations.Add(org1);
            context.Organizations.Add(org2);
            await context.SaveChangesAsync();

            var request = new UpdateOrganizationRequest { Name = "ExistingOrg" };
            var existingOrg = await context.Organizations.FindAsync(2);

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                OrganizationValidator.ValidateOrganizationUpdateRequest(request, existingOrg!, context));

            Assert.Equal(StatusCode.AlreadyExists, ex.StatusCode);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        public void ValidatePagination_Throws_WhenInvalid(int page, int pageSize)
        {
            var request = new Protos.QueryOrganizationsRequest { Page = page, PageSize = pageSize };

            var ex = Assert.Throws<RpcException>(() =>
                OrganizationValidator.ValidatePagination(request));

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        }
    }
}
