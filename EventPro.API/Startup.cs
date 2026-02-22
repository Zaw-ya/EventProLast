using EventPro.API.Configuration;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace EventPro.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Logging
            LoggingConfig.ConfigureSerilog(Configuration);

            // Configure CORS
            services.AddCorsConfiguration();

            // Configure Swagger
            services.AddSwaggerConfiguration();

            // Configure JWT Authentication
            services.AddJwtAuthentication(Configuration);

            // Configure Dependency Injection
            services.AddDependencyInjection(Configuration);

            // Configure Controllers
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            #region Firebase Admin SDK Initialization
            var firebaseJsonFileName = Configuration["FireBaseJSON"];
            if (string.IsNullOrEmpty(firebaseJsonFileName))
            {
                Log.Warning("Firebase: 'FireBaseJSON' key is missing from appsettings. Firebase features disabled.");
            }
            else
            {
                var firebasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, firebaseJsonFileName);
                if (File.Exists(firebasePath))
                {
                    FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile(firebasePath) });
                    Log.Information("Firebase Admin SDK initialized with project: {ProjectId}", Configuration["FireBaseProjId"]);
                }
                else
                {
                    Log.Warning("Firebase admin SDK file not found at {Path}. Firebase features disabled.", firebasePath);
                }
            }
            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "EventPro API v1");
                options.DocumentTitle = "EventPro API Documentation";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
