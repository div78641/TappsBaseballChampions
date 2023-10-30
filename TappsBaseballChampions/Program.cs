using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
ILogger logger = services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Start Program");
    services.GetRequiredService<ConsoleApplication>().Run(args);
    logger.LogInformation("End Program");
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

static IHostBuilder CreateHostBuilder(string[] strings)
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<ConsoleApplication>();
        })
        .ConfigureAppConfiguration(app =>
        {
            app.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        });
}