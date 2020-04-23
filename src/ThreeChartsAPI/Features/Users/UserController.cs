using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Features.Charts;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Dtos;
using ThreeChartsAPI.Features.Users.Dtos;
using TimeZoneConverter;

namespace ThreeChartsAPI.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ChartRepository _chartRepository;
        private readonly IChartService _chartService;
        private readonly IChartDateService _chartDateService;
        private readonly ILastFmService _lastFm;

        public UserController(
            IUserService userService,
            ChartRepository chartRepository,
            IChartService chartService,
            IChartDateService chartDateService,
            ILastFmService lastFmService)
        {
            _userService = userService;
            _chartRepository = chartRepository;
            _chartService = chartService;
            _chartDateService = chartDateService;
            _lastFm = lastFmService;
        }

        [HttpPut("preferences")]
        public async Task<ActionResult<UserDto>> UpdatePreferences(
            [FromBody] UserPreferencesDto preferencesDto)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userName == null)
            {
                return Unauthorized();
            }

            var user = await _userService.FindUserFromUserName(userName);
            if (user == null)
            {
                return NotFound();
            }

            var updateResult = await _userService.UpdateUserPreferences(user, preferencesDto);
            if (updateResult.IsFailed)
            {
                return BadRequest();
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                LastFmUrl = user.LastFmUrl,
                ProfilePicture = user.ProfilePicture,
                RegisteredAt = user.RegisteredAt,
                IanaTimezone = user.IanaTimezone,
            });
        }

        [HttpGet]
        [Route("details")]
        public async Task<ActionResult<UserDto>> GetDetails()
        {
            var sessionKey = User.FindFirst("SessionKey").Value;
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (sessionKey == null || userName == null)
            {
                return Unauthorized();
            }

            var user = await _userService.FindUserFromUserName(userName);
            if (user == null)
            {
                var userInfo = await _lastFm.GetUserInfo(null, sessionKey);
                if (userInfo.IsFailed)
                {
                    var lastFmError = userInfo.Errors.Find(error =>
                        error is LastFmResultError
                    ) as LastFmResultError;

                    return StatusCode(lastFmError!.StatusCode, new LastFmErrorDto()
                    {
                        ErrorCode = lastFmError!.LastFmErrorCode ?? -1,
                        Message = lastFmError!.LastFmErrorMessage ?? "Last.fm service unavailable."
                    });
                }

                user = await _userService.GetOrCreateUserFromInfo(userInfo.Value);
            }

            return Ok(new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                LastFmUrl = user.LastFmUrl,
                ProfilePicture = user.ProfilePicture,
                RegisteredAt = user.RegisteredAt,
                IanaTimezone = user.IanaTimezone,
            });
        }

        [HttpPost]
        [Route("sync")]
        public async Task<ActionResult<UserWeeksDto>> SyncWeeks()
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userName == null)
            {
                return Unauthorized();
            }

            var user = await _userService.FindUserFromUserName(userName);
            if (user == null)
            {
                return NotFound();
            }

            if (user.IanaTimezone == null)
            {
                return BadRequest();
            }
            
            var syncStartDate = user.RegisteredAt;
            var startWeekNumber = 1;
            
            var latestWeek = await _chartRepository.QueryWeeksOf(user.Id)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();
            
            if (latestWeek != null)
            {
                syncStartDate = latestWeek.To.AddSeconds(1);
                startWeekNumber = latestWeek.WeekNumber + 1;
            }

            var userTimeZone = TZConvert.GetTimeZoneInfo(user.IanaTimezone);
            var syncResult = await _chartService.SyncWeeks(
                user,
                startWeekNumber,
                syncStartDate,
                null,
                userTimeZone);

            if (syncResult.IsFailed)
            {
                var lastFmError = syncResult.Errors.Find(error =>
                    error is LastFmResultError
                ) as LastFmResultError;

                return StatusCode(lastFmError!.StatusCode, new LastFmErrorDto()
                {
                    ErrorCode = lastFmError!.LastFmErrorCode ?? -1,
                    Message = lastFmError!.LastFmErrorMessage ?? "Last.fm service unavailable."
                });
            }

            var syncedWeeks = syncResult.Value;
            var weekDtos = syncedWeeks
                .OrderBy(week => week.WeekNumber)
                .Select(week => new UserWeekDto()
                {
                    Id = week.Id,
                    OwnerId = week.OwnerId,
                    WeekNumber = week.WeekNumber,
                    From = week.From,
                    To = week.To
                })
                .ToList();

            return Ok(new UserWeeksDto { Weeks = weekDtos });
        }

        [HttpGet]
        [Route("outdated-weeks")]
        public async Task<ActionResult<UserWeeksDto>> GetOutdatedWeeks()
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userName == null)
            {
                return Unauthorized();
            }

            var user = await _userService.FindUserFromUserName(userName);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.IanaTimezone == null)
            {
                return BadRequest();
            }

            var userTimeZone = TZConvert.GetTimeZoneInfo(user.IanaTimezone);
            var utcNow = DateTime.UtcNow;
            var outdatedWeeks = await _chartDateService.GetOutdatedWeeks(
                user.Id,
                user.RegisteredAt,
                utcNow,
                userTimeZone);

            return new UserWeeksDto()
            {
                Weeks = outdatedWeeks.Select(week => new UserWeekDto()
                {
                    OwnerId = user.Id,
                    WeekNumber = week.WeekNumber,
                    From = week.From,
                    To = week.To,
                }).ToList()
            };
        }
    }
}