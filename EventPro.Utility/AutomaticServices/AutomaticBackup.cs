using Microsoft.AspNetCore.Builder;
using EventPro.Kernal.StaticFiles;
using EventPro.Services.BackupService.Interface;


namespace EventPro.Utility.AutomaticServices
{
    public static class AutomaticBackup
    {
        public static Timer? CreationDataBaseBackupTimer { get; set; }
        public static Timer? CreationEventsDataBackupTimer { get; set; }
        public static IApplicationBuilder UseAutomaticBackup(this IApplicationBuilder app, IBackupService backupService)
        {
            string file = backupService.FindLastCreatedEventsDataBackup();

            if (!File.Exists(file))
            {
                backupService.GetDatabaseBackup();
                backupService.GetEventsDataBackup();
                return app;
            }

            DateTime creationTime = File.GetCreationTime(file);
            TimeSpan elapsed = DateTime.Now.Subtract(creationTime);
            if (elapsed > StaticPeriod.CreationEventsDataBackup)
            {
                CreationDataBaseBackupTimer = new Timer(async _ => await backupService.GetDatabaseBackup(), null, TimeSpan.FromMinutes(1), StaticPeriod.CreationDataBaseBackup);
                CreationEventsDataBackupTimer = new Timer(async _ => await backupService.GetEventsDataBackup(), null, TimeSpan.FromMinutes(1), StaticPeriod.CreationEventsDataBackup);
            }

            return app;
        }



    }
}