using Azure.Storage.Blobs;
using EventPro.API.Services.WatiService.Implementation;
using EventPro.API.Services.WatiService.Interface;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Implementation;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.Storage.Implementation;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.Business.WhatsAppMessagesWebhook.Implementation;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using EventPro.Services.Repository;
using EventPro.Services.Repository.Interface;
using EventPro.Services.UnitOFWorkService;
using EventPro.Services.UnitOFWorkService.Implementation;
using EventPro.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace EventPro.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("Database")["ConnectionString"];
            var redisConnectionString = configuration.GetSection("Database")["RedisCache"];

            // ------------------------------
            // Database Context
            // ------------------------------
            services.AddDbContextFactory<EventProContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<EventProContext>(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<EventProContext>>();
                return factory.CreateDbContext();
            });

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // ------------------------------
            // Repository Pattern
            // ------------------------------
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // ------------------------------
            // Application Services
            // ------------------------------
            services.AddScoped<IWhatsappSendingProviderService, WhatsappSendingProvidersService>();
            services.AddScoped<ITwilioWebhookService, TwilioWebhookService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddSingleton<UrlProtector>();

            // ------------------------------
            // Redis / In-Memory cache
            // ------------------------------
            bool redisConnected = false;
            IConnectionMultiplexer redis = null;

            try
            {
                redis = ConnectionMultiplexer.Connect(redisConnectionString);
                redisConnected = redis.IsConnected;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not connect to Redis at {ConnectionString}. Using fallback providers.", redisConnectionString);
            }

            services.AddDataProtection()
                       .PersistKeysToDbContext<EventProContext>()
                       .SetApplicationName("MyApp");

            if (redisConnected)
            {
                services.AddSingleton<IConnectionMultiplexer>(redis);
                services.AddSingleton<ITicketStore>(new RedisTicketStore(redisConnectionString));

                services.AddSingleton<RedLockFactory>(sp =>
                    RedLockFactory.Create(new List<RedLockMultiplexer> { new RedLockMultiplexer(redis) }));
            }
            else
            {
                services.AddDistributedMemoryCache();
                services.AddSingleton<RedLockFactory>(sp => null);
            }

            services.AddSingleton<IMemoryCacheStoreService>(sp =>
                new MemoryCacheStoreService(sp.GetService<IConnectionMultiplexer>()));

            // ------------------------------
            // Distributed Lock Helper
            // ------------------------------
            services.AddSingleton<DistributedLockHelper>();

            // ------------------------------
            // RabbitMQ
            // ------------------------------
            services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
            {
                HostName = "guppy.rmq6.cloudamqp.com",
                UserName = "nmxrffdi",
                Password = "XpoQK4VehJpnyLo0SVyLx6BuSrbRARoq",
                Port = 5672,
                VirtualHost = "nmxrffdi"
            });

            // Queue / Background messaging services
            services.AddSingleton<IWebHookBulkMessagingQueueProducerService, WebHookBulkMessagingQueueProducerService>();
            services.AddSingleton<IWebHookBulkMessagingQueueConsumerService, WebHookBulkMessagingQueueConsumerService>();
            services.AddSingleton<IWebHookSingleMessagingQueueProducerService, WebHookSingleMessagingQueueProducerService>();
            services.AddSingleton<IWebHookSingleMessagingQueueConsumerService, WebHookSingleMessagingQueueConsumerService>();

            services.AddSingleton<IMessageSendingBulkQueueProducerService, MessageSendingBulkQueueProducerService>();
            services.AddSingleton<IMessageSendingBulkQueueConsumerService, MessageSendingBulkQueueConsumerService>();
            services.AddSingleton<IMessageSendingSingleQueueProducerService, MessageSendingSingleQueueProducerService>();
            services.AddSingleton<IMessageSendingSingleQueueConsumerService, MessageSendingSingleQueueConsumerService>();

            return services;
        }
    }
}
