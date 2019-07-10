using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FM.GrpcDashboard
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _conf;
        private readonly ILogger _logger;

        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            _env = env;
            _conf = configuration;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            services.AddSingleton<ConsulService>();
            services.AddSingleton<GrpcService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            _logger.LogDebug("FM.GrpcDashboard start...");

            app.Use(async (context, next) =>
            {
                var rq = await FormatRequest(context.Request);
                await next.Invoke();
                var rs = await FormatResponse(context.Response);
                _logger.LogInformation($"{Environment.NewLine}Request: {rq}{Environment.NewLine}Response: {rs}");
            });

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();
            // app.UseBrowserLink();
            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }

        private Task<string> FormatRequest(HttpRequest request)
        {
            return Task.FromResult($"{(request.Method + ": " + request.Host.ToString() + request.PathBase.ToString() + request.Path.ToString())}");
        }

        private Task<string> FormatResponse(HttpResponse response)
        {
            return Task.FromResult($"StatusCode:{response.StatusCode}{Environment.NewLine}Headers:{response.Headers.ToJson()}");
        }
    }
}
