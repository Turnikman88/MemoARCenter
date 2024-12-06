using MemoARCenter.Client.Pages;
using MemoARCenter.Components;
using MemoARCenter.Models;
using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Services;
namespace MemoARCenter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()  // Allow requests from any origin
                          .AllowAnyHeader()  // Allow any headers
                          .AllowAnyMethod(); // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
                });
            });

            var config = builder.Configuration;
            builder.Services.Configure<AppSettings>(config.GetSection("AppSettings"));

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(80); // HTTP
                options.ListenAnyIP(443, listenOptions =>
                {
                    listenOptions.UseHttps("/etc/letsencrypt/live/memoar.art/fullchain.pem", "/etc/letsencrypt/live/memoar.art/privkey.pem");
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
