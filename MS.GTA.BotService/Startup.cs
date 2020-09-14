// <copyright company="Microsoft Corporation" file="Startup.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using MS.GTA.BotService.Bots;
    using MS.GTA.BotService.Dialogs;
    using MS.GTA.BOTService.BusinessLibrary.Interfaces;
    using MS.GTA.BOTService.BusinessLibrary.Business;
    using MS.GTA.BOTService.Common.Configuration;
    using MS.GTA.BOTService.Data.Interfaces;
    using MS.GTA.BOTService.Data.Query;

    /// <summary>
    /// The base class for starting up the application.
    /// </summary>
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures the dependency injection services used.
        /// </summary>
        /// <param name="services">The services collection to populate.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddMvc();
            services.AddOptions();
            services.AddApplicationInsightsTelemetry();
            

            _ = services.AddAuthentication("Bearer").AddJwtBearer(options =>
            {
                options.Authority = this.Configuration["BearerTokenAuthentication:Authority"];
                options.ClaimsIssuer = this.Configuration["BearerTokenAuthentication:Issuer"];
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidAudiences = this.Configuration["BearerTokenAuthentication:ValidAudiences"].Split(';'),
                };
            });

            services.Configure<CosmosDBConfiguration>(this.Configuration.GetSection("CosmosDB"));

            services.AddSingleton<IConfiguration>(this.Configuration);

            services.AddSingleton<ICosmosQueryClientProvider, CosmosQueryClientProvider>();
            services.AddTransient<IJobApplicationQuery, JobApplicationQuery>();
            services.AddTransient<IJobApplicationManager, JobApplicationManager>();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();
            services.AddSingleton<MainDialog>();

            services.AddTransient<IBot, TeamsBot<MainDialog>>();

            _ = services.AddHttpContextAccessor();
        }

        /// <summary>
        /// Configures the application request pipeline.
        /// </summary>
        /// <param name="app">The application builder to configure.</param>
        /// <param name="env"> The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            try
            {
                app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
                app.UseWebSockets();
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }
    }
}