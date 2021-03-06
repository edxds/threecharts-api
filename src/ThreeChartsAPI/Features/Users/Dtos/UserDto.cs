using System;

namespace ThreeChartsAPI.Features.Users.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? RealName { get; set; }
        public string? LastFmUrl { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string? IanaTimezone { get; set; }
    }
}