using System;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users;
using ThreeChartsAPI.Features.Users.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class UserService_GetOrCreateUserFromInfoShould
    {
        private ThreeChartsContext _context = ThreeChartsTestContext.BuildInMemoryContext();

        [Fact]
        public async Task GetOrCreateUserFromInfo_CreatesUser()
        {
            var service = new UserService(_context);

            var expectedUser = new User()
            {
                Id = 1,
                UserName = "edxds",
                RegisteredAt = new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc),
            };

            var actualUser = await service.GetOrCreateUserFromInfo(new LastFmUserInfo()
            {
                Name = "edxds",
                RegisterDate = 1583193600,
            });

            actualUser.Should().BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task GetOrCreateUserFromInfo_ReturnsExistingUser()
        {
            var service = new UserService(_context);

            var expectedUser = new User()
            {
                UserName = "edxds",
                RegisteredAt = new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc)
            };

            await _context.Users.AddAsync(expectedUser);
            await _context.SaveChangesAsync();

            var actualUser = await service.GetOrCreateUserFromInfo(new LastFmUserInfo()
            {
                Name = "edxds",
                RegisterDate = 1583193600,
            });

            actualUser.Should().BeEquivalentTo(expectedUser);
        }
    }
}