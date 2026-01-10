using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace EventPro.DAL.Models
{
    public partial class EventProContext : DbContext
    {
        //Scaffold-DbContext "Server=GALAXY\GALAXYDB;database=EventProUAT;integrated security=SSPI" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -force
        private DataBaseConfig config = new DataBaseConfig();
        private readonly IConfiguration configuration;
        private readonly int TimeOutInHours;
        public EventProContext(IConfiguration iConfig)
        {
            configuration = iConfig;
            config = new DataBaseConfig
            {
                ConnectionString = configuration.GetSection("Database").GetSection("ConnectionString").Value,
                Client = configuration.GetSection("Database").GetSection("Client").Value
            };
            TimeOutInHours = Convert.ToInt32(configuration.GetSection("CacheTimeInHr").Value);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(config.ConnectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(60));
            }
        }
    }
}
