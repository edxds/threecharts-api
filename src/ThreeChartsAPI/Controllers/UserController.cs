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
        private readonly ILastFmService _lastFm;

        public UserController(
            IUserService userService,
            IOnboardingService onboardingService,
            ILastFmService lastFmService)
        {
            _userService = userService;
            _onboardingService = onboardingService;
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
        [Route("onboard")]
        public async Task<ActionResult> Onboard()
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

            var onboardingResult = await _onboardingService.OnboardUser(user, null);
            if (onboardingResult.IsFailed)
            {
                var lastFmError = onboardingResult.Errors.Find(error =>
                    error is LastFmResultError
                ) as LastFmResultError;

                return StatusCode(lastFmError!.StatusCode, new LastFmErrorDto()
                {
                    ErrorCode = lastFmError!.LastFmErrorCode ?? -1,
                    Message = lastFmError!.LastFmErrorMessage ?? "Last.fm service unavailable."
                });
            }

            return NoContent();
        }
    }
}