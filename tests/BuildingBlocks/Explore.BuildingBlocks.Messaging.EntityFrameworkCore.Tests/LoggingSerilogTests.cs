using BuildingBlocks.Logging.Serilog.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class LoggingSerilogTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void UseSerilogLogging_ShouldInitializeLogger_ForConfiguredSinks(bool writeToConsole, bool writeToFile)
    {
        var logFilePath = Path.Combine(Path.GetTempPath(), $"explore-serilog-{Guid.NewGuid():N}.log");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LoggingOptions:ServiceName"] = "Explore.Tests",
                ["LoggingOptions:WriteToConsole"] = writeToConsole.ToString(),
                ["LoggingOptions:WriteToFile"] = writeToFile.ToString(),
                ["LoggingOptions:FilePath"] = logFilePath,
                ["Serilog:MinimumLevel:Default"] = "Information"
            })
            .Build();

        using var host = Host
            .CreateDefaultBuilder()
            .UseSerilogLogging(configuration)
            .Build();
    }
}

