using MemoARCenter.Client.Pages;
using MemoARCenter.Components;
using MemoARCenter.Models;
using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Services;
using Serilog;
namespace MemoARCenter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) 
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Debug()

                // .MinimumLevel.Override("Default", Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddControllers();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddScoped<IDBCreator, DBCreatorService>();
            builder.Services.AddScoped<IImageEdit,ImageEditService>();
            builder.Services.AddScoped<IVideoEdit,VideoEditService>();
            builder.Services.AddScoped<IQRCode, QRCodeService>();
            builder.Services.AddLogging();


            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()  
                          .AllowAnyHeader()  
                          .AllowAnyMethod(); 
                });
            });

            var config = builder.Configuration;
            var pfxPassword = Environment.GetEnvironmentVariable("PFX_PASSWORD");

            builder.Services.Configure<AppSettings>(config.GetSection("AppSettings"));


            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(80); // HTTP
                options.ListenAnyIP(443, listenOptions =>
                {
                    listenOptions.UseHttps("/etc/letsencrypt/live/memoar.art/certificate.pfx", pfxPassword);
                });
            });

            var app = builder.Build();

                       // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
