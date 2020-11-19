using ChunkUpload.Interfaces;
using ChunkUpload.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChunkUpload
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
            //services.AddSingleton<IFileStorage>(new LocalStorage("uploads"));            

            // For IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = int.MaxValue;
            });

            // For Kestrel
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });

            // Form's MultipartBodyLengthLimit
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddSingleton<IUploadService, UploadService>();

            services.AddSingleton<IFileStorage>((sp) =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                return new BlobStorage(Configuration.GetConnectionString("Default"), "chunk-uploads", loggerFactory);
                //return new LocalStorage(@"C:\Users\adamo\AppData\Local\uploads");
            });

            services.AddSingleton((sp) =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                return new BlockBlobUploader(Configuration.GetConnectionString("Default"), "chunk-uploads", new AppDataBlockTracker(env, "blocks.json"));
            });

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*
            app.Use(next => context =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });
            */

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
