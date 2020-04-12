using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ThreeChartsAPI.Models
{
    public class ThreeChartsContext : DbContext
    {
        static ThreeChartsContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ChartEntryStat>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ChartEntryType>();
        }

        public ThreeChartsContext(DbContextOptions<ThreeChartsContext> options) : base(options) { }

        public DbSet<SpotifyApiToken> SpotifyApiTokens { get; set; } = null!;
        
        public DbSet<Album> Albums { get; set; } = null!;
        public DbSet<Artist> Artists { get; set; } = null!;
        public DbSet<Track> Tracks { get; set; } = null!;

        public DbSet<ChartEntry> ChartEntries { get; set; } = null!;
        public DbSet<ChartWeek> ChartWeeks { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresEnum<ChartEntryStat>();
            builder.HasPostgresEnum<ChartEntryType>();
        }
    }
}