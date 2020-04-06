using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeChartsAPI.Models.Dtos;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Models.LastFm.Dtos;
using ThreeChartsAPI.Services;
using ThreeChartsAPI.Services.LastFm;
using ThreeChartsAPI.Services.Onboarding;

namespace ThreeChartsAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOnboardingService _onboardingService;
        private readonly IChartWeekService _chartWeekService;
        private readonly ILastFmService _lastFm;

        public UserController(
            IUserService userService,
            IOnboardingService onboardingService,
            IChartWeekService chartWeekService,
            ILastFmService lastFmService)
        {
            _userService = userService;
            _onboardingService = onboardingService;
            _chartWeekService = chartWeekService;
            _lastFm = lastFmService;
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

            var weeks = await _chartWeekService.GetUserChartWeeks(user.Id);
            var syncStartDate = user.RegisteredAt;
            var startWeekNumber = 0;
            if (weeks.Count > 0)
            {
                var lastestWeek = weeks.OrderByDescending(week => week.WeekNumber).First();
                syncStartDate = lastestWeek.To.AddSeconds(1);
                startWeekNumber = lastestWeek.WeekNumber + 1;
            }

            var syncResult = await _onboardingService.SyncWeeks(user, startWeekNumber, syncStartDate, null);
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

            var syncedWeeks = await _chartWeekService.GetUserChartWeeks(user.Id);
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

            return Ok(weekDtos);
        }
    }
}