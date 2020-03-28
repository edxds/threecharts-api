using Microsoft.EntityFrameworkCore;

namespace ThreeChartsAPI.Models
{
    public class ThreeChartsContext : DbContext
    {
        public ThreeChartsContext(DbContextOptions<ThreeChartsContext> options) : base(options) { }

        public DbSet<Album> Albums { get; set; } = null!;
        public DbSet<Artist> Artists { get; set; } = null!;
        public DbSet<Track> Tracks { get; set; } = null!;

        public DbSet<ChartEntry> ChartEntries { get; set; } = null!;
        public DbSet<ChartWeek> ChartWeeks { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;
    }
}