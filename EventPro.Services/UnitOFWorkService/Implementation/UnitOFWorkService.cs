using Microsoft.Extensions.Configuration;
using EventPro.Services.BackupService.Interface;
using EventPro.Services.FreeupSpaceService.Interface;
using EventPro.Services.PinnacleService.Interface;
using EventPro.Services.RemovingDataService.Interface;
using EventPro.Services.UnitOFWorkService.Interface;

namespace EventPro.Services.UnitOFWorkService.Implementation
{
    public class UnitOFWorkService : IUnitOFWorkService
    {
        private readonly IConfiguration _configuration;

        public UnitOFWorkService(IConfiguration configuration)
        {
            _configuration = configuration;

            RemovingData = new RemovingDataService.Implementation.RemovingDataService();
            FreeupSpace = new FreeupSpaceService.Implementation.FreeupSpaceService(RemovingData);
            Backup = new BackupService.implementation.BackupService(_configuration);
            //   Notification = new NotificationService.Implementation.NotificationService(Pinnacle, _configuration);
        }
        public IBackupService? Backup { get; private set; }

        public IFreeupSpaceService? FreeupSpace { get; private set; }

        public IRemovingDataService? RemovingData { get; private set; }

        //  public INotificationService Notification { get; private set; }

        public IPinnacleService Pinnacle { get; private set; }
    }
}
