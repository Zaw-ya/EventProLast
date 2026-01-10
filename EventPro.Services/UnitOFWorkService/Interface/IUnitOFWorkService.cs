using EventPro.Services.BackupService.Interface;
using EventPro.Services.FreeupSpaceService.Interface;
using EventPro.Services.PinnacleService.Interface;
using EventPro.Services.RemovingDataService.Interface;

namespace EventPro.Services.UnitOFWorkService.Interface
{
    public interface IUnitOFWorkService
    {
        IBackupService Backup { get; }
        IFreeupSpaceService FreeupSpace { get; }
        IRemovingDataService RemovingData { get; }
        //  INotificationService Notification { get; }
        IPinnacleService Pinnacle { get; }
    }
}
