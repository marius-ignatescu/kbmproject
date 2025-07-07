using KBMGrpcService.Domain.Organizations.Mapping;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Tests.Organizations
{
    public class OrganizationMapperTests
    {
        [Fact]
        public void MapToOrganizationResponse_MapsCorrectly()
        {
            var entity = new Organization
            {
                OrganizationId = 1,
                Name = "Test Org",
                Address = "123 Main St",
                CreatedAt = new DateTime(2023, 1, 1),
                UpdatedAt = new DateTime(2023, 6, 1)
            };

            var response = OrganizationMapper.MapToOrganizationResponse(entity);

            Assert.Equal("Test Org", response.Name);
            Assert.Equal("123 Main St", response.Address);
            Assert.Equal("1", response.Id.ToString());
            Assert.Equal("1/1/2023 12:00:00 AM", response.CreatedAt);
            Assert.Equal("6/1/2023 12:00:00 AM", response.UpdatedAt);
        }

        [Fact]
        public void MapToOrganizationResponse_HandlesNulls()
        {
            var entity = new Organization
            {
                OrganizationId = 2,
                Name = "Null Org",
                Address = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var response = OrganizationMapper.MapToOrganizationResponse(entity);

            Assert.Equal(string.Empty, response.Address);
            Assert.Equal(string.Empty, response.UpdatedAt);
        }

        [Fact]
        public void CreateOrganizationEntity_MapsCorrectly()
        {
            var request = new CreateOrganizationRequest
            {
                Name = "New Org",
                Address = "456 Side St"
            };

            var entity = OrganizationMapper.CreateOrganizationEntity(request);

            Assert.Equal("New Org", entity.Name);
            Assert.Equal("456 Side St", entity.Address);
            Assert.True((DateTime.UtcNow - entity.CreatedAt).TotalSeconds < 5);
        }
    }
}
