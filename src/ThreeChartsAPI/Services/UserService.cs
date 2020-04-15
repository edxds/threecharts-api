using System;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;
using TimeZoneConverter;

namespace ThreeChartsAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ThreeChartsContext _context;

        public UserService(ThreeChartsContext context)
        {
            _context = context;
        }

        public async Task<Result> UpdateUserPreferences(User user, UserPreferencesDto newPreferences)
        {
            TimeZoneInfo info;
            var validTimeZone = TZConvert.TryGetTimeZoneInfo(newPreferences.IanaTimezone, out info);
            if (!validTimeZone)
            {
                return Results.Fail("Invalid time zone");
            }
            
            user.IanaTimezone = newPreferences.IanaTimezone;
            await _context.SaveChangesAsync();

            return Results.Ok();
        }

        public async Task<User> GetOrCreateUserFromInfo(LastFmUserInfo userInfo)
        {
            var existingUser = await _context.Users
                .Where(user => user.UserName == userInfo.Name)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return existingUser;
            }

            var registeredAt = DateTimeOffset
                .FromUnixTimeSeconds(userInfo.RegisterDate)
                .DateTime;

            var newUser = new User()
            {
                UserName = userInfo.Name,
                RegisteredAt = registeredAt,
                RealName = userInfo.RealName,
                ProfilePicture = userInfo.Image,
                LastFmUrl = userInfo.Url,
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public Task<User> FindUserFromUserName(string userName)
        {
            return _context.Users.FirstOrDefaultAsync(user => user.UserName == userName);
        }
    }
}