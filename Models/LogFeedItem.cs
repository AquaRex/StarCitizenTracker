using System;
using System.Collections.Generic;

namespace StarCitizenTracker.Models
{
    public class LogFeedItem
    {
        public string Killer { get; set; }
        public string Victim { get; set; }
        public string Weapon { get; set; }
        public string DamageType { get; set; }
        public List<string> Inventory { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; }
    }

    public class SecondaryLogFeedItem
    {
        public string Text { get; set; }
        public System.Drawing.Color Color { get; set; }
        public string[] TextParts { get; set; }
        public System.Drawing.Color[] Colors { get; set; }
        public DateTime Timestamp { get; set; }
    }
}