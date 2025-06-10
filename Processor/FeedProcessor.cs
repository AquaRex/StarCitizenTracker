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


        //------------------------ START PROCESSOR ------------------------

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

        //------------------------ PROCESS CONSOLE LINE ------------------------

        public void ProcessLine(string line)
        {
            mainFeedProcessor.ProcessLine(line);
            secondaryFeedProcessor.ProcessLine(line);
        }

        //------------------------ UPDATE DATE AND TIME ------------------------

        public void UpdateDateAndTime(string dateAndTime)
        {
            dateAndTimeDisplay.UpdateDateAndTime(dateAndTime);
        }

        //------------------------ MAIN KILL FEED LINE ------------------------

        public void AddMainLine(string killer, string victim, string weapon, string damageType)
        {
            mainLogFeed.AddLine(killer, victim, weapon, damageType);
        }

        //------------------------ SECONDARY FEED LINE ------------------------

        public void AddSecondaryLine(string formattedText, Color defaultColor)
        {
            secondaryForm.AddLine(formattedText, defaultColor);
        }

        //------------------------ UPDATE INVENTORY ------------------------

        public void UpdateInventory(string corpseStatus, List<string> items)
        {
            inventoryLogFeed.UpdateInventory(corpseStatus, items);
        }

        //------------------------ DISPOSE ------------------------

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
