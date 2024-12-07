using MemoARCenter.Components;
using MemoARCenter.Extensions;
using MemoARCenter.Helpers.Models.System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
namespace MemoARCenter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Host.UseSerilog();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddControllers();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

            ApplicationServiceExtensions.AddLetsEncrypt(builder);

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = appSettings.MaxBytesAllowedTraffic;
                options.MemoryBufferThreshold = appSettings.MaxBytesAllowedTraffic;
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = appSettings.MaxBytesAllowedTraffic;
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseCors();

            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
