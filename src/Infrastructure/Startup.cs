using Core.Services;
using Infrastructure.Common;
using Infrastructure.EnvVarService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Infrastructure;

public static class Startup
{
    public static IHostBuilder ConfigureHost(this IHostBuilder hostBuilder, string[]? args) =>
        hostBuilder
            .ConfigureDefaults(args)
            .ConfigureConfiguration()
            .ConfigureLogging()
            .ConfigureServices();

    private static IHostBuilder ConfigureConfiguration(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureAppConfiguration((context, builder) => { builder.AddJsonFile("appsettings.json", true, true); });
    
    private static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureServices((context, collection) =>
            {
                collection
                    .AddOptions<EnvVarSettings>()
                    .BindConfiguration(nameof(EnvVarSettings));
                
                collection
                    .AddTransient<ICommentService, CommentService>()
                    .AddTransient<IEnvVarService, EnvVarService.EnvVarService>();
            });

    private static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureLogging((_, builder) => { builder.ClearProviders(); })
            .UseSerilog((_, _, serilogConfig) =>
            {
                var localAppData = Environment
                    .GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(localAppData, "SMS");
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }
                
#if DEBUG
                serilogConfig.MinimumLevel.Debug();
#else
                serilogConfig.MinimumLevel.Information();
#endif
                
                serilogConfig
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)

                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)

                    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)

                    .WriteTo.Console()
                    .WriteTo.File(
                        new CompactJsonFormatter(),
                        Path.Combine(appFolder, "logs-sms-wpf-app-.json"),
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 5);
            });
}