using Microsoft.AspNetCore.Builder;
using EventPro.Kernal.StaticFiles;
using EventPro.Services.FreeupSpaceService.Interface;

namespace EventPro.Utility.AutomaticServices
{
    public static class AutomaticFreeupSpace
    {
        public static Timer? CheckingEventsDataValidityTimer { get; set; }
        public static Timer? CheckingEventsDataDBValidityTimer { get; set; }
        public static Timer? CheckingEventsDataBackupValidityTimer { get; set; }
        public static IApplicationBuilder UseAutomaticFreeupSpace(this IApplicationBuilder app, IFreeupSpaceService FreeupSpace)
        {
            CheckingEventsDataValidityTimer = new Timer(async _ => await FreeupSpace.RemoveOldEventsData(), null, TimeSpan.FromMinutes(1), StaticPeriod.CheckingEventsDataValidity);
            CheckingEventsDataDBValidityTimer = new Timer(async _ => await FreeupSpace.RemoveOldEventsDBData(), null, TimeSpan.FromMinutes(1), StaticPeriod.CheckingEventsDataValidity);
            CheckingEventsDataBackupValidityTimer = new Timer(async _ => await FreeupSpace.RemoveOldEventsDataBackup(), null, TimeSpan.FromMinutes(1), StaticPeriod.CheckingEventsDataBackupValidity);

            return app;
        }
    }
}
