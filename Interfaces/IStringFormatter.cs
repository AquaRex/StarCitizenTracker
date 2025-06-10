using System.Drawing;
using StarCitizenTracker.Models;

namespace StarCitizenTracker.Interfaces
{
    public interface IStringFormatter
    {
        FormattedStrings FormatStrings(string killer, string victim, string weapon, string damageType);
    }

    public class FormattedStrings
    {
        public string[] PrimaryText { get; set; }
        public Color[] PrimaryColors { get; set; }
        public string[] SecondaryText { get; set; }
        public Color[] SecondaryColors { get; set; }
    }
}
