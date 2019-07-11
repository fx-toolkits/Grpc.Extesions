using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Extension.BaseService;
using Grpc.Extension.Model;
using Grpc.Extension.Common;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Extension.Registers;

namespace Grpc.Extension
{
    public static class ServerExtensions
    {
        internal static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 注入GrpcService
        /// </summary>
        /// <param name="server"></param>
        /// <param name="grpcServices"></param>
        /// <returns></returns>
        public static Server UseGrpcService(this Server server, IEnumerable<IGrpcService> grpcServices)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            grpcServices.ToList().ForEach(grpc => grpc.RegisterMethod(builder));
            server.Services.Add(builder.Build());
            return server;
        }

        /// <summary>
        /// 使用DashBoard(提供基础服务)
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static Server UseDashBoard(this Server server)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            /*
             * callHandlers:
             * 
             * 定义在server中
             * private readonly Dictionary<string, IServerCallHandler> callHandlers = new Dictionary<string, IServerCallHandler>();
             */
            // serverServiceDefinition.GetCallHandlers();
            var callHandlers = server.GetFieldValue<IDictionary>("callHandlers", bindingFlags);
            GrpcServiceExtension.BuildMeta(callHandlers.Item1);
            //注册基础服务
            server.UseGrpcService(new List<IGrpcService> { new CmdService(), new MetaService() });
            return server;
        }

        /// <summary>
        /// 启动并注册服务
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static Server StartAndRegisterService(this Server server)
        {
            server.Start();
            var ipAndPort = server.Ports.FirstOrDefault();
            if (ipAndPort != null)
            {
                MetaModel.StartTime = DateTime.Now;
                MetaModel.Ip = LocalServiceOption.Instance.IP;
                MetaModel.Port = LocalServiceOption.Instance.Port;

                GrpcEnvironment.Logger.ForType<Server>().Info($"server listening {MetaModel.Ip}:{MetaModel.Port}");

                // 注册到Consul
                var consulManager = ServiceProvider.GetService<ServiceRegister>();
                consulManager.RegisterService();
            }
            return server;
        }

        /// <summary>
        /// 停止并反注册服务
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static Server StopAndDeRegisterService(this Server server)
        {
            // 从Consul反注册
            var consulManager = ServiceProvider.GetService<ServiceRegister>();
            consulManager.DeregisterService();
            server.ShutdownAsync().Wait();

            return server;
        }
    }
}
