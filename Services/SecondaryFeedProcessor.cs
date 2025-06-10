using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StarCitizenTracker.Interfaces;
using StarCitizenTracker.Models;
using StarCitizenTracker.Config;

namespace StarCitizenTracker.Services
{
    public class SecondaryFeedProcessor
    {
        private FeedProcessor feedProcessor;
        private string playerBeingCorpsified = null;
        private List<string> collectedItems = new List<string>();
        private IIDConverter idConverter;

        private List<CommandPattern> commandPatterns;
        private Dictionary<string, RepeatedCommandEvent> repeatedCommandEvents = new Dictionary<string, RepeatedCommandEvent>();
        private Dictionary<string, Timer> commandEventTimers = new Dictionary<string, Timer>();
        private const int COMMAND_EVENT_TIMEOUT = 5000;


        private class RepeatedCommandEvent
        {
            public Dictionary<string, HashSet<string>> CapturedData { get; set; } = new Dictionary<string, HashSet<string>>();
            public CommandPattern Pattern { get; set; }
        }

        public class CommandPattern
        {
            public Regex Regex { get; }
            public string[] EventKeyGroups { get; } // CHANGED from string EventKey
            public string OutputFormat { get; }
            public Color OutputColor { get; }
            public string[] AggregatedKeys { get; }

            // Note the changed constructor parameter
            public CommandPattern(string regex, string[] eventKeyGroups, string outputFormat, Color outputColor, string[] aggregatedKeys)
            {
                Regex = new Regex(regex, RegexOptions.Compiled);
                EventKeyGroups = eventKeyGroups; // CHANGED
                OutputFormat = outputFormat;
                OutputColor = outputColor;
                AggregatedKeys = aggregatedKeys;
            }
        }

        //------------------------ START PROCESSOR ------------------------

        public void StartProcessor(FeedProcessor mainProcessor)
        {
            feedProcessor = mainProcessor;
            idConverter = new LocationIdConverter();
            InitializeCommandPatterns();
        }

        //------------------------ PROCESS LINE ------------------------

        public void ProcessLine(string line)
        {

            //------------------------ RUN PATTERN CHECKS ------------------------
            foreach (var pattern in commandPatterns)
            {
                var match = pattern.Regex.Match(line);
                if (match.Success)
                {
                    var eventId = string.Join("-", pattern.EventKeyGroups.Select(g => match.Groups[g].Value));

                    if (repeatedCommandEvents.ContainsKey(eventId))
                    {
                        var commandEvent = repeatedCommandEvents[eventId];
                        foreach (string groupName in pattern.Regex.GetGroupNames())
                        {
                            if (match.Groups[groupName].Success)
                            {
                                if (!commandEvent.CapturedData.ContainsKey(groupName))
                                {
                                    commandEvent.CapturedData[groupName] = new HashSet<string>();
                                }
                                commandEvent.CapturedData[groupName].Add(match.Groups[groupName].Value);
                            }
                        }
                        commandEventTimers[eventId].Stop();
                        commandEventTimers[eventId].Start();
                    }
                    else
                    {
                        var newEvent = new RepeatedCommandEvent { Pattern = pattern };
                        foreach (string groupName in pattern.Regex.GetGroupNames())
                        {
                            if (match.Groups[groupName].Success)
                            {
                                newEvent.CapturedData[groupName] = new HashSet<string> { match.Groups[groupName].Value };
                            }
                        }
                        repeatedCommandEvents[eventId] = newEvent;

                        var newTimer = new Timer { Interval = COMMAND_EVENT_TIMEOUT };
                        newTimer.Tag = eventId;
                        newTimer.Tick += CommandTimer_Tick;
                        newTimer.Start();
                        commandEventTimers[eventId] = newTimer;
                    }
                    return;
                }
            }

            //------------------------ PLAYER RESPAWNED ------------------------

            if (TrackerConfig.Instance.TrackNearbyDeaths && Regex.IsMatch(line, @"CSCPlayerPUSpawningComponent::UnregisterFromExternalSystems.*lost reservation for spawnpoint", RegexOptions.IgnoreCase))
            {
                var playerName = ExtractMatch(line, @"Player '([^']+)'");
                var spawnpoint = ExtractMatch(line, @"lost reservation for spawnpoint ([^\s\[]+)");
                var location = ExtractMatch(line, @"at location (\d+)");
                var locationName = idConverter.ConvertLocationID(location);
                feedProcessor.AddSecondaryLine($"Player: #red{{{playerName}}}, #1d991d{{Respawned}} in {spawnpoint} ( #white{{{locationName}}} )", Color.FromArgb(150, 150, 150));
            }

            //------------------------ OBJECT DETACHMENT EVENTS ------------------------

            else if (TrackerConfig.Instance.TrackNearObjectDetachment && Regex.IsMatch(line, @"CEntity::OnOwnerRemoved.*unblock removal of parent", RegexOptions.IgnoreCase))
            {
                var matches = Regex.Matches(line, @"name = ""([^""]+)""");
                var playerName = matches.Count > 0 ? matches[0].Groups[1].Value : "Unknown";
                var parentName = matches.Count > 1 ? matches[1].Groups[1].Value : "Unknown Parent";
                feedProcessor.AddSecondaryLine($"Object:  #red{{{playerName}}}, detached from #white{{{parentName}}}", Color.FromArgb(150, 150, 150));
            }

            //------------------------ MARK NEAREST HOSPITAL ------------------------

            else if (TrackerConfig.Instance.TrackNearbyDeaths && Regex.IsMatch(line, @"\[Notice\].*\[ACTOR STATE\].*Player '([^']+)'.*DoesLocationContainHospital: Nearby hospital", RegexOptions.IgnoreCase))
            {
                var playerName = ExtractMatch(line, @"Player '([^']+)'");
                var hospitalLocation = ExtractMatch(line, @"(?<=DoesLocationContainHospital: Nearby hospital\s)[^\.]+");
                feedProcessor.AddSecondaryLine($"Player: #red{{{playerName}}}, closest hospital {hospitalLocation}", Color.FromArgb(150, 150, 150));
            }

            //------------------------ NPC KILLS ------------------------

            else if (TrackerConfig.Instance.TrackNPCKills && Regex.IsMatch(line, @"\bkilled\b", RegexOptions.IgnoreCase))
            {
                var killer = ExtractMatch(line, @"killed by '([^']+)'");
                var victim = ExtractMatch(line, @"Kill: '([^']+)'");
                if (victim.Contains("_NPC_") || victim.Contains("_PU_"))
                {
                    feedProcessor.AddSecondaryLine($"Player: #red{{{killer}}}, killed NPC: #white{{{victim}}}", Color.FromArgb(200, 200, 200));
                }
            }

            //------------------------ IGNORING COLLISION DAMAGE ( NEARBY COLLISION EVENT ) ------------------------

            else if (TrackerConfig.Instance.TrackShipAttacks && Regex.IsMatch(line, @"\[Notice\].*\<Body Hit Discarded>.*Actor '([^']+)'.*ignoring collision damage", RegexOptions.IgnoreCase))
            {
                var playerName = ExtractMatch(line, @"Actor '([^']+)'");
                var collider = ExtractMatch(line, @"from '([^']+)'");
                var reason = ExtractMatch(line, @"(?<=from '[^']+' )(.*)");
                feedProcessor.AddSecondaryLine($"Player: #red{{{playerName}}}, ignoring collision damage from #white{{[ {collider}, {reason} ]}}", Color.FromArgb(200, 200, 200));
            }

            //------------------------ NEARBY SHIP DESTROYED ------------------------

            else if (TrackerConfig.Instance.TrackShipAttacks && Regex.IsMatch(line, @"<Vehicle Destruction>.*caused by", RegexOptions.IgnoreCase))
            {
                var vehicleName = ExtractMatch(line, @"Vehicle '([^']+)'");
                var zone = ExtractMatch(line, @"in zone '([^']+)'");
                var cause = ExtractMatch(line, @"caused by '([^']+)'");
                feedProcessor.AddSecondaryLine($"Nearby Ship: #white{{{vehicleName}}} #red{{DESTROYED}} in zone #white{{{zone}}} by {cause}", Color.FromArgb(200, 200, 200));
            }

            //------------------------ ACTOR STALL, NEARBY PLAYER DESYNCED ------------------------
            // Seems to trigger from players that potentially are on player screen and needs to be resynced
            // Could mean a player is very close to our location!
            else if (TrackerConfig.Instance.TrackNearbyMovements && Regex.IsMatch(line, @"<Actor stall> Actor stall detected", RegexOptions.IgnoreCase))
            {
                var playerName = ExtractMatch(line, @"Player: ([^,]+),");
                var stallType = ExtractMatch(line, @"Type: ([^,]+),");
                var stallLength = ExtractMatch(line, @"Length: ([\d\.]+)\.");
                feedProcessor.AddSecondaryLine($"#red{{Nearby Player Detected:}} #red{{{playerName}}}, Type: #white{{{stallType}}}, stall length: #white{{{stallLength}s}}", Color.FromArgb(200, 200, 200));
            }
        }

