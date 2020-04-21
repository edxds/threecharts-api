using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeChartsAPI.Features.Charts.Dtos;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.Users.Dtos;

namespace ThreeChartsAPI.Features.Charts
{
    [Authorize]
    [ApiController]
    [Route("api/charts")]
    public class ChartController : ControllerBase
    {
        private readonly IChartService _chartService;

        public ChartController(IChartService chartService)
        {
            _chartService = chartService;
        }

        [HttpGet("weeks/{ownerId}")]
        public async Task<ActionResult<UserWeeksDto>> GetWeeks(int ownerId)
        {
            var weeks = await _chartService.GetUserChartWeeks(ownerId);
            var weekDtos = weeks
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

            return Ok(new UserWeeksDto() { Weeks = weekDtos });
        }

        [HttpGet("weeks/{ownerId}/{weekId}")]
        public async Task<ActionResult<ChartsDto>> GetUserCharts(int ownerId, int weekId)
        {
            var week = await _chartService.GetChartWeek(ownerId, weekId);
            if (week == null)
            {
                return NotFound();
            }

            var chartsDto = new ChartsDto()
            {
                OwnerId = week.OwnerId,
                WeekId = week.Id,
                WeekNumber = week.WeekNumber,

                TrackEntries = week.ChartEntries
                    .Where(entry => entry.Type == ChartEntryType.Track)
                    .OrderBy(entry => entry.Rank)
                    .Select(entry => new TrackEntryDto()
                    {
                        Id = entry.Id,
                        Rank = entry.Rank,
                        Stat = entry.Stat,
                        StatText = entry.StatText,
                        TrackId = entry.Track!.Id,
                        Title = entry.Track!.Title,
                        ArtistName = entry.Track!.ArtistName,
                    })
                    .ToList(),

                AlbumEntries = week.ChartEntries
                    .Where(entry => entry.Type == ChartEntryType.Album)
                    .OrderBy(entry => entry.Rank)
                    .Select(entry => new AlbumEntryDto()
                    {
                        Id = entry.Id,
                        Rank = entry.Rank,
                        Stat = entry.Stat,
                        StatText = entry.StatText,
                        AlbumId = entry.Album!.Id,
                        Title = entry.Album!.Title,
                        ArtistName = entry.Album!.ArtistName,
                    })
                    .ToList(),

                ArtistEntries = week.ChartEntries
                    .Where(entry => entry.Type == ChartEntryType.Artist)
                    .OrderBy(entry => entry.Rank)
                    .Select(entry => new ArtistEntryDto()
                    {
                        Id = entry.Id,
                        Stat = entry.Stat,
                        Rank = entry.Rank,
                        StatText = entry.StatText,
                        ArtistId = entry.Artist!.Id,
                        Name = entry.Artist!.Name,
                    })
                    .ToList(),
            };

            return Ok(chartsDto);
        }
    }
}