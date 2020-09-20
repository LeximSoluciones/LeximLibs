using System;
using System.Collections.Generic;
using Lexim.Logging.Slack;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.ElasticSearch;
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
            if (!string.IsNullOrEmpty(config.Papertrail?.Server))
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
                            Hostname = config.HostName ?? Environment.MachineName,
                        },
                    },
                    MessageSend = new MessageTransmitterConfig
                    {
                        Protocol = ProtocolType.Tcp,
                        Tcp = new TcpConfig
                        {
                            Server = config.Papertrail.Server,
                            Port = config.Papertrail.Port,
                            Tls = new TlsConfig
                            {
                                Enabled = true,
                            },
                        },
                    },
                    
                };

                configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(config.Papertrail.LogLevel ?? "Trace"), syslogTarget));
            }
            return configuration;
        }

        public static LoggingConfiguration UseElastic(this LoggingConfiguration configuration, LogConfig config)
        {
            if (!string.IsNullOrEmpty(config.Elastic?.Uri))
            {
                if (config.HostName.Contains("-"))
                    throw new InvalidOperationException($"Dash character (-) is not allowed in the Logging.HostName property. Please check your application settings file.");

                var target = new ElasticSearchTarget
                {
                    Name = "Elastic",
                    CloudId = config.Elastic.CloudId,
                    Username = config.Elastic.Username,
                    Password = config.Elastic.Password,
                    RequireAuth = true,
                    Uri = config.Elastic.Uri,
                    Index = $"logs-{config.HostName}",
                    Fields = new List<Field>
                    {
                        new Field { Name = "host.name", Layout = config.HostName },
                        new Field { Name = "application", Layout = config.ApplicationName }
                    },
                    Layout = "${message}"
                };

                configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(config.Elastic.LogLevel ?? "Trace"), target));
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
            if (!string.IsNullOrEmpty(configuration.Slack?.TelemetryKey))
            {
                var slackTarget = new SlackTarget
                {
                    Layout = "${message}",
                    WebHookUrl = "https://hooks.slack.com/services/" + configuration.Slack.TelemetryKey,
                    Channel = configuration.Slack.Channel,
                    Username = configuration.ApplicationName,
                    Compact = true
                };

                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(configuration.Slack.LogLevel ?? "Warn"), slackTarget));
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
                   .UseConsole(logConfig)
                   .UsePaperTrail(logConfig)
                   .UseElastic(logConfig)
                   .UseSlack(logConfig)
                   .Apply();
        }
    }
}
