using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    [Route("api/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly ILastFmService _lastFm;
        private readonly IUserService _userService;

        public AuthorizationController(ILastFmService lastFmService, IUserService userService)
        {
            _lastFm = lastFmService;
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("url")]
        public ActionResult<AuthorizationUrlDto> GetAuthorizationUrl([FromQuery] string? callback)
        {
            return Ok(new AuthorizationUrlDto(_lastFm.GetAuthorizationUrl(callback)));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("authorize")]
        public async Task<ActionResult<UserDto>> Authorize(AuthorizeDto dto)
        {
            var sessionResult = await _lastFm.CreateLastFmSession(dto.Token);
            if (sessionResult.IsFailed)
            {
                return HandleLastFmError(sessionResult);
            }


            var session = sessionResult.Value;
            var user = await _userService.FindUserFromUserName(session.LastFmUser);
            if (user == null)
            {
                var userInfo = await _lastFm.GetUserInfo(null, session.Key);
                if (userInfo.IsFailed)
                {
                    return HandleLastFmError(sessionResult);
                }

                user = await _userService.GetOrCreateUserFromInfo(userInfo.Value);
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, session.LastFmUser),
                new Claim("SessionKey", session.Key)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

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
        [Route("sign-out")]
        public async Task<ActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        private ActionResult HandleLastFmError(Result failedResult)
        {
            var lastFmError = failedResult.Errors.Find(error =>
                    error is LastFmResultError
                ) as LastFmResultError;

            return StatusCode(lastFmError!.StatusCode, new LastFmErrorDto()
            {
                ErrorCode = lastFmError!.LastFmErrorCode ?? -1,
                Message = lastFmError!.LastFmErrorMessage ?? "Last.fm service unavailable."
            });
        }
    }
}