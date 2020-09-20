namespace Lexim.Logging
{
    public class LogConfig
    {
        public string ApplicationName { get; set; }
        public string HostName { get; set; }

        public string FileLogLevel { get; set; }
        public string ConsoleLogLevel { get; set; }

        public SlackConfig Slack { get; set; } = new SlackConfig();
        public PapertrailConfig Papertrail { get; set; } = new PapertrailConfig();
        public ElasticConfig Elastic { get; set; } = new ElasticConfig();
    }

    public class SlackConfig
    {
        public string TelemetryKey { get; set; }
        public string Channel { get; set; }
        public string LogLevel { get; set; }
    }

    public class PapertrailConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string LogLevel { get; set; }
    }

    public class ElasticConfig
    {
        public string CloudId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Uri { get; set; }
        public string Index { get; set; }
        public string LogLevel { get; set; }
    }
}