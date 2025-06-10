using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace StarCitizenTracker.Services
{
    public class DefaultIconRenderer
    {
        private Dictionary<string, Image> iconCache = new Dictionary<string, Image>();

        public void DrawIcon(Graphics g, Rectangle rect, string killer, string victim, string weapon, string damageType)
        {
            var icon = GetIcon(killer, victim, weapon, damageType);
            if (icon != null)
            {
                DrawImageIcon(g, rect, icon);
            }
        }

        private Image GetIcon(string killer, string victim, string weapon, string damageType)
        {
            string iconKey = DetermineIconKey(killer, victim, weapon, damageType);

            if (iconCache.ContainsKey(iconKey))
                return iconCache[iconKey];

            var icon = LoadIcon(iconKey);
            if (icon != null)
                iconCache[iconKey] = icon;

            return icon;
        }

        private string DetermineIconKey(string killer, string victim, string weapon, string damageType)
        {
            if (killer == victim)
            {
                return "defaultKillIcon";
            }
            else
            {
                if (victim == "Corpse")
                {
                    return "defaultKillIcon";
                }
                if (victim == "Corpsified")
                {
                    return "respawnIcon";
                }
                if (weapon == "Unknown")
                {
                    return "defaultKillIcon";
                }
                else
                {
                    return "defaultKillIcon";
                }
            }
        }

        private Image LoadIcon(string iconName)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string[] possibleNames = {
                    $"StarCitizenLogOverlay.{iconName}.png",
                    $"StarCitizenLogOverlay.Resources.{iconName}.png",
                    $"{assembly.GetName().Name}.{iconName}.png",
                    $"{assembly.GetName().Name}.Resources.{iconName}.png"
                };

                foreach (string name in possibleNames)
                {
                    var stream = assembly.GetManifestResourceStream(name);
                    if (stream != null)
                        return Image.FromStream(stream);
                }
            }
            catch { }

            return null;
        }

        private void DrawImageIcon(Graphics g, Rectangle rect, Image icon)
        {
            var colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = 1.0f;

            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(icon, rect, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, imageAttributes);
            imageAttributes.Dispose();
        }
    }
}