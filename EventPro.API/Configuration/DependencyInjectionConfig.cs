using Azure.Storage.Blobs;
using EventPro.API.Services.WatiService.Implementation;
using EventPro.API.Services.WatiService.Interface;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.Storage.Implementation;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Services.Repository;
using EventPro.Services.Repository.Interface;
using EventPro.Services.UnitOFWorkService;
using EventPro.Services.UnitOFWorkService.Implementation;
using EventPro.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EventPro.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddSingleton<CacheCheck>();
            //services.AddMemoryCache();

            // Database Context
            services.AddScoped<EventProContext>();

            // Repository Pattern
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // Application Services
            services.AddScoped<IWatiService, WatiService>();
            services.AddScoped<IWhatsappSendingProviderService, WhatsappSendingProvidersService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddSingleton<UrlProtector>();

            // Configure Redis
            // Get Redis connection string from configuration
            var redisConnectionString = configuration.GetSection("Database")["RedisCache"];
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<IMemoryCacheStoreService, MemoryCacheStoreService>();

            // Configure Azure Blob Storage (Not used - using Cloudinary instead)
            //services.AddSingleton(new BlobServiceClient(configuration.GetSection("Database")["BlobStorage"]));
            //services.AddSingleton<IBlobStorage, BlobStorage>();

            return services;
        }
    }
}
