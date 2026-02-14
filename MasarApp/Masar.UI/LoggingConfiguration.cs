using Serilog;
using System.IO;

namespace Masar.UI;

public static class LoggingConfiguration
{
    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine("logs", "masar-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
