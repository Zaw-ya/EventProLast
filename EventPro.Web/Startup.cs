using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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
using EventPro.Kernal.StaticFiles;
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

using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

using Serilog;
using Serilog.Events;

using StackExchange.Redis;


namespace EventPro.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Get Redis connection string from configuration
            var redisConnectionString = Configuration.GetSection("Database")["RedisCache"];
            var connectionString = Configuration.GetSection("Database")["ConnectionString"];

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // For configurationHelper class.
            ConfigurationHelper.Initialize(Configuration);
            // For SeriLog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
            //.WriteTo.MSSqlServer(
            //    Configuration.GetSection("Database")["ConnectionString"],
            //    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions()
            //    {
            //        TableName = "SeriLog",
            //        AutoCreateSqlTable = true,

            //    })
                .CreateLogger();
            // Keep only the factory registration:
            services.AddDbContextFactory<EventProContext>(options =>
                options.UseSqlServer(connectionString));

            // If you still need scoped DbContext for other services, add:
            services.AddScoped<EventProContext>(provider =>
            {
                var factory = provider.GetRequiredService<IDbContextFactory<EventProContext>>();
                return factory.CreateDbContext();
            });

            IConnectionMultiplexer redis = null;
            bool redisConnected = false;
            try
            {
                redis = ConnectionMultiplexer.Connect(redisConnectionString);
                redisConnected = redis.IsConnected;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not connect to Redis at {ConnectionString}. Using fallback providers.", redisConnectionString);
            }

            if (redisConnected)
            {
                services.AddSingleton<IConnectionMultiplexer>(redis);
                services.AddSingleton<ITicketStore>(new RedisTicketStore(redisConnectionString));
                services.AddDataProtection()
                        .PersistKeysToFileSystem(new DirectoryInfo(@"c:\keys\"))
                        .SetApplicationName("EventPro");

                services.AddSingleton<RedLockFactory>(sp =>
                {
                    return RedLockFactory.Create(new List<RedLockMultiplexer> { new RedLockMultiplexer(redis) });
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
                // Fallback: If Redis is down, we don't set a SessionStore, 
                // ASP.NET will use the default cookie-based store.
                services.AddDataProtection()
                    .SetApplicationName("EventPro.cc");

                // Mock RedLock setup if needed, or just let users know
            }


            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
     {
         options.Cookie.Name = ".MyApp.Auth";
         options.Cookie.HttpOnly = true;
         options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
         options.Cookie.SameSite = SameSiteMode.None;
         options.LoginPath = "/Login";
         options.AccessDeniedPath = "/AccessDenied";
         options.ExpireTimeSpan = TimeSpan.FromHours(2);
         options.SlidingExpiration = true;
     });

            services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                var serviceProvider = services.BuildServiceProvider();

                var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
                var ticketStore = serviceProvider.GetService<ITicketStore>();

                if (ticketStore != null)
                {
                    options.SessionStore = ticketStore;
                }

                options.TicketDataFormat = new SecureDataFormat<AuthenticationTicket>(
                    new TicketSerializer(),
                    dataProtectionProvider.CreateProtector(
                        "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        "v2"));

            });


            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<BrotliCompressionProviderOptions>(opts =>
            {
                opts.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(opts =>
            {
                opts.Level = CompressionLevel.Fastest;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<UrlProtector>();
            services.AddScoped<ForwardToPrimaryFilter>();
            services.AddScoped<TwilioRoutingRuleForVms>();
            services.AddHttpClient();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            //  services.AddScoped<EventProContext>();
            services.AddScoped<IWatiService, WatiService>();
            services.AddScoped<ITwilioService, TwilioService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOFWorkService, UnitOFWorkService>();
            services.AddScoped<IFirbaseAPI, FirbaseAPI>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ITwilioWebhookService, TwilioWebhookService>();
            services.AddScoped<IWhatsappSendingProviderService, WhatsappSendingProvidersService>();
            services.AddSingleton<IUnitOFWorkDefaultWhatsappService, UnitOFWorkDefaultWhatsappService>();
            services.AddSingleton<IMemoryCacheStoreService>(sp =>
                new MemoryCacheStoreService(sp.GetService<IConnectionMultiplexer>()));

            // Helpers / Filters / Middleware-related
            services.AddScoped<UrlProtector>();
            services.AddScoped<ForwardToPrimaryFilter>();
            services.AddScoped<TwilioRoutingRuleForVms>();

            // ------------------------------
            // Hangfire
            // ------------------------------
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();
            services.AddScoped<INotificationTokenService, NotificationTokenService>();
            services.AddDirectoryBrowser();

            services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = Protocols.DefaultProtocol.DefaultPort,
                VirtualHost = "/",
            }
            );
            services.AddSingleton<IWebHookBulkMessagingQueueProducerService, WebHookBulkMessagingQueueProducerService>();
            services.AddSingleton<IWebHookBulkMessagingQueueConsumerService, WebHookBulkMessagingQueueConsumerService>();
            services.AddSingleton<IWebHookSingleMessagingQueueProducerService, WebHookSingleMessagingQueueProducerService>();
            services.AddSingleton<IWebHookSingleMessagingQueueConsumerService, WebHookSingleMessagingQueueConsumerService>();
            services.AddSingleton<IMessageSendingBulkQueueProducerService, MessageSendingBulkQueueProducerService>();
            services.AddSingleton<IMessageSendingBulkQueueConsumerService, MessageSendingBulkQueueConsumerService>();
            services.AddSingleton<IMessageSendingSingleQueueProducerService, MessageSendingSingleQueueProducerService>();
            services.AddSingleton<IMessageSendingSingleQueueConsumerService, MessageSendingSingleQueueConsumerService>();
            services.AddHostedService<BulkMessagingConsumerBackgroundService>();
            services.AddHostedService<SingleMessagingConsumerBackgroundService>();
            services.AddHostedService<MessageSendingBulkConsumerBackgroundService>();
            services.AddHostedService<MessageSendingSingleConsumerBackgroundService>();

            // ------------------------------
            // BlobStorage (try-catch)
            // ------------------------------
            var blobConnection = Configuration.GetSection("Database")["BlobStorage"];
            if (!string.IsNullOrWhiteSpace(blobConnection))
            {
                try
                {
                    services.AddSingleton<IBlobStorage, BlobStorage>();
                    services.AddSingleton(new BlobServiceClient(blobConnection));
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "BlobStorage is disabled due to invalid connection string");
                    services.AddSingleton<IBlobStorage, DummyBlobStorage>(); // fallback

                }
            }
            else
            {
                Log.Warning("BlobStorage connection string is empty. Blob features disabled.");
            }

            var firebasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myinvite-1b0ca-firebase-adminsdk-yp15k-b2881aed59.json");
            if (File.Exists(firebasePath))
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(firebasePath),
                });
            }
            else
            {
                Log.Warning("Firebase admin SDK file not found at {Path}. Firebase features will be disabled.", firebasePath);
            }

            services.AddScoped<IEmailSender, NotificationService>();

            services.AddSingleton<IMemoryCacheStoreService>(sp =>
            {
                var redis = sp.GetService<IConnectionMultiplexer>();
                return new MemoryCacheStoreService(redis);
            });
            services.AddHttpContextAccessor();

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "X-CSRF-TOKEN"; // consistent name across instances
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.Lax; // or Strict if your app allows
                options.Cookie.HttpOnly = true;
            });

            services.AddSingleton<DistributedLockHelper>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                                 IUnitOFWorkService unitOFWorkService, INotificationTokenService notifyService)
        {
            IUnitOFWorkService _unitOfWorkService = unitOFWorkService;
            Seeding.SeedAll(Configuration);
            app.UseResponseCompression();

            // Global exception handler - catches all unhandled exceptions
            app.UseExceptionHandler("/Home/Error");

            // Status code pages - handles 404, 403, etc.
            app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");

            app.UseMiddleware<StaticFileCacheMiddleware>();
            app.UseStaticFiles();

            // app.UseAutomaticFreeupSpace(_unitOfWorkService.FreeupSpace);
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = null, // Accept infinite number of proxies if needed
                KnownProxies = { },  // Or specify the IP of the App Gateway (recommended)
                KnownNetworks = { }  // Leave empty to allow unknown networks (not recommended in prod)
            });

            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });
            // For SeriLog
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication(); // Enables authentication middleware
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                /* 
                // BLOB STORAGE FALLBACK (Disabled for Local File System Usage)
                endpoints.MapGet("upload/{**fileName}", async (IBlobStorage blobStorage, string fileName) =>
                {
                    fileName = Configuration.GetSection("Uploads")["environment"] + "/" + fileName;
                    try
                    {
                        FileResponse fileResponse = await blobStorage.DownloadAsync(fileName, cancellationToken: default);

                        var provider = new FileExtensionContentTypeProvider();
                        if (!provider.TryGetContentType(fileName, out string contentType))
                        {
                            contentType = MediaTypeNames.Application.Octet;
                        }
                        if (contentType == "text/calendar")
                            return Results.File(fileResponse.Stream, contentType, "calendar Reminder.ics");

                        return Results.File(fileResponse.Stream, contentType);
                    }
                    catch
                    {
                        return Results.NotFound();
                    }
                });
                */
            });

            app.UseHangfireDashboard("/Admin/Notifications", new DashboardOptions
            {
                Authorization = new[] { new NotifyAuthorizationFilter() }
            });
            RecurringJobOptions jobOptions = new()
            {
                TimeZone = TimeZoneInfo.Local
            };

            RecurringJob.AddOrUpdate<AppSettingsController>(
          "CheckTwilioBalances",
          x => x.CheckAllAccountsAsync(),
          "0 */6 * * *",
          jobOptions);



            //       BackgroundJob.Enqueue<AppSettingsController>(x => x.CheckAllAccountsAsync());
            //   RecurringJob.AddOrUpdate("NotifyBeforeEvent", () => notifyService.SendNotifyTokensAsync(), Configuration.GetSection("NotificationTimeCronLocalTime").Value, jobOptions);
            //  RecurringJob.AddOrUpdate("SendwhatsBeforeEvent", () => notifyService.SendWhatsMsgToGK(), Configuration.GetSection("WhatsMSGTimeCronLocalTime").Value, jobOptions);

        }
    }
}
