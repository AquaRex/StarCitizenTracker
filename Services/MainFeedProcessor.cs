using StarCitizenTracker.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StarCitizenTracker.Config;

namespace StarCitizenTracker.Services
{
    public class MainFeedProcessor
    {
        private FeedProcessor feedProcessor;
        private string playerBeingCorpsified = null;
        private List<string> collectedItems = new List<string>();

        //------------------------ START PROCESSOR ------------------------

        public void StartProcessor(FeedProcessor mainProcessor)
        {
            feedProcessor = mainProcessor;
        }

        //------------------------ PROCESS LINE ------------------------

        public void ProcessLine(string line)
        {
            var timeMatch = Regex.Match(line, @"^<[^>]+>");
            if (timeMatch.Success)
            {
                feedProcessor.UpdateDateAndTime(timeMatch.Value);
            }

            if (playerBeingCorpsified != null && line.Contains("Adding non kept item"))
            {
                var itemName = ExtractMatch(line, @"Class\(([^)]+)\)");
                if (itemName != "Unknown")
                {
                    collectedItems.Add(itemName);
                }
                return;
            }
            else if (playerBeingCorpsified != null)
            {
                // Send list to inventoryFeed
                feedProcessor.UpdateInventory(playerBeingCorpsified, new List<string>(collectedItems));

                // Reset
                playerBeingCorpsified = null;
                collectedItems.Clear();
            }

            if (TrackerConfig.Instance.TrackKillFeed && Regex.IsMatch(line, @"\bkilled\b", RegexOptions.IgnoreCase))
            {
                var killer = ExtractMatch(line, @"killed by '([^']+)'");
                var victim = ExtractMatch(line, @"Kill: '([^']+)'");
                var weapon = ExtractMatch(line, @"using '([^']+)'");
                var damageType = ExtractMatch(line, @"damage type '([^']+)'");
                if (victim.Contains("_NPC_") || victim.Contains("_PU_")) return;

                feedProcessor.AddMainLine(killer, victim, weapon, damageType);
            }
            else if (TrackerConfig.Instance.TrackNearbyDeaths && Regex.IsMatch(line, @"\[Notice\].*\[ACTOR STATE\].*Player '([^']+)'.*IsCorpseEnabled:", RegexOptions.IgnoreCase))
            {
                var playerName = ExtractMatch(line, @"Player '([^']+)'");
                var corpseStatus = ExtractMatch(line, @"(IsCorpseEnabled: [^\.]+)");

                feedProcessor.AddMainLine(playerName, "Corpse", corpseStatus.Trim(), "Death");
            }
            else if (TrackerConfig.Instance.TrackNearbyDeaths && Regex.IsMatch(line, @"\[Notice\].*\[ACTOR STATE\].*Player '([^']+)'.*Running corpsify for corpse", RegexOptions.IgnoreCase))
            {
                var playerName = ExtractMatch(line, @"Player '([^']+)'");

                feedProcessor.AddMainLine(playerName, "Corpsified", "Running corpsify for corpse", "Corpsify");

                playerBeingCorpsified = playerName;
                collectedItems.Clear();
            }
        }

        private string ExtractMatch(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        //------------------------ DISPOSE ------------------------

        public void Dispose()
        {
            
        }
    }
}