        //------------------------ COMMAND PATTERN ------------------------

        private void InitializeCommandPatterns()
        {
            commandPatterns = new List<CommandPattern>();

            //------------------------ COMMAND PATTERNS ------------------------

            if (TrackerConfig.Instance.TrackNearbyDeaths && TrackerConfig.Instance.TrackNearbyInventory)
            {
                commandPatterns.Add(new CommandPattern(
                    regex: @"\[OnHandleHit\] Fake hit FROM (?<attacker>[^ ]+) TO (?<target>[^ ]+)\. Being sent to child (?<child>[^ \[\]]+)",
                    eventKeyGroups: new[] { "target" },
                    outputFormat: "#red{{attacker}} attacked #white{{target}} with crew: ( #red{{child}} )",
                    outputColor: Color.FromArgb(255, 165, 0),
                    aggregatedKeys: new[] { "child" }
                ));
            }


            // if (TrackerConfig.Instance.SomeOtherFlag)
            // {
            //     commandPatterns.Add(...)
            // }

        }

        //------------------------ TIMER FOR PATTERN DETECTION ------------------------

        private void CommandTimer_Tick(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            if (timer == null) return;
            var eventId = timer.Tag.ToString();

            if (repeatedCommandEvents.ContainsKey(eventId))
            {
                var commandEvent = repeatedCommandEvents[eventId];
                var pattern = commandEvent.Pattern;
                string formattedString = pattern.OutputFormat;

                foreach (var key in commandEvent.CapturedData.Keys)
                {
                    string replacement;
                    if (Array.Exists(pattern.AggregatedKeys, k => k == key))
                    {
                        replacement = string.Join(", ", commandEvent.CapturedData[key]);
                    }
                    else
                    {
                        replacement = commandEvent.CapturedData[key].First();
                    }
                    formattedString = formattedString.Replace($"{{{key}}}", replacement);
                }

                feedProcessor.AddSecondaryLine(formattedString, pattern.OutputColor);
            }

            // Cleanup
            timer.Stop();
            timer.Dispose();
            repeatedCommandEvents.Remove(eventId);
            commandEventTimers.Remove(eventId);
        }

        private string ExtractMatch(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        //------------------------ DISPOSE ------------------------

        public void Dispose()
        {
            foreach (var timer in commandEventTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}
