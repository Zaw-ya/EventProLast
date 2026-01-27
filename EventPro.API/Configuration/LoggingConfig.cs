using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace EventPro.API.Configuration
{
    public static class LoggingConfig
    {
        public static void ConfigureSerilog(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                //.WriteTo.MSSqlServer(
                //    configuration.GetSection("Database")["ConnectionString"],
                //    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions()
                //    {
                //        TableName = "SeriLogAPI",
                //        AutoCreateSqlTable = true,
                //    })
                .CreateLogger();
        }
    }
}
