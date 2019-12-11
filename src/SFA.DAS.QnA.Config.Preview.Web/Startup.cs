using System;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.QnA.Config.Preview.Api.Client;
using SFA.DAS.QnA.Config.Preview.Settings;
using SFA.DAS.QnA.Config.Preview.Web.Extensions;

namespace SFA.DAS.QnA.Config.Preview.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private const string ServiceName = "SFA.DAS.QnA.Config.Preview";
        private const string Version = "1.0";

        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IWebConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration = ConfigurationService.GetConfig(_config["EnvironmentName"], _config["ConfigurationStorageConnectionString"], Version, ServiceName).Result;


            services.AddMvc()
                .AddControllersAsServices()
                .AddSessionStateTempDataProvider()
                .AddFluentValidation(fvc => fvc.RegisterValidatorsFromAssemblyContaining<Startup>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSingleton<Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator, CacheOverrideHtmlGenerator>();

            services.AddAntiforgery(options => options.Cookie = new CookieBuilder() { Name = ".QnA.Config.Preview.AntiForgery", HttpOnly = true });

           
            services.AddSession(opt =>
            {
                opt.IdleTimeout = TimeSpan.FromHours(1);
                opt.Cookie = new CookieBuilder()
                {
                    Name = ".QnA.Config.Preview.Session",
                    HttpOnly = true
                };
            });

            services.AddHealthChecks();

            ConfigureIoc(services);
        }

        private void ConfigureIoc(IServiceCollection services)
        {
            services.AddOptions();
            services.AddApplicationInsightsTelemetry();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IWebConfiguration, WebConfiguration>();
            services.AddTransient<IQnaApiClient>(s => new QnaApiClient(Configuration.QnaApiAuthentication.ApiBaseAddress, s.GetService<ITokenService>(),null));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseRequestLocalization();
            app.UseHealthChecks("/health");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}");
            });
        }
    }
}
