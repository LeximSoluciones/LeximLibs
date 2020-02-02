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
    public static class AspNetCoreLoggingExtensions
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

                logConfig.Apply();
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
    }
}
