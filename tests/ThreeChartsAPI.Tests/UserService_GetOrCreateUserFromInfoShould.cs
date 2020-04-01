using System;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Services;
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

            var unixTime = new DateTimeOffset(new DateTime(2020, 3, 3, 0, 0, 0)).ToUnixTimeMilliseconds();
            Console.WriteLine(DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime.ToLocalTime());

            var expectedUser = new User()
            {
                Id = 1,
                UserName = "edxds",
                RegisteredAt = new DateTime(2020, 3, 3),
            };

            var actualUser = await service.GetOrCreateUserFromInfo(new LastFmUserInfo()
            {
                Name = "edxds",
                RegisterDate = new DateTimeOffset(new DateTime(2020, 3, 3)).ToUnixTimeMilliseconds(),
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
                RegisteredAt = new DateTime(2020, 3, 3)
            };

            await _context.Users.AddAsync(expectedUser);
            await _context.SaveChangesAsync();

            var actualUser = await service.GetOrCreateUserFromInfo(new LastFmUserInfo()
            {
                Name = "edxds",
                RegisterDate = new DateTimeOffset(new DateTime(2020, 3, 3)).ToUnixTimeMilliseconds(),
            });

            actualUser.Should().BeEquivalentTo(expectedUser);
        }
    }
}