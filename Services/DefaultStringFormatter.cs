using System.Drawing;
using StarCitizenTracker.Interfaces;

namespace StarCitizenTracker.Services
{
    public class DefaultStringFormatter : IStringFormatter
    {
        private Color killerColor = Color.FromArgb(254, 36, 37);
        private Color victimColor = Color.FromArgb(35, 129, 255);
        private Color weaponColor = Color.FromArgb(200, 200, 200);
        private Color textColor = Color.FromArgb(220, 220, 220);

        public FormattedStrings FormatStrings(string killer, string victim, string weapon, string damageType)
        {
            if (killer == victim)
            {
                return new FormattedStrings
                {
                    PrimaryText = new[] { killer, " killed ", "themselves" },
                    PrimaryColors = new[] { killerColor, textColor, victimColor },
                    SecondaryText = new[] { $"{damageType}" },
                    SecondaryColors = new[] { weaponColor }
                };
            }
            else
            {
                if (victim == "Corpse")
                {
                    if (weapon == "IsCorpseEnabled: No")
                    {
                        return new FormattedStrings
                        {
                            PrimaryText = new[] { killer, " died nearby" },
                            PrimaryColors = new[] { killerColor, textColor, victimColor },
                            SecondaryText = new[] { $"died in safe-zone" },
                            SecondaryColors = new[] { weaponColor }
                        };
                    }
                    else
                    {
                        if(weapon == "IsCorpseEnabled: Yes, there is no local inventory")
                        {
                            return new FormattedStrings
                            {
                                PrimaryText = new[] { killer, " died nearby" },
                                PrimaryColors = new[] { killerColor, textColor, victimColor },
                                SecondaryText = new[] { $"left behind an empty corpse" },
                                SecondaryColors = new[] { weaponColor }
                            };
                        }
                        else
                        {
                            return new FormattedStrings
                            {
                                PrimaryText = new[] { killer, " died nearby" },
                                PrimaryColors = new[] { killerColor, textColor, victimColor },
                                SecondaryText = new[] { $"left behind a lootable corpse" },
                                SecondaryColors = new[] { weaponColor }
                            };
                        }
                    }
                }
                if (victim == "Corpsified")
                {
                    return new FormattedStrings
                    {
                        PrimaryText = new[] { killer, " has respawned " },
                        PrimaryColors = new[] { killerColor, textColor, victimColor },
                        SecondaryText = new[] { $"after dying nearby" },
                        SecondaryColors = new[] { weaponColor }
                    };
                }
                if (damageType == "Crash")
                {
                    return new FormattedStrings
                    {
                        PrimaryText = new[] { killer, " killed ", victim },
                        PrimaryColors = new[] { killerColor, textColor, victimColor },
                        SecondaryText = new[] { $"by collision" },
                        SecondaryColors = new[] { weaponColor }
                    };
                }
                else
                {
                    return new FormattedStrings
                    {
                        PrimaryText = new[] { killer, " killed ", victim },
                        PrimaryColors = new[] { killerColor, textColor, victimColor },
                        SecondaryText = new[] { $"with {damageType} from {weapon}" },
                        SecondaryColors = new[] { weaponColor }
                    };
                }
            }
        }
    }
}