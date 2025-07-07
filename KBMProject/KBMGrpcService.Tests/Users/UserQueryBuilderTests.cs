using KBMGrpcService.Domain.Users.Queries;
using KBMGrpcService.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace KBMGrpcService.Tests.Users
{
    public class UserQueryBuilderTests
    {
        private List<User> GetTestUsers() => new()
        {
            new User { UserId = 1, Username = "alice", Name = "Alice User", Email = "alice@gmail.com" },
            new User { UserId = 2, Username = "bob", Name = "Bob User", Email = "bob@gmail.com" },
            new User { UserId = 3, Username = "charlie", Name = "Charlie User", Email = "charlie@gmail.com" }
        };

        [Fact]
        public void ApplyFiltering_ReturnsMatchingUsers()
        {
            var users = GetTestUsers().AsQueryable();

            var result = UserQueryBuilder.ApplyFiltering(users, "bob").ToList();

            Assert.Single(result);
            Assert.Equal("bob", result[0].Username);
        }

        [Fact]
        public void ApplyFiltering_WithEmptySearch_ReturnsAllUsers()
        {
            var users = GetTestUsers().AsQueryable();

            var result = UserQueryBuilder.ApplyFiltering(users, "").ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void ApplyPaging_ReturnsCorrectPage()
        {
            var users = GetTestUsers().AsQueryable();

            var result = UserQueryBuilder.ApplyPaging(users, 2, 2).ToList();

            Assert.Single(result);
            Assert.Equal("charlie", result[0].Username);
        }

        [Fact]
        public void ApplyOrdering_ValidColumnAscending_Works()
        {
            var users = GetTestUsers().AsQueryable();
            var logger = new Mock<ILogger>();

            var result = UserQueryBuilder.ApplyOrdering(users, "Username", "asc", logger.Object).ToList();

            Assert.Equal("alice", result[0].Username);
        }

        [Fact]
        public void ApplyOrdering_InvalidColumn_LogsWarningAndDefaults()
        {
            var users = GetTestUsers().AsQueryable();
            var logger = new Mock<ILogger>();

            var result = UserQueryBuilder.ApplyOrdering(users, "InvalidColumn", "asc", logger.Object).ToList();

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
