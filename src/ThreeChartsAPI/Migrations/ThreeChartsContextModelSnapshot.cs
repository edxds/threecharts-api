﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ThreeChartsAPI.Features.Charts.Models;

namespace ThreeChartsAPI.Migrations
{
    [DbContext(typeof(ThreeChartsContext))]
    partial class ThreeChartsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:Enum:chart_entry_stat", "new,reentry,no_diff,increase,decrease")
                .HasAnnotation("Npgsql:Enum:chart_entry_type", "album,artist,track")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ThreeChartsAPI.Models.Album", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ArtistName")
                        .IsRequired()
                        .HasColumnName("artist_name")
                        .HasColumnType("text");

                    b.Property<string>("ArtworkUrl")
                        .HasColumnName("artwork_url")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_albums");

                    b.ToTable("albums");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.Artist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ArtworkUrl")
                        .HasColumnName("artwork_url")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_artists");

                    b.ToTable("artists");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.ChartEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("AlbumId")
                        .HasColumnName("album_id")
                        .HasColumnType("integer");

                    b.Property<int?>("ArtistId")
                        .HasColumnName("artist_id")
                        .HasColumnType("integer");

                    b.Property<int>("Rank")
                        .HasColumnName("rank")
                        .HasColumnType("integer");

                    b.Property<ChartEntryStat>("Stat")
                        .HasColumnName("stat")
                        .HasColumnType("chart_entry_stat");

                    b.Property<string>("StatText")
                        .HasColumnName("stat_text")
                        .HasColumnType("text");

                    b.Property<int?>("TrackId")
                        .HasColumnName("track_id")
                        .HasColumnType("integer");

                    b.Property<ChartEntryType>("Type")
                        .HasColumnName("type")
                        .HasColumnType("chart_entry_type");

                    b.Property<int>("WeekId")
                        .HasColumnName("week_id")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("pk_chart_entries");

                    b.HasIndex("AlbumId")
                        .HasName("ix_chart_entries_album_id");

                    b.HasIndex("ArtistId")
                        .HasName("ix_chart_entries_artist_id");

                    b.HasIndex("TrackId")
                        .HasName("ix_chart_entries_track_id");

                    b.HasIndex("WeekId")
                        .HasName("ix_chart_entries_week_id");

                    b.ToTable("chart_entries");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.ChartWeek", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("From")
                        .HasColumnName("from")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("OwnerId")
                        .HasColumnName("owner_id")
                        .HasColumnType("integer");

                    b.Property<DateTime>("To")
                        .HasColumnName("to")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("WeekNumber")
                        .HasColumnName("week_number")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("pk_chart_weeks");

                    b.HasIndex("OwnerId")
                        .HasName("ix_chart_weeks_owner_id");

                    b.ToTable("chart_weeks");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.SpotifyApiToken", b =>
                {
                    b.Property<string>("Token")
                        .HasColumnName("token")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnName("expires_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnName("type")
                        .HasColumnType("text");

                    b.HasKey("Token")
                        .HasName("pk_spotify_api_tokens");

                    b.ToTable("spotify_api_tokens");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.Track", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ArtistName")
                        .IsRequired()
                        .HasColumnName("artist_name")
                        .HasColumnType("text");

                    b.Property<string>("ArtworkUrl")
                        .HasColumnName("artwork_url")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_tracks");

                    b.ToTable("tracks");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("IanaTimezone")
                        .HasColumnName("iana_timezone")
                        .HasColumnType("text");

                    b.Property<string>("LastFmUrl")
                        .HasColumnName("last_fm_url")
                        .HasColumnType("text");

                    b.Property<string>("ProfilePicture")
                        .HasColumnName("profile_picture")
                        .HasColumnType("text");

                    b.Property<string>("RealName")
                        .HasColumnName("real_name")
                        .HasColumnType("text");

                    b.Property<DateTime>("RegisteredAt")
                        .HasColumnName("registered_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("user_name")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users");
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.ChartEntry", b =>
                {
                    b.HasOne("ThreeChartsAPI.Models.Album", "Album")
                        .WithMany("Entries")
                        .HasForeignKey("AlbumId")
                        .HasConstraintName("fk_chart_entries_albums_album_id");

                    b.HasOne("ThreeChartsAPI.Models.Artist", "Artist")
                        .WithMany("Entries")
                        .HasForeignKey("ArtistId")
                        .HasConstraintName("fk_chart_entries_artists_artist_id");

                    b.HasOne("ThreeChartsAPI.Models.Track", "Track")
                        .WithMany("Entries")
                        .HasForeignKey("TrackId")
                        .HasConstraintName("fk_chart_entries_tracks_track_id");

                    b.HasOne("ThreeChartsAPI.Models.ChartWeek", "Week")
                        .WithMany("ChartEntries")
                        .HasForeignKey("WeekId")
                        .HasConstraintName("fk_chart_entries_chart_weeks_week_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ThreeChartsAPI.Models.ChartWeek", b =>
                {
                    b.HasOne("ThreeChartsAPI.Models.User", "Owner")
                        .WithMany("ChartWeeks")
                        .HasForeignKey("OwnerId")
                        .HasConstraintName("fk_chart_weeks_users_owner_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
