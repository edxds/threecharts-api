using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EFCore.NamingConventions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Services;
using ThreeChartsAPI.Services.LastFm;
using ThreeChartsAPI.Services.Onboarding;

namespace ThreeChartsAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.SetIsOriginAllowed(_ => true);
                    builder.AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddDbContext<ThreeChartsContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("ThreeChartsDB"));
                options.UseSnakeCaseNamingConvention();
            });

            services.AddHttpClient("lastFm", c =>
            {
                c.BaseAddress = new Uri("http://ws.audioscrobbler.com/2.0/");
            });

            services.AddSingleton<ILastFmDeserializer, LastFmJsonDeserializer>();

            services.AddSingleton(
                new HttpLastFmService.Settings(
                    Configuration.GetValue<string>("LastFmApiKey"),
                    Configuration.GetValue<string>("LastFmApiSecret")
                )
            );

            services.AddSingleton<ILastFmService, HttpLastFmService>();

            // Services that use DbContext should be scoped
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChartWeekService, ChartWeekService>();
            services.AddScoped<IOnboardingService, OnboardingService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "threecharts_identity";
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = redirectContext =>
                        {
                            redirectContext.HttpContext.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                MinimumSameSitePolicy = SameSiteMode.None,
            });

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
