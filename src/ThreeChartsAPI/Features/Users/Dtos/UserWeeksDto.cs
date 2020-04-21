using System;
using System.Collections.Generic;

namespace ThreeChartsAPI.Features.Users.Dtos
{
    public class UserWeekDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }

        public int WeekNumber { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class UserWeeksDto
    {
        public List<UserWeekDto> Weeks { get; set; } = null!;
    }
}