using System;
using Lexim.Logging.Slack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Syslog;
using NLog.Targets.Syslog.Settings;
using LogLevel = NLog.LogLevel;

namespace Lexim.Logging
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddLeximLogging(this IServiceCollection services, string applicationName, IConfiguration configuration, Action<LogConfig> configBuilder = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (applicationName == null) throw new ArgumentNullException(nameof(applicationName));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            var logConfig = configuration.GetSection("Logging").Get<LogConfig>();

            if (logConfig != null)
            {
                configBuilder?.Invoke(logConfig);

                logConfig.ApplicationName = applicationName;

                new LoggingConfiguration()
                    .UseFile(logConfig)
                    .UsePaperTrail(logConfig)
                    .UseSlack(logConfig)
                    .UseConsole(logConfig)
                    .Apply();
            }

            services.AddNLogProvider();

            return services;
        }

        private static void AddNLogProvider(this IServiceCollection services)
        {
            //services.AddSingleton<ILoggerFactory, LoggerFactory>();
            //services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            });
        }

        private static void Apply(this LoggingConfiguration config) => LogManager.Configuration = config;

        //public static LoggingConfiguration WithRule(this LoggingConfiguration config, LoggingRule rule) => config.With(x => x.LoggingRules.Add(rule));

        private static LoggingConfiguration UsePaperTrail(this LoggingConfiguration config, LogConfig configuration)
        {
            if (!string.IsNullOrEmpty(configuration.PaperTrailServer))
            {
                var syslogTarget = new SyslogTarget
                {
                    Name = "PaperTrail",
                    MessageCreation = new MessageBuilderConfig
                    {
                        Facility = Facility.Local7,
                        Rfc5424 = new Rfc5424Config
                        {
                            AppName = configuration.ApplicationName,
                        }
                    },
                    MessageSend = new MessageTransmitterConfig
                    {
                        Protocol = ProtocolType.Tcp,
                        Tcp = new TcpConfig
                        {
                            Server = configuration.PaperTrailServer,
                            Port = configuration.PaperTrailPort,
                            Tls = new TlsConfig
                            {
                                Enabled = true,
                            }
                        }
                    }
                };

                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(configuration.PaperTrailLogLevel ?? "Trace"), syslogTarget));
            }
            return config;
        }

        private static LoggingConfiguration UseFile(this LoggingConfiguration config, LogConfig fileTelemetryConfig)
        {
            if (!string.IsNullOrEmpty(fileTelemetryConfig.FileLogLevel))
            {
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(fileTelemetryConfig.FileLogLevel), new FileTarget() { Name = "File", Layout = "${longdate} ${logger} ${message}", FileName = "${basedir}/logs/${shortdate}.log" }));
            }

            return config;
        }

        private static LoggingConfiguration UseSlack(this LoggingConfiguration config, LogConfig configuration)
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

        private static LoggingConfiguration UseConsole(this LoggingConfiguration config, LogConfig configuration)
        {
            if (!string.IsNullOrEmpty(configuration.ConsoleLogLevel))
            {
                var target = new ConsoleTarget("Console");
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.FromString(configuration.ConsoleLogLevel ?? "Trace"), target));
            }

            return config;
        }
    }
}
