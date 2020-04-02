using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeChartsAPI.Models.Dtos;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Models.LastFm.Dtos;
using ThreeChartsAPI.Services;
using ThreeChartsAPI.Services.LastFm;

namespace ThreeChartsAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILastFmService _lastFm;

        public UserController(IUserService userService, ILastFmService lastFmService)
        {
            _userService = userService;
            _lastFm = lastFmService;
        }

        [HttpGet]
        [Route("details")]
        public async Task<ActionResult<UserDto>> GetDetails()
        {
            var sessionKey = User.FindFirst("SessionKey").Value;
            if (sessionKey == null)
            {
                return Unauthorized();
            }

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

            var user = await _userService.GetOrCreateUserFromInfo(userInfo.Value);
            return Ok(new UserDto()
            {
                UserName = user.UserName,
                RealName = user.RealName,
                LastFmUrl = user.LastFmUrl,
                ProfilePicture = user.ProfilePicture,
                RegisteredAt = user.RegisteredAt,
            });
        }
    }
}