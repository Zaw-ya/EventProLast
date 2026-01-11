using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using EventPro.API.Services.WatiService.Implementation;
using EventPro.API.Services.WatiService.Interface;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.Storage.Implementation;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text;
using EventPro.Services.UnitOFWorkService;
using EventPro.Services.UnitOFWorkService.Implementation;
using EventPro.Services.Repository;
using EventPro.Services.Repository.Interface;

namespace EventPro.API
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
            // For SeriLog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                //.WriteTo.MSSqlServer(
                //    Configuration.GetSection("Database")["ConnectionString"],
                //    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions()
                //    {
                //        TableName = "SeriLogAPI",
                //        AutoCreateSqlTable = true,
                //    })
                .CreateLogger();

            services.AddControllers();
            //services.AddMemoryCache();
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowOrigin",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:44302/", "https://app.EventPro.me")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                    });
            });

            services.AddSwaggerGen();

            // Adding Authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

            // MrX7
            //services.AddSingleton<CacheCheck>();
            services.AddScoped<EventProContext>();
            services.AddScoped<IWatiService, WatiService>();
            services.AddSingleton<UrlProtector>();
            services.AddScoped<IWhatsappSendingProviderService, WhatsappSendingProvidersService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            // Configure Redis
            // Get Redis connection string from configuration
            var redisConnectionString = Configuration.GetSection("Database")["RedisCache"];
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<IMemoryCacheStoreService, MemoryCacheStoreService>();
            services.AddSingleton<IMemoryCacheStoreService, MemoryCacheStoreService>();
            services.AddSingleton(

                new BlobServiceClient(Configuration.GetSection("Database")["BlobStorage"])
            );
            services.AddSingleton<IBlobStorage, BlobStorage>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // For SeriLog
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
