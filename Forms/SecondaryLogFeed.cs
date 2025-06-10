using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StarCitizenTracker.Elements;
using StarCitizenTracker.Interfaces;
using StarCitizenTracker.Models;
using StarCitizenTracker.Services;

namespace StarCitizenTracker.Forms
{
    public class SecondaryLogFeed : TrackerWindow
    {
        private Timer removeTimer;
        private List<SecondaryLogFeedItem> secondaryItems = new List<SecondaryLogFeedItem>();
        private const int MAX_SECONDARY_ITEMS = 12;
        private const int SECONDARY_ITEM_HEIGHT = 20;
        private const int SECONDARY_ITEM_REMOVE_TIME = 120000;
        private Font secondaryFont = new Font("Segoe UI", 8F, FontStyle.Regular);
        private Color playerNameColor = Color.FromArgb(254, 36, 37);

        private bool isLocked = false;
        private Button lockButton;
        private Button closeButton;

        public SecondaryLogFeed()
        {
            InitializeFeedComponents();
            SetupTimers();
        }

        //------------------------ INITIALIZE WINDOW ------------------------

        private void InitializeFeedComponents()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Size = new Size(800, MAX_SECONDARY_ITEMS * SECONDARY_ITEM_HEIGHT + 20);
            this.Location = new Point(screen.Width - this.Width - 580, 50);
            this.Paint += OnPaint;
        }

        private void SetupTimers()
        {
            removeTimer = new Timer { Interval = 8000 };
            removeTimer.Tick += (s, e) => RemoveOldSecondaryItems();
            removeTimer.Start();
        }

        //------------------------ ADD LINE ------------------------

        public void AddLine(string textWithFormatting, Color defaultColor)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLine(textWithFormatting, defaultColor)));
                return;
            }

            var textParts = new List<string>();
            var colors = new List<Color>();
            var regex = new Regex(@"#([a-zA-Z0-9]+)\{([^{}]+)\}", RegexOptions.Compiled);
            int lastIndex = 0;

            foreach (Match match in regex.Matches(textWithFormatting))
            {
                if (match.Index > lastIndex)
                {
                    textParts.Add(textWithFormatting.Substring(lastIndex, match.Index - lastIndex));
                    colors.Add(defaultColor);
                }

                string colorName = match.Groups[1].Value;
                string content = match.Groups[2].Value;

                textParts.Add(content);
                colors.Add(ParseColor(colorName, defaultColor));

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < textWithFormatting.Length)
            {
                textParts.Add(textWithFormatting.Substring(lastIndex));
                colors.Add(defaultColor);
            }

            secondaryItems.Insert(0, new SecondaryLogFeedItem
            {
                TextParts = textParts.ToArray(),
                Colors = colors.ToArray(),
                Timestamp = DateTime.Now
            });

            if (secondaryItems.Count > MAX_SECONDARY_ITEMS)
                secondaryItems.RemoveRange(MAX_SECONDARY_ITEMS, secondaryItems.Count - MAX_SECONDARY_ITEMS);
            Invalidate();
        }

        private Color ParseColor(string colorStr, Color defaultColor)
        {
            if (string.IsNullOrEmpty(colorStr)) return defaultColor;
            if (colorStr.ToLower() == "player") return playerNameColor;

            try
            {
                // Prepend # for hex values if missing, as FromHtml expects it
                if (Regex.IsMatch(colorStr, @"^[0-9A-Fa-f]{6}$"))
                {
                    return ColorTranslator.FromHtml($"#{colorStr}");
                }
                return Color.FromName(colorStr);
            }
            catch
            {
                return defaultColor; // Fallback
            }
        }

        private void RemoveOldSecondaryItems()
        {
            var cutoff = DateTime.Now.AddMilliseconds(-SECONDARY_ITEM_REMOVE_TIME);
            var initialCount = secondaryItems.Count;
            secondaryItems.RemoveAll(item => item.Timestamp < cutoff);
            if (secondaryItems.Count != initialCount) Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            int yOffset = 5;
            foreach (var item in secondaryItems)
            {
                DrawSecondaryItem(e.Graphics, item, yOffset);
                yOffset += SECONDARY_ITEM_HEIGHT;
            }
        }

        //------------------------ DRAW FEED ITEM ------------------------

        private void DrawSecondaryItem(Graphics g, SecondaryLogFeedItem item, int yOffset)
        {
            var bulletColor = item.Colors?.Length > 0 ? item.Colors[0] : Color.White;
            using (var brush = new SolidBrush(bulletColor))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillEllipse(brush, 5, yOffset + 8, 4, 4);
                g.SmoothingMode = SmoothingMode.None;
            }

            int currentX = 15;
            if (item.TextParts != null && item.Colors != null)
            {
                for (int i = 0; i < item.TextParts.Length && i < item.Colors.Length; i++)
                {
                    using (var brush = new SolidBrush(item.Colors[i]))
                    {
                        g.DrawString(item.TextParts[i], secondaryFont, brush, currentX, yOffset + 2);
                        currentX += (int)g.MeasureString(item.TextParts[i], secondaryFont).Width;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(item.Text))
            {
                using (var brush = new SolidBrush(item.Color))
                {
                    g.DrawString(item.Text, secondaryFont, brush, currentX, yOffset + 2);
                }
            }
        }

        //------------------------ CLEAR WINDOW ------------------------

        protected override void ClearWIndow()
        {
            secondaryItems.Clear();
            base.ClearWIndow();
        }

        //------------------------ DISPOSE ------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                secondaryFont?.Dispose();
                removeTimer?.Dispose();
                lockButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}