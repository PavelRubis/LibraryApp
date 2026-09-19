using Serilog;
using Serilog.Settings.Configuration;

namespace WebAPI;

public static class ConfigureLogging
{
    private const string SectionName = "Serilog";

    public static void ConfigureBootstrapLogger(this WebApplicationBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration();
        loggerConfiguration.ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions
        {
            SectionName = SectionName
        });
        loggerConfiguration.Enrich.FromLogContext();
        Log.Logger = loggerConfiguration.CreateBootstrapLogger();
    }

    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, configuration) => ConfigureLogger(configuration, builder.Configuration));
    }

    private static void ConfigureLogger(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        loggerConfiguration.ReadFrom.Configuration(configuration, new ConfigurationReaderOptions
        {
            SectionName = SectionName
        });
        loggerConfiguration.Enrich.FromLogContext();
    }
}
