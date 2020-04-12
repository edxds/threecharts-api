using System;
using System.ComponentModel.DataAnnotations;

namespace ThreeChartsAPI.Models
{
    public class SpotifyApiToken
    {
        [Key]
        public string Token { get; set; } = null!;
        public string Type { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}