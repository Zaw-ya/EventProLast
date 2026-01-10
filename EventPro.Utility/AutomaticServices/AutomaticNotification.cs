using Microsoft.AspNetCore.Builder;
using EventPro.Kernal.StaticFiles;
using EventPro.Services.NotificationService.Interface;


namespace EventPro.Utility.AutomaticServices
{
    public static class AutomaticNotification
    {
        public static Timer? CheckingCurrentPinnacleBalanceTimer { get; set; }

        public static IApplicationBuilder UseAutomaticNotification(this IApplicationBuilder app, INotificationService Notification)
        {
            CheckingCurrentPinnacleBalanceTimer = new Timer(async _ => await Notification.SendPinnacleBalanceAlert(), null, TimeSpan.FromMinutes(0), StaticPeriod.CheckingCurrentPinnacleBalance);

            return app;
        }
    }
}
