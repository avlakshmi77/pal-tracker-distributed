﻿using System;
using System.Net.Http;
using Allocations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Discovery.Client;
using Steeltoe.Common.Discovery;

namespace AllocationsServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            // Add framework services.
            services.AddMvc();

            services.AddDbContext<AllocationContext>(options => options.UseMySql(Configuration));
            services.AddScoped<IAllocationDataGateway, AllocationDataGateway>();
            services.AddDiscoveryClient(Configuration);
            services.AddSingleton<IProjectClient>(sp =>
            {
               // var httpClient = new HttpClient
               var handler = new DiscoveryHttpClientHandler(sp.GetService<IDiscoveryClient>());
               var httpClient = new HttpClient(handler, false)
                {
                    BaseAddress = new Uri(Configuration.GetValue<string>("REGISTRATION_SERVER_ENDPOINT"))
                };

                return new ProjectClient(httpClient);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvc();
            app.UseDiscoveryClient();
        }
    }
}