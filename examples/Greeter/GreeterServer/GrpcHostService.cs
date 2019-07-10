﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreeterServer.Impls;
using Grpc.Core;
using Grpc.Extension;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Internal;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GreeterServer
{
    public class GrpcHostService : IHostedService
    {
        private readonly IConfiguration _conf;
        private readonly IEnumerable<ServerInterceptor> _serverInterceptors;
        private Server _server;

        public GrpcHostService(IConfiguration conf, IEnumerable<ServerInterceptor> serverInterceptors)
        {
            this._conf = conf;
            this._serverInterceptors = serverInterceptors;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //构建Server
            var serverBuilder = new ServerBuilder();
            var serverOptions = _conf.GetSection("services:GreeterServer").Get<LocalServiceOption>();
            _server = serverBuilder.UseGrpcOptions(serverOptions)
                .UseInterceptor(_serverInterceptors) // 使用中间件
                .UseGrpcService(Greeter.BindService(new GreeterImpl()))
                .UseLogger(log => // 使用日志
                {
                    log.LoggerMonitor = info => Console.WriteLine(info);
                    log.LoggerError = exception => Console.WriteLine(exception);
                })
                .Build();

            var innerLogger = new Grpc.Core.Logging.LogLevelFilterLogger(new Grpc.Core.Logging.ConsoleLogger(), Grpc.Core.Logging.LogLevel.Debug);
            GrpcEnvironment.SetLogger(innerLogger);

            _server.UseDashBoard() // 使用DashBoard,需要使用FM.GrpcDashboard网站
               .StartAndRegisterService(); // 启动服务并注册到consul

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.StopAndDeRegisterService(); // 停止服务并从consul反注册
            return Task.CompletedTask;
        }
    }
}
