using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using StarCitizenTracker.Elements;

namespace StarCitizenTracker.Forms
{
    public class InventoryLogFeed : TrackerWindow
    {
        private string currentPlayer = "No Recent Corpse";
        private List<string> currentInventory = new List<string>();

        private Font headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        private Font inventoryFont = new Font("Segoe UI", 7.5F, FontStyle.Regular);
        private Color headerColor = Color.FromArgb(254, 36, 37);
        private Color textColor = Color.LightGray;

        private bool isLocked = false;
        private Button lockButton;
        private Button closeButton;

        public InventoryLogFeed()
        {
            InitializeFeedComponents();
        }

        //------------------------ INITIALIZE WINDOW ------------------------

        private void InitializeFeedComponents()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Size = new Size(400, 380);
            this.Location = new Point(screen.Width - 400 - 20 - 160, 50);
            this.Paint += OnPaint;
        }

        //------------------------ ADD FEED ITEM ------------------------

        public void UpdateInventory(string playerName, List<string> items)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateInventory(playerName, items)));
                return;
            }

            this.currentPlayer = $"{playerName}'s Inventory";
            this.currentInventory = items;
            this.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(this.BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            int yOffset = 5;
            int xOffset = 5;

            // Draw Header
            using (var brush = new SolidBrush(headerColor))
            {
                g.DrawString(currentPlayer, headerFont, brush, xOffset, yOffset);
            }
            yOffset += headerFont.Height + 5;

            // Draw Inventory List
            using (var brush = new SolidBrush(textColor))
            {
                if (currentInventory.Count == 0 && currentPlayer != "No Recent Corpse")
                {
                    g.DrawString("", inventoryFont, brush, xOffset, yOffset);
                }
                else
                {
                    foreach (var item in currentInventory)
                    {
                        g.DrawString(item, inventoryFont, brush, xOffset, yOffset);
                        yOffset += inventoryFont.Height;

                        // Stop drawing if we run out of vertical space..(temp)
                        if (yOffset > this.Height)
                        {
                            break;
                        }
                    }
                }
            }
        }

        //------------------------ CLEAR WINDOW ------------------------

        protected override void ClearWIndow()
        {
            currentPlayer = "";
            currentInventory.Clear();
            base.ClearWIndow();
        }

        //------------------------ DISPOSE ------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                headerFont?.Dispose();
                inventoryFont?.Dispose();
                lockButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}