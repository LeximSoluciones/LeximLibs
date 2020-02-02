using System;
using Lexim.Logging.Slack;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Syslog;
using NLog.Targets.Syslog.Settings;
using LogLevel = NLog.LogLevel;

namespace Lexim.Logging
{
    public static class LoggingExtensions
    {
        public static void Apply(this LoggingConfiguration config) => LogManager.Configuration = config;

        public static LoggingConfiguration UsePaperTrail(this LoggingConfiguration configuration, LogConfig config)
        {
            if (!string.IsNullOrEmpty(config.PaperTrailServer))
            {
                var syslogTarget = new SyslogTarget
                {
                    Name = "PaperTrail",
                    MessageCreation = new MessageBuilderConfig
                    {
                        Facility = Facility.Local7,
                        Rfc5424 = new Rfc5424Config
                        {
                            AppName = config.ApplicationName,
                            Hostname = config.HostName ?? Environment.MachineName
                        }
                    },
                    MessageSend = new MessageTransmitterConfig
                    {
                        Protocol = ProtocolType.Tcp,
                        Tcp = new TcpConfig
                        {
                            Server = config.PaperTrailServer,
                            Port = config.PaperTrailPort,
                            Tls = new TlsConfig
                            {
                                Enabled = true,
                            }
                        }
                    }
                };

                configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(config.PaperTrailLogLevel ?? "Trace"), syslogTarget));
            }
            return configuration;
        }

        public static LoggingConfiguration UseFile(this LoggingConfiguration config, LogConfig fileTelemetryConfig)
        {
            if (!string.IsNullOrEmpty(fileTelemetryConfig.FileLogLevel))
            {
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(fileTelemetryConfig.FileLogLevel), new FileTarget() { Name = "File", Layout = "${longdate} ${logger} ${message}", FileName = "${basedir}/logs/${shortdate}.log" }));
            }

            return config;
        }

        public static LoggingConfiguration UseSlack(this LoggingConfiguration config, LogConfig configuration)
        {
            if (!string.IsNullOrEmpty(configuration.SlackTelemetryKey))
            {
                var slackTarget = new SlackTarget
                {
                    Layout = "${message}",
                    WebHookUrl = "https://hooks.slack.com/services/" + configuration.SlackTelemetryKey,
                    Channel = configuration.SlackChannel,
                    Username = configuration.ApplicationName,
                    Compact = true
                };

                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(configuration.SlackLogLevel ?? "Warn"), slackTarget));
            }

            return config;
        }

        public static LoggingConfiguration UseConsole(this LoggingConfiguration config, LogConfig configuration)
        {
            if (!string.IsNullOrEmpty(configuration.ConsoleLogLevel))
            {
                var target = new ConsoleTarget("Console");
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(configuration.ConsoleLogLevel ?? "Trace"), target));
            }

            return config;
        }

        public static void Apply(this LogConfig logConfig)
        {
            new LoggingConfiguration()
                   .UseFile(logConfig)
                   .UsePaperTrail(logConfig)
                   .UseSlack(logConfig)
                   .UseConsole(logConfig)
                   .Apply();
        }
    }
}
