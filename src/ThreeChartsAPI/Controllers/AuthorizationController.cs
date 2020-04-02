using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeChartsAPI.Models.Dtos;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Models.LastFm.Dtos;
using ThreeChartsAPI.Services.LastFm;

namespace ThreeChartsAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly ILastFmService _lastFm;

        public AuthorizationController(ILastFmService lastFmService)
        {
            _lastFm = lastFmService;
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
        public async Task<ActionResult> Authorize(AuthorizeDto dto)
        {
            var sessionResult = await _lastFm.CreateLastFmSession(dto.Token);
            if (sessionResult.IsFailed)
            {
                var lastFmError = sessionResult.Errors.Find(error =>
                    error is LastFmResultError
                ) as LastFmResultError;

                return StatusCode(lastFmError!.StatusCode, new LastFmErrorDto()
                {
                    ErrorCode = lastFmError!.LastFmErrorCode ?? -1,
                    Message = lastFmError!.LastFmErrorMessage ?? "Last.fm service unavailable."
                });
            }

            var session = sessionResult.Value;

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

            return NoContent();
        }

        [HttpPost]
        [Route("sign-out")]
        public async Task<ActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }
    }
}