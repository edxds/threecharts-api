using System;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users;
using ThreeChartsAPI.Features.Users.Dtos;
using ThreeChartsAPI.Features.Users.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetOrCreateUserFromInfo_WithValidInfo_CreatesUser()
        {
            // Arrange
            var service = new UserService(FakeThreeChartsContext.BuildInMemoryContext());
            
            // Act
            var actualUser = await service.GetOrCreateUserFromInfo(new LastFmUserInfo()
            {
                Name = "edxds",
                RegisterDate = 1583193600,
            });

            // Assert
            actualUser.Should().BeEquivalentTo(new User
            {
                Id = 1,
                UserName = "edxds",
                RegisteredAt = new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc),
            });
        }

        [Fact]
        public async Task GetOrCreateUserFromInfo_WithValidInfo_ReturnsExistingUser()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var service = new UserService(context);

            var expectedUser = new User
            {
                UserName = "edxds",
                RegisteredAt = new DateTime(2020, 3, 3, 0, 0, 0, DateTimeKind.Utc)
            };

            await context.Users.AddAsync(expectedUser);
            await context.SaveChangesAsync();

            // Act
            var actualUser = await service.GetOrCreateUserFromInfo(new LastFmUserInfo()
            {
                Name = "edxds",
                RegisterDate = 1583193600,
            });

            // Assert
            actualUser.Should().BeEquivalentTo(expectedUser);
        }
        
        [Fact]
        public async Task UpdateUserPreferences_WithValidTimeZone_UpdatesUserCorrectly()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var service = new UserService(context);
            var user = new User();
            
            await context.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            await service.UpdateUserPreferences(user, new UserPreferencesDto
            {
                IanaTimezone = "America/Sao_Paulo"
            });

            // Assert
            user.IanaTimezone.Should().BeEquivalentTo("America/Sao_Paulo");
        }

        [Fact]
        public async Task UpdateUserPreferences_WithInvalidTimeZone_ShouldFail()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var service = new UserService(context);
            var user = new User();

            await context.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await service.UpdateUserPreferences(user, new UserPreferencesDto
            {
                IanaTimezone = "Invalid/Time_Zone"
            });

            // Assert
            result.IsFailed.Should().BeTrue();
        }
    }
}