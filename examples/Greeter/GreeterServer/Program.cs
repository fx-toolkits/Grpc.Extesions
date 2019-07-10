using System.IO;
using System.Threading.Tasks;
using Grpc.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreeterServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.StartAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configs"));
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    hostContext.HostingEnvironment.ApplicationName = "GreeterServer";

                    configApp.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configs"));
                    configApp.AddJsonFile("appsettings.json", optional: false);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                })
                .ConfigureServices(services =>
                {
                    services.AddGrpcMiddleware4Srv().BuildInterl4Grpc();
                    services.AddHostedService<GrpcHostService>();
                })
                .UseConsoleLifetime();
    }
}
