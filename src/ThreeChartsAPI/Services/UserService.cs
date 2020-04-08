using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ThreeChartsContext _context;

        public UserService(ThreeChartsContext context)
        {
            _context = context;
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