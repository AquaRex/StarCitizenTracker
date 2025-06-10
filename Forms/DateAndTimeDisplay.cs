using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StarCitizenTracker.Elements;

namespace StarCitizenTracker.Forms
{
    public class DateAndTimeDisplay : TrackerWindow
    {
        private string currentTimeString = "";

        private Font timeFont = new Font("Consolas", 10F, FontStyle.Bold);
        private Color textColor = Color.LightGray;

        private bool isLocked = false;
        private Button lockButton;
        private Button closeButton;

        public DateAndTimeDisplay()
        {
            InitializeFeedComponents();
        }

        //------------------------ INITIALIZE WINDOW ------------------------

        private void InitializeFeedComponents()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Size = new Size(175, 15);
            this.Location = new Point(screen.Width - 300, screen.Height - 120);
            this.Paint += OnPaint;
        }

        //------------------------ ADD FEED ITEM ------------------------

        public void UpdateDateAndTime(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDateAndTime(text)));
                return;
            }

            string formattedTime = text;
            var match = Regex.Match(text, @"<(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2}:\d{2})");

            if (match.Success)
            {
                // Rebuild string format "YYYY-MM-DD  HH:MM:SS"
                formattedTime = $"{match.Groups[1].Value}  {match.Groups[2].Value}";
            }

            currentTimeString = formattedTime;
            this.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(this.BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (isLocked)
            {
                g.Clear(this.TransparencyKey);
            }

            // Center the time string in the window
            using (var brush = new SolidBrush(textColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                var drawRect = new Rectangle(0, 0, this.Width, this.Height);
                g.DrawString(currentTimeString, timeFont, brush, drawRect, sf);
            }
        }

        //------------------------ DISPOSE ------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timeFont?.Dispose();
                lockButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}