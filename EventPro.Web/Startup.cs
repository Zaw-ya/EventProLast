using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

using EventPro.Business.EventService;
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
using EventPro.DAL.Common;
using EventPro.DAL.Common.Interfaces;
using EventPro.DAL.Models;
using EventPro.Services.AuditLogService.implementation;
using EventPro.Services.AuditLogService.Interface;
using EventPro.Services.NotificationService.Implementation;
using EventPro.Services.Repository;
using EventPro.Services.Repository.Interface;
using EventPro.Services.TwilioService;
using EventPro.Services.TwilioService.Interface;
using EventPro.Services.UnitOFWorkService;
using EventPro.Services.UnitOFWorkService.Implementation;
using EventPro.Services.UnitOFWorkService.Interface;
using EventPro.Services.WatiService.Implementation;
using EventPro.Services.WatiService.Interface;
using EventPro.Web.Controllers.Admin;
using EventPro.Web.CustomMiddleware;
using EventPro.Web.Filters;
using EventPro.Web.Seeds;
using EventPro.Web.Services;
using EventPro.Web.Services.DefaultWhatsappService.Implementation;
using EventPro.Web.Services.DefaultWhatsappService.Interface;
using EventPro.Web.Services.Interface;

using FirebaseAdmin;

using Google.Apis.Auth.OAuth2;

using Hangfire;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

using Serilog;

using StackExchange.Redis;

