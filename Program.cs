using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

await Host.CreateDefaultBuilder(args)
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureServices((hostContext, services) => {
        services.AddHostedService<ConsoleHostedService>();

        // Configure your settings here
        var configurationManager = new ConfigurationManager();
        Configure(configurationManager);
        IConfigurationRoot configuration = configurationManager;
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(new FileUploader.FileUploader());
    })
    .ConfigureLogging((hostContext, logging) => {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
    })
    .RunConsoleAsync();

/// <summary>
/// Use this method to register your configuration flow. Exactly like in ASP.NET Core.
/// </summary>
static void Configure(ConfigurationManager configurationManager)
{
    string envName = "dev";

    configurationManager
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{envName}.override.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
}