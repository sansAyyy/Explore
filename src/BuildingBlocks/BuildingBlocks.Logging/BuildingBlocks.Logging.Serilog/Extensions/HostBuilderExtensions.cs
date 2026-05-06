using BuildingBlocks.Logging.Serilog.Enrichers;
using BuildingBlocks.Logging.Serilog.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;

namespace BuildingBlocks.Logging.Serilog.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSerilogLogging(
            this IHostBuilder hostBuilder,
            IConfiguration configuration)
        {
            var loggingOptions = configuration
                .GetSection("LoggingOptions")
                .Get<LoggingOptions>()!;

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.With(new ServiceNameEnricher(loggingOptions.ServiceName));

            loggerConfig.WriteTo.Async(async =>
            {
                if (loggingOptions.WriteToConsole)
                {
                    async.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {ServiceName} {CorrelationId} {Message:lj}{NewLine}{Exception}");
                }

                if (loggingOptions.WriteToFile)
                {
                    async.File(
                        path: loggingOptions.FilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31,
                        formatter: new JsonFormatter());
                }
            });

            Log.Logger = loggerConfig.CreateLogger();

            return hostBuilder.UseSerilog();
        }
    }
}

