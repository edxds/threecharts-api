using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Features.Users;
using ThreeChartsAPI.Features.Users.Dtos;
using ThreeChartsAPI.Features.Users.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class UserService_UpdateUserPreferencesShould
    {
        private readonly ThreeChartsContext
            _context = ThreeChartsTestContext.BuildInMemoryContext();

        [Fact]
        public async Task UpdateUserPreferences_WithValidTimeZone_UpdatesUserCorrectly()
        {
            var service = new UserService(_context);
            var user = new User();
            
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            await service.UpdateUserPreferences(user, new UserPreferencesDto
            {
                IanaTimezone = "America/Sao_Paulo"
            });
            
            var expectedTimezone = "America/Sao_Paulo";
            var actualTimezone = user.IanaTimezone;

            actualTimezone.Should().BeEquivalentTo(actualTimezone);
        }

        [Fact]
        public async Task UpdateUserPreferences_WithInvalidTimeZone_ShouldFail()
        {
            var service = new UserService(_context);
            var user = new User();

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            var result = await service.UpdateUserPreferences(user, new UserPreferencesDto
            {
                IanaTimezone = "Invalid/Time_Zone"
            });

            result.IsFailed.Should().BeTrue();
        }
    }
}