namespace EventPro.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetSection("Database")["ConnectionString"];
            var redisConnectionString = Configuration.GetSection("Database")["RedisCache"];

            // ------------------------------
            // DbContext setup
            // ------------------------------
            services.AddDbContextFactory<EventProContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<EventProContext>(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<EventProContext>>();
                return factory.CreateDbContext();
            });
            // HttpContextAccessor accessed by IUnitOFWorkDefaultWhatsappService
            services.AddHttpContextAccessor();

            // used in any service needs IHttpClientFactory
            services.AddHttpClient();


            // ------------------------------
            // Redis / In-Memory cache
            // ------------------------------
            bool redisConnected = false;
            IConnectionMultiplexer redis = null;

            // Attempt Redis connection
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
                // Use In-Memory fallback
                services.AddDistributedMemoryCache();
                //services.AddDataProtection().SetApplicationName("EventPro.cc");

                // Dummy RedLockFactory to avoid DI errors
                services.AddSingleton<RedLockFactory>(sp => null);
            }

            // ------------------------------
            // Authentication
            // ------------------------------
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.Cookie.Name = ".EventPro.Auth";
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.None;
                        options.LoginPath = "/Login";
                        options.AccessDeniedPath = "/AccessDenied";
                        options.ExpireTimeSpan = TimeSpan.FromHours(2);
                        options.SlidingExpiration = true;

                        // If redis ticket found
                        options.Events.OnSigningIn = context =>
                        {
                            var ticketStore = context.HttpContext.RequestServices.GetService<ITicketStore>();
                            if (ticketStore != null)
                                options.SessionStore = ticketStore;

                            return Task.CompletedTask;
                        };
                    });

            //services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            //{
            //    var sp = services.BuildServiceProvider();
            //    var dataProtectionProvider = sp.GetRequiredService<IDataProtectionProvider>();
            //    var ticketStore = sp.GetService<ITicketStore>();

            //    if (ticketStore != null)
            //        options.SessionStore = ticketStore;

            //    options.TicketDataFormat = new SecureDataFormat<AuthenticationTicket>(
            //        new TicketSerializer(),
            //        dataProtectionProvider.CreateProtector(
            //            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            //            CookieAuthenticationDefaults.AuthenticationScheme, "v2"));
            //});

            // ------------------------------
            // Forward headers
            // ------------------------------
            services.Configure<ForwardedHeadersOptions>(options =>
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

            // ------------------------------
            // Response compression
            // ------------------------------
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<BrotliCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);
            services.Configure<GzipCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);

            // ------------------------------
            // MVC
            // ------------------------------
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            // ------------------------------
            // DI for Services
            // ------------------------------

            // Business / Core services
            services.AddScoped<IWatiService, WatiService>();
            services.AddScoped<ITwilioService, TwilioService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<INotificationTokenService, NotificationTokenService>();

            // Repository & Unit of Work
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUnitOFWorkService, UnitOFWorkService>();

            // Messaging / WhatsApp / Twilio related
            services.AddScoped<IWhatsappSendingProviderService, WhatsappSendingProvidersService>();
            services.AddScoped<ITwilioWebhookService, TwilioWebhookService>();
            services.AddScoped<IFirbaseAPI, FirbaseAPI>();           // Firebase
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IEmailSender, NotificationService>();

            // Queue / Background messaging services
            services.AddSingleton<IWebHookBulkMessagingQueueProducerService, WebHookBulkMessagingQueueProducerService>();
            services.AddSingleton<IWebHookBulkMessagingQueueConsumerService, WebHookBulkMessagingQueueConsumerService>();
            services.AddSingleton<IWebHookSingleMessagingQueueProducerService, WebHookSingleMessagingQueueProducerService>();
            services.AddSingleton<IWebHookSingleMessagingQueueConsumerService, WebHookSingleMessagingQueueConsumerService>();

            services.AddSingleton<IMessageSendingBulkQueueProducerService, MessageSendingBulkQueueProducerService>();
            services.AddSingleton<IMessageSendingBulkQueueConsumerService, MessageSendingBulkQueueConsumerService>();
            services.AddSingleton<IMessageSendingSingleQueueProducerService, MessageSendingSingleQueueProducerService>();
            services.AddSingleton<IMessageSendingSingleQueueConsumerService, MessageSendingSingleQueueConsumerService>();

            // ------------------------------
            // Hosted Services (Background Workers)
            // ------------------------------
            services.AddHostedService<SingleMessagingConsumerBackgroundService>();
            services.AddHostedService<BulkMessagingConsumerBackgroundService>();


            // Singletons
            services.AddSingleton<IUnitOFWorkDefaultWhatsappService, UnitOFWorkDefaultWhatsappService>();
            services.AddSingleton<IMemoryCacheStoreService>(sp =>
                new MemoryCacheStoreService(sp.GetService<IConnectionMultiplexer>()));

            // Helpers / Filters / Middleware-related
            services.AddSingleton<UrlProtector>();
            services.AddScoped<ForwardToPrimaryFilter>();
            services.AddScoped<TwilioRoutingRuleForVms>();

            // ------------------------------
            // Hangfire
            // ------------------------------
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            // ------------------------------
            // RabbitMQ
            // ------------------------------
            services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
            {
                HostName = "guppy.rmq6.cloudamqp.com", // HostName = DNS 
                UserName = "nmxrffdi",
                Password = "XpoQK4VehJpnyLo0SVyLx6BuSrbRARoq",
                Port = 5672,
                VirtualHost = "nmxrffdi"
            });

            // ------------------------------
            // BlobStorage (try-catch)
            // ------------------------------
            var blobConnection = Configuration.GetSection("Database")["BlobStorage"];
            if (!string.IsNullOrWhiteSpace(blobConnection))
            {
                try
                {
                    services.AddSingleton(new BlobServiceClient(blobConnection));
                    services.AddSingleton<IBlobStorage, BlobStorage>();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "BlobStorage is disabled due to invalid connection string");
                    services.AddSingleton<IBlobStorage,DummyBlobStorage>();
                }
            }
            else
            {
                Log.Warning("BlobStorage connection string is empty. Blob features disabled.");
            }

            // ------------------------------
            // Firebase
            // ------------------------------
            var firebasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myinvite-1b0ca-firebase-adminsdk-yp15k-b2881aed59.json");
            if (File.Exists(firebasePath))
            {
                FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile(firebasePath) });
            }
            else
            {
                Log.Warning("Firebase admin SDK file not found. Features disabled.");
            }

            // ------------------------------
            // Distributed Lock Helper
            // ------------------------------
            services.AddSingleton<DistributedLockHelper>();
        }

        // Here i pass the service provider not in the old version we pass INotificationTokenService drictly and it was not supported
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                       IUnitOFWorkService unitOFWorkService, INotificationTokenService serviceProvider, IConnectionFactory rabbitFactory)
        {
            // Test rabbitmq working 
            try
            {
                using var connection = rabbitFactory.CreateConnection();
                using var channel = connection.CreateModel();

                #region Testing for succsess connection
                //string queueName = "test_queue";
                //string message = "Hello RabbitMQ!";

                //// اتأكد ان الـ queue موجودة
                //channel.QueueDeclare(queue: queueName,
                //                     durable: false,
                //                     exclusive: false,
                //                     autoDelete: false,
                //                     arguments: null);

                //// حوّل الرسالة لـ byte array
                //var body = System.Text.Encoding.UTF8.GetBytes(message);

                //// ابعت الرسالة
                //channel.BasicPublish(exchange: "",
                //                     routingKey: queueName,
                //                     basicProperties: null,
                //                     body: body); 
                #endregion

                Log.Information("RabbitMQ connection successful! Host: {Host}", rabbitFactory.VirtualHost);
            }
            catch (BrokerUnreachableException ex)
            {
                Log.Error(ex, "RabbitMQ is not reachable at {Host}. Please check if the service is running.", rabbitFactory.VirtualHost);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while connecting to RabbitMQ.");
            }

            // 
            Seeding.SeedAll(Configuration);
            app.UseResponseCompression();
            app.UseExceptionHandler("/Home/Error");
            app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");
            app.UseMiddleware<StaticFileCacheMiddleware>();
            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = null
            });

            app.Use((context, next) => { context.Request.Scheme = "https"; return next(); });

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseHangfireDashboard("/Admin/Notifications", new DashboardOptions
            {
                Authorization = new[] { new NotifyAuthorizationFilter() }
            });

            RecurringJob.AddOrUpdate<AppSettingsController>(
                "CheckTwilioBalances",
                x => x.CheckAllAccountsAsync(),
                "0 */6 * * *",
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            // مثال لو فيه notification jobs:
            // RecurringJob.AddOrUpdate("NotifyBeforeEvent", () => notifyService.SendNotifyTokensAsync(), "*/5 * * * *");
        }

    }
}
