using KBMGrpcService.Domain.Organizations.Queries;
using KBMGrpcService.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace KBMGrpcService.Tests.Organizations
{
    public class OrganizationQueryBuilderTests
    {
        private List<Organization> GetTestOrganizations() => new()
        {
            new Organization { OrganizationId = 1, Name = "Alpha Corp", Address = "123 Some St" },
            new Organization { OrganizationId = 2, Name = "Beta Ltd", Address = "456 Some Blvd" },
            new Organization { OrganizationId = 3, Name = "Gamma Inc", Address = null }
        };

        [Fact]
        public void ApplyFiltering_ReturnsMatchingOrganizations()
        {
            var orgs = GetTestOrganizations().AsQueryable();

            var result = OrganizationQueryBuilder.ApplyFiltering(orgs, "beta").ToList();

            Assert.Single(result);
            Assert.Equal("Beta Ltd", result[0].Name);
        }

        [Fact]
        public void ApplyFiltering_WithEmptySearch_ReturnsAll()
        {
            var orgs = GetTestOrganizations().AsQueryable();

            var result = OrganizationQueryBuilder.ApplyFiltering(orgs, "").ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void ApplyPaging_ReturnsCorrectPage()
        {
            var orgs = GetTestOrganizations().AsQueryable();

            var result = OrganizationQueryBuilder.ApplyPaging(orgs, 2, 2).ToList();

            Assert.Single(result);
            Assert.Equal("Gamma Inc", result[0].Name);
        }

        [Fact]
        public void ApplyOrdering_ValidColumnAscending_Works()
        {
            var orgs = GetTestOrganizations().AsQueryable();
            var logger = new Mock<ILogger>();

            var result = OrganizationQueryBuilder.ApplyOrdering(orgs, "Name", "asc", logger.Object).ToList();

            Assert.Equal("Alpha Corp", result[0].Name);
        }

        [Fact]
        public void ApplyOrdering_InvalidColumn_LogsWarningAndDefaults()
        {
            var orgs = GetTestOrganizations().AsQueryable();
            var logger = new Mock<ILogger>();

            var result = OrganizationQueryBuilder.ApplyOrdering(orgs, "InvalidColumn", "asc", logger.Object).ToList();

            logger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Invalid OrderBy column")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(3, result.Count);
        }
    }
}
