namespace ThreeChartsAPI.Tests
{
    public static class LastFmJsonDeserializerData
    {
        public static string WeeklyTrackChartJson = @"
            {
                ""weeklytrackchart"": {
                    ""@attr"": {
                        ""user"": ""edxds"",
                        ""from"": ""1584662400"",
                        ""to"": ""1585267199""
                    },
                    ""track"": [
                        {
                            ""artist"": {
                                ""mbid"": """",
                                ""#text"": ""ITZY""
                            },
                            ""@attr"": {
                                ""rank"": ""1""
                            },
                            ""mbid"": """",
                            ""url"": ""https://www.last.fm/music/ITZY/_/WANNABE"",
                            ""image"": [
                                {
                                    ""size"": ""small"",
                                    ""#text"": ""https://lastfm.freetls.fastly.net/i/u/34s/2a96cbd8b46e442fc41c2b86b821562f.png""
                                },
                                {
                                    ""size"": ""medium"",
                                    ""#text"": ""https://lastfm.freetls.fastly.net/i/u/64s/2a96cbd8b46e442fc41c2b86b821562f.png""
                                },
                                {
                                    ""size"": ""large"",
                                    ""#text"": ""https://lastfm.freetls.fastly.net/i/u/174s/2a96cbd8b46e442fc41c2b86b821562f.png""
                                }
                            ],
                            ""name"": ""WANNABE"",
                            ""playcount"": ""29""
                        },
                        {
                            ""artist"": {
                                ""mbid"": """",
                                ""#text"": ""Pabllo Vittar""
                            },
                            ""@attr"": {
                                ""rank"": ""2""
                            },
                            ""mbid"": """",
                            ""url"": ""https://www.last.fm/music/Pabllo+Vittar/_/Rajad%C3%A3o"",
                            ""image"": [
                                {
                                    ""size"": ""small"",
                                    ""#text"": ""https://lastfm.freetls.fastly.net/i/u/34s/2a96cbd8b46e442fc41c2b86b821562f.png""
                                },
                                {
                                    ""size"": ""medium"",
                                    ""#text"": ""https://lastfm.freetls.fastly.net/i/u/64s/2a96cbd8b46e442fc41c2b86b821562f.png""
                                },
                                {
                                    ""size"": ""large"",
                                    ""#text"": ""https://lastfm.freetls.fastly.net/i/u/174s/2a96cbd8b46e442fc41c2b86b821562f.png""
                                }
                            ],
                            ""name"": ""Rajadão"",
                            ""playcount"": ""29""
                        }
                    ]
                }
            }
        ";

        public static string WeeklyAlbumChartJson = @"
            {
                ""weeklyalbumchart"": {
                    ""album"": [
                        {
                            ""artist"": {
                                ""mbid"": """",
                                ""#text"": ""Dua Lipa""
                            },
                            ""@attr"": {
                                ""rank"": ""1""
                            },
                            ""mbid"": """",
                            ""playcount"": ""94"",
                            ""name"": ""Future Nostalgia"",
                            ""url"": ""https://www.last.fm/music/Dua+Lipa/Future+Nostalgia""
                        },
                        {
                            ""artist"": {
                                ""mbid"": """",
                                ""#text"": ""Pabllo Vittar""
                            },
                            ""@attr"": {
                                ""rank"": ""2""
                            },
                            ""mbid"": """",
                            ""playcount"": ""93"",
                            ""name"": ""111"",
                            ""url"": ""https://www.last.fm/music/Pabllo+Vittar/111""
                        }
                    ],
                    ""@attr"": {
                        ""user"": ""edxds"",
                        ""from"": ""1584662400"",
                        ""to"": ""1585267199""
                    }
                }
            }
        ";

        public static string WeeklyArtistChartJson = @"
            {
                ""weeklyartistchart"": {
                    ""artist"": [
                        {
                            ""@attr"": {
                                ""rank"": ""1""
                            },
                            ""mbid"": ""d9550ac2-7bcf-4655-a3fe-24922e254a74"",
                            ""playcount"": ""97"",
                            ""name"": ""Pabllo Vittar"",
                            ""url"": ""https://www.last.fm/music/Pabllo+Vittar""
                        },
                        {
                            ""@attr"": {
                                ""rank"": ""2""
                            },
                            ""mbid"": ""6f1a58bf-9b1b-49cf-a44a-6cefad7ae04f"",
                            ""playcount"": ""94"",
                            ""name"": ""Dua Lipa"",
                            ""url"": ""https://www.last.fm/music/Dua+Lipa""
                        }
                    ],
                    ""@attr"": {
                        ""user"": ""edxds"",
                        ""from"": ""1584662400"",
                        ""to"": ""1585267199""
                    }
                }
            }
        ";

        public static string SessionJson = @"
            {
                ""session"": {
                    ""subscriber"": 0,
                    ""name"": ""edxds"",
                    ""key"": ""session_key""
                }
            }
        ";
    }
}