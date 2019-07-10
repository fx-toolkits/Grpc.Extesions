using Grpc.Core;
using Grpc.Extension;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;


namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Configs");
            var configBuilder = new ConfigurationBuilder();
            var conf = configBuilder.SetBasePath(basePath).AddJsonFile("appsettings.json", false, true).Build();

            var innerLogger = new Grpc.Core.Logging.LogLevelFilterLogger(new Grpc.Core.Logging.ConsoleLogger(), Grpc.Core.Logging.LogLevel.Debug);
            GrpcEnvironment.SetLogger(innerLogger);

            //ʹ������ע��
            var services = new ServiceCollection()
                 .AddGrpcMiddleware4Client()
                 // .AddSingleton<ClientInterceptor>(new ClientCallTimeout(10)) // ע��ͻ����м��
                .AddGrpcClient<Greeter.GreeterClient>(conf.GetSection("services:remotes:GreeterServer").Get<RemoteServiceOption>()); // ע��grpc client
            var provider = services.BuildServiceProvider();

            //��������ȡclient
            var client = provider.GetService<Greeter.GreeterClient>();
            var user = "you";

            for (int i = 0; i < 10; i++)
            {
                var reply = client.SayHello(new HelloRequest { Name = user + i.ToString() });
                Console.WriteLine($"Greeting{i.ToString()}: {reply.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
