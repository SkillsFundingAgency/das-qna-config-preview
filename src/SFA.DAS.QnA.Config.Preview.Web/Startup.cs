using System;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using SFA.DAS.QnA.Config.Preview.Api.Client;
using SFA.DAS.QnA.Config.Preview.Session;
using SFA.DAS.QnA.Config.Preview.Settings;
using SFA.DAS.QnA.Config.Preview.Web.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.IO;

namespace SFA.DAS.QnA.Config.Preview.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const string ServiceName = "SFA.DAS.QnA.Config.Preview";
        private const string Version = "1.0";

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _config = configuration;
        }

        public IWebConfiguration Configuration { get; set; }

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
            services.AddLogging();
            services.AddApplicationInsightsTelemetry();
           
            services.AddTransient<IWebConfiguration, WebConfiguration>();
            services.AddTransient<ITokenService, TokenService>(s => new TokenService(Configuration, _hostingEnvironment));
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ISessionService>( s => new SessionService(s.GetService<IHttpContextAccessor>(), _config["EnvironmentName"]));
            services.AddTransient<IQnaApiClient>(s => new QnaApiClient(Configuration.QnaApiAuthentication.ApiBaseAddress, s.GetService<ITokenService>(), s.GetService<ILogger<QnaApiClient>>()));
          
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("preview", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "QnA API Config Preview", Version = "0.1" });

                if (_hostingEnvironment.IsDevelopment())
                {
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                }
            });
            services.BuildServiceProvider();
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
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/preview/swagger.json", "QnA API Config Preview");
            });
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
