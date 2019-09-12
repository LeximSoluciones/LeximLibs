using System;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace Lexim.Data
{
    public static class NHibernateExtensions
    {
        public static IServiceCollection AddNHibernate(this IServiceCollection services, IConfiguration configuration, Action<NhibernateConfig> configBuilder = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = configuration.GetSection("Lexim.Data").Get<NhibernateConfig>();

            if (config != null)
            {
                configBuilder?.Invoke(config);

                services.AddSingleton<ISessionFactory>(CreateSessionFactory(config));
                services.AddScoped<ISession>(provider => provider.GetService<ISessionFactory>().OpenSession());
                services.AddScoped<IStatelessSession>(provider => provider.GetService<ISessionFactory>().OpenStatelessSession());
            }
            else
            {
                throw new InvalidOperationException($"Could not find 'Lexim.Data' section in configuration.");
            }

            services.AddScoped<NHibernateDataService>();

            return services;
        }

        private static ISessionFactory CreateSessionFactory(NhibernateConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            return
                Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2008.ConnectionString(config.ConnectionString))
                    .FluentMappings(config)
                    .AutoMappings(config)
                    .Config(config)
                    .BuildSessionFactory();
        }

        private static FluentConfiguration AutoMappings(this FluentConfiguration configuration, NhibernateConfig config)
        {
            if (config.AutoMap && config.MappingsAssembly != null)
            {
                var autoMap = AutoMap.Assembly(config.MappingsAssembly);

                if (config.AutoMappingFilter != null)
                    autoMap = autoMap.Where(config.AutoMappingFilter);

                if (config.UseNumericEnums)
                    autoMap = autoMap.Conventions.Add<EnumConvention>();

                return configuration.Mappings(m => m.AutoMappings.Add(autoMap));
            }

            return configuration;
        }

        private static FluentConfiguration FluentMappings(this FluentConfiguration configuration, NhibernateConfig config)
        {
            if (!config.AutoMap && config.MappingsAssembly != null)
            {
                return configuration.Mappings(mapping => mapping.FluentMappings.AddFromAssembly(config.MappingsAssembly));
            }

            return configuration;
        }

        private static FluentConfiguration Config(this FluentConfiguration configuration, NhibernateConfig config)
        {
            return configuration.ExposeConfiguration(c =>
            {
                if (config.UpdateSchema)
                {
                    new SchemaUpdate(c).Execute(sql =>
                    {
                        if (!string.IsNullOrEmpty(config.ScriptsPath))
                            System.IO.File.AppendAllText(config.ScriptsPath, sql);
                    }, config.CommitUpdates);
                }
                
                //c.SetProperty("command_timeout", "300");
                //c.SetProperty("adonet.batch_size", "1");
            });
        }
    }
}
