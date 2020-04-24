using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Features.Charts.Dtos;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Dtos;
using ThreeChartsAPI.Features.Users;
using ThreeChartsAPI.Features.Users.Dtos;

namespace ThreeChartsAPI.Features.Charts
{
    [Authorize]
    [ApiController]
    [Route("api/charts")]
    public class ChartController : ControllerBase
    {
        private readonly ChartRepository _repo;
        private readonly IChartService _chartService;
        private readonly IUserService _userService;

        public ChartController(
            ChartRepository repo,
            IChartService chartService,
            IUserService userService)
        {
            _repo = repo;
            _chartService = chartService;
            _userService = userService;
        }

        [HttpGet("weeks/{ownerId}")]
        public async Task<ActionResult<UserWeeksDto>> GetWeeks(int ownerId)
        {
            var weeks = await _repo.QueryWeeksOf(ownerId)
                .OrderBy(week => week.WeekNumber)
                .Select(
                    week => new UserWeekDto()
                    {
                        Id = week.Id,
                        OwnerId = week.OwnerId,
                        WeekNumber = week.WeekNumber,
                        From = week.From,
                        To = week.To
                    })
                .ToListAsync();

            return Ok(new UserWeeksDto { Weeks = weeks });
        }

        [HttpGet("weeks/{ownerId}/{weekId}")]
        public async Task<ActionResult<ChartsDto>> GetUserCharts(int ownerId, int weekId)
        {
            var week = await _repo
                .QueryWeeksWithRelationsOf(ownerId)
                .FirstOrDefaultAsync(w => w.Id == weekId);
            
            if (week == null)
            {
                return NotFound();
            }
            
            return Ok(MakeChartsDtoFrom(week));
        }

        [HttpGet("live")]
        public async Task<ActionResult<ChartsDto>> GetLiveChartForUser(CancellationToken token)
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
            
            var week = await _chartService.GetLiveWeekFor(user, DateTime.UtcNow);
            
            if (week.IsFailed)
            {
                var lastFmError = week.Errors.Find(error =>
                    error is LastFmResultError
                ) as LastFmResultError;

                return StatusCode(lastFmError!.StatusCode, new LastFmErrorDto()
                {
                    ErrorCode = lastFmError!.LastFmErrorCode ?? -1,
                    Message = lastFmError!.LastFmErrorMessage ?? "Last.fm service unavailable."
                });
            }
            
            return Ok(MakeChartsDtoFrom(week.Value));
        }
        
        private ChartsDto MakeChartsDtoFrom(ChartWeek week) =>
            new ChartsDto
            {
                OwnerId = week.OwnerId,
                WeekId = week.Id,
                WeekNumber = week.WeekNumber,

                TrackEntries = week.ChartEntries
                    .Where(entry => entry.Type == ChartEntryType.Track)
                    .OrderBy(entry => entry.Rank)
                    .Select(entry => new EntryDto
                    {
                        Id = entry.Id,
                        Type = entry.Type,
                        Rank = entry.Rank,
                        Stat = entry.Stat,
                        StatText = entry.StatText,
                        Title = entry.Title,
                        Artist = entry.Artist,
                    })
                    .ToList(),

                AlbumEntries = week.ChartEntries
                    .Where(entry => entry.Type == ChartEntryType.Album)
                    .OrderBy(entry => entry.Rank)
                    .Select(entry => new EntryDto
                    {
                        Id = entry.Id,
                        Type = entry.Type,
                        Rank = entry.Rank,
                        Stat = entry.Stat,
                        StatText = entry.StatText,
                        Title = entry.Title,
                        Artist = entry.Artist,
                    })
                    .ToList(),

                ArtistEntries = week.ChartEntries
                    .Where(entry => entry.Type == ChartEntryType.Artist)
                    .OrderBy(entry => entry.Rank)
                    .Select(entry => new EntryDto
                    {
                        Id = entry.Id,
                        Type = entry.Type,
                        Stat = entry.Stat,
                        Rank = entry.Rank,
                        StatText = entry.StatText,
                        Artist = entry.Artist,
                    })
                    .ToList(),
            };
    }
}