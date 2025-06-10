using System;
using System.IO;
using System.Windows.Forms;
using StarCitizenTracker.Config;

namespace StarCitizenTracker
{
    public class LogMonitorService
    {
        private FileSystemWatcher watcher;
        private Timer pollTimer;
        private string logPath;
        private long lastFileSize = 0;

        public event Action<string> LogLineReceived;

        public void Start()
        {
            logPath = TrackerConfig.Instance.GetLogFilePath();
            SetupLogWatcher();
        }

        private void SetupLogWatcher()
        {
            string dir = Path.GetDirectoryName(logPath);
            string file = Path.GetFileName(logPath);

            if (!Directory.Exists(dir) || !File.Exists(logPath)) return;

            lastFileSize = new FileInfo(logPath).Length;
            watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            watcher.Changed += (s, e) => ReadNewLines();
            watcher.EnableRaisingEvents = true;

            pollTimer = new Timer { Interval = 500 };
            pollTimer.Tick += (s, e) => ReadNewLines();
            pollTimer.Start();
        }

        private void ReadNewLines()
        {
            try
            {
                using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length == lastFileSize) return;

                    fs.Seek(lastFileSize, SeekOrigin.Begin);
                    using (var sr = new StreamReader(fs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                            LogLineReceived?.Invoke(line);
                        lastFileSize = fs.Length;
                    }
                }
            }
            catch { }
        }

        public void Dispose()
        {
            watcher?.Dispose();
            pollTimer?.Dispose();
        }
    }
}