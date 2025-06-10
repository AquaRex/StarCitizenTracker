using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StarCitizenTracker.Config
{
    public class TrackerConfig
    {
        private static readonly TrackerConfig instance = new TrackerConfig();
        public static TrackerConfig Instance => instance;

        private readonly Dictionary<string, string> _configValues;

        public string LogFilePath { get; }
        public bool TrackKillFeed { get; }
        public bool TrackNPCKills { get; }
        public bool TrackNearbyDeaths { get; }
        public bool TrackNearbyInventory { get; }
        public bool TrackShipAttacks { get; }
        public bool TrackNearbyMovements { get; }
        public bool TrackNearObjectDetachment { get; }

        //------------------------ READ CONFIG FILE ------------------------

        private TrackerConfig()
        {
            _configValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var configPath = Path.Combine(AppContext.BaseDirectory, "config.txt");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                            continue;
                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            _configValues[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            //------------------------ GET FILE PATH ------------------------

            LogFilePath = GetConfigString("LogFilePath", @"D:\Program Files\Roberts Space Industries\StarCitizen\LIVE\Game.log");

            //------------------------ BOOLEAN CONFIGS ------------------------

            bool.TryParse(GetConfigString("TrackKillFeed", "true"), out var trackKillFeed);
            TrackKillFeed = trackKillFeed;

            bool.TryParse(GetConfigString("TrackNPCKills", "true"), out var trackNearDeaths);
            TrackNearbyDeaths = trackNearDeaths;

            bool.TryParse(GetConfigString("TrackNearbyInventory", "true"), out var trackNearbyInventory);
            TrackNearbyInventory = trackNearbyInventory;

            bool.TryParse(GetConfigString("TrackNPCKills", "true"), out var trackNpcs);
            TrackNPCKills = trackNpcs;

            bool.TryParse(GetConfigString("TrackShipAttacks", "true"), out var trackShipAttacks);
            TrackShipAttacks = trackShipAttacks;

            bool.TryParse(GetConfigString("TrackNearbyMovements", "true"), out var trackNearbyMovements);
            TrackNearbyMovements = trackNearbyMovements;

            bool.TryParse(GetConfigString("TrackNearObjectDetachment", "true"), out var trackNearObjectDetachment);
            TrackNearObjectDetachment = trackNearObjectDetachment;
        }

        private string GetConfigString(string key, string defaultValue)
        {
            return _configValues.TryGetValue(key, out var value) ? value : defaultValue;
        }

        //------------------------ PUBLIC FUNCTIONS ------------------------

        public string GetLogFilePath()
        {
            return LogFilePath;
        }
    }
}