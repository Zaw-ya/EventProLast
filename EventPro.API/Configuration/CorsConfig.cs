using Microsoft.Extensions.DependencyInjection;

namespace EventPro.API.Configuration
{
    public static class CorsConfig
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            //services.AddMemoryCache();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", builder =>
                {
                    builder.WithOrigins("https://localhost:44302/", "https://app.EventPro.me")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
