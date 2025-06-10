namespace StarCitizenTracker.Config
{
    public class TrackerConfig
    {
        //------------------------ CONFIGURATION ------------------------

        private string LogFilePath = @"D:\Program Files\Roberts Space Industries\StarCitizen\LIVE\Game.log";




        //------------------------ END CONFIGURATION ------------------------

        //------------------------ PUBLIC FUNCTIONS ------------------------

        public string GetLogFilePath()
        {
            return LogFilePath;
        }
    }
}
