using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using StarCitizenTracker.Elements;
using StarCitizenTracker.Interfaces;
using StarCitizenTracker.Models;
using StarCitizenTracker.Services;

namespace StarCitizenTracker.Forms
{
    public partial class MainLogFeed : TrackerWindow
    {
        private Timer removeTimer;
        private FeedProcessor feedProcessor;
        private List<LogFeedItem> killFeedItems = new List<LogFeedItem>();
        private const int MAX_ITEMS = 8;
        private const int ITEM_HEIGHT = 45;
        private const int ITEM_REMOVE_TIME = 120000;
        private Font playerFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        private Font weaponFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        private IStringFormatter stringFormatter;
        private DefaultIconRenderer iconRenderer = new DefaultIconRenderer();
        private NotifyIcon trayIcon;

        private bool isLocked = false;
        private Button lockButton;
        private Button closeButton;

        public MainLogFeed()
        {
            InitializeFeedComponents();
            SetupTimers();
            SetupTrayIcon();
        }

        //------------------------ INITIALIZE WINDOW ------------------------

        private void InitializeFeedComponents()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Size = new Size(400, MAX_ITEMS * ITEM_HEIGHT + 20);
            this.Location = new Point(screen.Width - this.Width - 20, 50);
            this.Paint += OnPaint;

            stringFormatter = new DefaultStringFormatter();
            feedProcessor = new FeedProcessor();
            feedProcessor.StartProcessor(this);
        }

        private void SetupTimers()
        {
            removeTimer = new Timer { Interval = 10000 };
            removeTimer.Tick += (s, e) => RemoveOldItems();
            removeTimer.Start();
        }

        //------------------------ SYSTEM TRAY ICON ------------------------

        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();
            try
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                string resourceName = "StarCitizenTracker.Resources.defaultKillIcon.png";

                using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream != null)
                    {
                        using (Bitmap pngBitmap = new Bitmap(resourceStream))
                        {
                            IntPtr hIcon = pngBitmap.GetHicon();
                            trayIcon.Icon = Icon.FromHandle(hIcon);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Embedded tray icon resource not found: {resourceName}. Using default icon.");
                        trayIcon.Icon = SystemIcons.Application;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading embedded tray icon: {ex.Message}. Using default icon.");
                trayIcon.Icon = SystemIcons.Application;
            }

            trayIcon.Text = "StarCitizen Tracker";

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, TrayIcon_Exit);
            trayIcon.ContextMenuStrip = contextMenu;

            trayIcon.Visible = true;
        }

        private void TrayIcon_Exit(object sender, EventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            Application.Exit();
        }

        //------------------------ ADD LINE ------------------------

        public void AddLine(string killer, string victim, string weapon, string damageType)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLine(killer, victim, weapon, damageType)));
                return;
            }

            killFeedItems.Insert(0, new LogFeedItem
            {
                Killer = killer,
                Victim = victim,
                Weapon = weapon,
                DamageType = damageType,
                Timestamp = DateTime.Now
            });

            if (killFeedItems.Count > MAX_ITEMS)
                killFeedItems.RemoveRange(MAX_ITEMS, killFeedItems.Count - MAX_ITEMS);
            Invalidate();
        }

        private void RemoveOldItems()
        {
            var cutoff = DateTime.Now.AddMilliseconds(-ITEM_REMOVE_TIME);
            int initialCount = killFeedItems.Count;
            killFeedItems.RemoveAll(item => item.Timestamp < cutoff);
            if (killFeedItems.Count != initialCount) Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            int yOffset = 10;
            foreach (var item in killFeedItems)
            {
                DrawKillFeedItem(e.Graphics, item, yOffset);
                yOffset += ITEM_HEIGHT;
            }
        }

        //------------------------ DRAW FEED ITEM ------------------------

        private void DrawKillFeedItem(Graphics g, LogFeedItem item, int yOffset)
        {
            var iconRect = new Rectangle(10, yOffset + (ITEM_HEIGHT - 32) / 2, 32, 32);
            iconRenderer.DrawIcon(g, iconRect, item.Killer, item.Victim, item.Weapon, item.DamageType);
            var textStartX = iconRect.Right + 10;
            var strings = stringFormatter.FormatStrings(item.Killer, item.Victim, item.Weapon, item.DamageType);
            DrawTextLine(g, strings.PrimaryText, textStartX, yOffset + 5, playerFont, strings.PrimaryColors);
            DrawTextLine(g, strings.SecondaryText, textStartX, yOffset + 25, weaponFont, strings.SecondaryColors);
        }

        private void DrawTextLine(Graphics g, string[] textParts, int x, int y, Font font, Color[] colors)
        {
            int currentX = x;
            for (int i = 0; i < textParts.Length; i++)
            {
                using (var brush = new SolidBrush(colors[i]))
                {
                    g.DrawString(textParts[i], font, brush, currentX, y);
                    currentX += (int)g.MeasureString(textParts[i], font).Width;
                }
            }
        }

        //------------------------ CLEAR WINDOW ------------------------

        protected override void ClearWIndow()
        {
            killFeedItems.Clear();
            base.ClearWIndow();
        }

        //------------------------ DISPOSE ------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                playerFont?.Dispose();
                weaponFont?.Dispose();
                removeTimer?.Dispose();
                lockButton?.Dispose();
                feedProcessor?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}