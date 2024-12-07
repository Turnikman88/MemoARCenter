using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Services;
using Serilog;

namespace MemoARCenter.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config = default)
        {
            CreateLogger();

            services.AddScoped<IDBCreator, DBCreatorService>();
            services.AddScoped<IImageEdit, ImageEditService>();
            services.AddScoped<IVideoEdit, VideoEditService>();
            services.AddScoped<IQRCode, QRCodeService>();
            services.AddLogging();

            return services;
        }

        public static void AddLetsEncrypt(WebApplicationBuilder builder)
        {
            var pfxPassword = Environment.GetEnvironmentVariable("PFX_PASSWORD");
            var env = Environment.GetEnvironmentVariable("MEMOAR_ENV");

            if (env == "Prod")
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(80); // HTTP
                    options.ListenAnyIP(443, listenOptions =>
                    {
                        listenOptions.UseHttps("/etc/letsencrypt/live/memoar.art/certificate.pfx", pfxPassword);
                    });
                });
            }
        }

        private static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Debug()

                // .MinimumLevel.Override("Default", Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();
        }

    }
}
