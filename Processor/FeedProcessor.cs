using System.Collections.Generic;
using System.Drawing;
using StarCitizenTracker.Forms;
using StarCitizenTracker.Services;

namespace StarCitizenTracker.Models
{
    public class FeedProcessor
    {
        private static readonly LogMonitorService logMonitor = new LogMonitorService();

        private MainLogFeed mainLogFeed;
        private SecondaryLogFeed secondaryForm;
        private InventoryLogFeed inventoryLogFeed;
        private DateAndTimeDisplay dateAndTimeDisplay;

        private StartupDebugLines startupDebugLines;

        private MainFeedProcessor mainFeedProcessor;
        private SecondaryFeedProcessor secondaryFeedProcessor;

        private string playerBeingCorpsified = null;
        private List<string> collectedItems = new List<string>();

        public void StartProcessor(MainLogFeed mainForm)
        {
            mainLogFeed = mainForm;

            secondaryForm = new SecondaryLogFeed();
            secondaryForm.Show();

            inventoryLogFeed = new InventoryLogFeed();
            inventoryLogFeed.Show();

            dateAndTimeDisplay = new DateAndTimeDisplay();
            dateAndTimeDisplay.Show();

            logMonitor.LogLineReceived += ProcessLine;
            logMonitor.Start();

            mainFeedProcessor = new MainFeedProcessor();
            mainFeedProcessor.StartProcessor(this);
            secondaryFeedProcessor = new SecondaryFeedProcessor();
            secondaryFeedProcessor.StartProcessor(this);

            startupDebugLines = new StartupDebugLines();
            startupDebugLines.InitializeStartupLines(this);
        }



        public void ProcessLine(string line)
        {
            mainFeedProcessor.ProcessLine(line);
            secondaryFeedProcessor.ProcessLine(line);
        }



        public void UpdateDateAndTime(string dateAndTime)
        {
            dateAndTimeDisplay.UpdateDateAndTime(dateAndTime);
        }

        public void AddMainLine(string killer, string victim, string weapon, string damageType)
        {
            mainLogFeed.AddLine(killer, victim, weapon, damageType);
        }

        public void AddSecondaryLine(string formattedText, Color defaultColor)
        {
            secondaryForm.AddLine(formattedText, defaultColor);
        }

        public void UpdateInventory(string corpseStatus, List<string> items)
        {
            inventoryLogFeed.UpdateInventory(corpseStatus, items);
        }

        public void Dispose()
        {
            secondaryForm?.Dispose();
            inventoryLogFeed?.Dispose();
            dateAndTimeDisplay?.Dispose();
            logMonitor?.Dispose();

            mainFeedProcessor?.Dispose();
            secondaryFeedProcessor?.Dispose();
        }
    }
}
