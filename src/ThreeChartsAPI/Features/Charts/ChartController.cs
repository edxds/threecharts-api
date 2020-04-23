using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ChartRepository _repo;

        public ChartController(ChartRepository repo)
        {
            _repo = repo;
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