using DSharpPlus;
using Marco.Data;
using Marco.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using X10D.Hosting.DependencyInjection;

Directory.CreateDirectory("data");

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("data/config.json", true, true))
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddNLog();
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton(new DiscordClient(new DiscordConfiguration
        {
            Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
            LoggerFactory = new NLogLoggerFactory(),
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages | DiscordIntents.MessageContents
        }));

        services.AddHostedSingleton<LoggingService>();

        services.AddSingleton<HttpClient>();
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<MacroCooldownService>();

        services.AddHostedSingleton<MacroListeningService>();
        services.AddHostedSingleton<MacroService>();

        services.AddDbContext<MarcoContext>();

        services.AddHostedSingleton<BotService>();
    })
    .UseConsoleLifetime()
    .RunConsoleAsync();
