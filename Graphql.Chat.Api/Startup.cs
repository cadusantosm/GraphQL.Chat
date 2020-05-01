using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using App.Metrics.Formatters.Prometheus;
using Graphql.Chat.Api.GraphQL;
using Graphql.Chat.Api.Util;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using HotChocolate.AspNetCore.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Graphql.Chat.Api
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            
            services
                .AddControllers()
                .AddControllersAsServices()
                .AddMetrics();

            services.AddHealthChecks()
                .AddRedis(Configuration.GetConnectionString("Redis"))
                .AddNpgSql(Configuration.GetConnectionString("DocumentStore"), name: "DocumentStore");

            services.AddGraphQL(SchemaBuilder.New()
                .AddMutationType<ChatMutation>()
                .AddQueryType<ChatQuery>()
                .AddAllLocalTypes().Create());
            
            var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.ReadFrom(Configuration)
                .OutputMetrics.AsPrometheusPlainText()
                .Build();

            services.AddMetrics(metrics);
            services.AddMetricsEndpoints(endpointsOptions => endpointsOptions.MetricsEndpointOutputFormatter =
                endpointsOptions.MetricsOutputFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First());
            services.AddMetricsReportingHostedService();
            services.AddMetricsTrackingMiddleware(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMetricsEndpoint();
            app.UseMetricsAllMiddleware();

            app.UseWebSockets();

            app.UseGraphQL("/graphql");
            app.UsePlayground(new PlaygroundOptions
            {
                Path = "/graphql-playground",
                QueryPath = "/internal/graphql"
            }).UseGraphQLSubscriptions(new SubscriptionMiddlewareOptions
            {
                Path = "/graphql"
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/healthcheck");
            });
        }
    }
}