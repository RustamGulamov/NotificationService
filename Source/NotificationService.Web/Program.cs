using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace NotificationService.Web
{
    /// <summary>
    /// Входная точка запуска веб сервиса.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Собирает хост веб приложения.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>Подготовленный строитель веб приложения.</returns>
        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .ConfigureKestrel(options => options.Listen(IPAddress.Any, 5102))
                        .UseIISIntegration()
                        .ConfigureAppConfiguration(
                            (hostingContext, config) => LoadConfiguration(config).AddEnvironmentVariables())
                        .UseStartup<Startup>();
                })
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseNLog();

        /// <summary>
        /// Входная точка программы.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        public static void Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = LoadConfiguration(new ConfigurationBuilder());
            LogManager.Configuration = new NLogLoggingConfiguration(configurationBuilder
                .Build()
                .GetSection("NLog"));

            Logger logger = LogManager.GetCurrentClassLogger();

            try
            {
                logger.Debug("Starting up webhost.");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped service due to exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static IConfigurationBuilder LoadConfiguration(IConfigurationBuilder builder)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return builder
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddJsonFile($"/config/appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables("NOTIFICATION_SERVICE_");
        }
    }
}
