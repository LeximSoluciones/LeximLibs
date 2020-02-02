namespace Lexim.Logging
{
    public class LogConfig
    {
        public string SlackTelemetryKey { get; set; }
        public string SlackChannel { get; set; }
        public string SlackLogLevel { get; set; }
        public string PaperTrailServer { get; set; }
        public int PaperTrailPort { get; set; }
        public string PaperTrailLogLevel { get; set; }
        public string FileLogLevel { get; set; }
        public string ApplicationName { get; set; }
        public string HostName { get; set; }
        public string ConsoleLogLevel { get; set; }
    }
}