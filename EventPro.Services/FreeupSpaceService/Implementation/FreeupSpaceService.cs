using EventPro.Kernal.StaticFiles;
using EventPro.Services.FreeupSpaceService.Interface;
using EventPro.Services.RemovingDataService.Interface;


namespace EventPro.Services.FreeupSpaceService.Implementation
{
    public class FreeupSpaceService : IFreeupSpaceService
    {
        private readonly IRemovingDataService _removingData;
        public FreeupSpaceService(IRemovingDataService removingData)
        {
            _removingData = removingData;
        }
        public async Task RemoveOldEventsData()
        {
            List<string> EventsDataDirectories = StaticDirectoryList.FilesRemoving;
            await _removingData.RemoveFromDirectoriesByPeriod(EventsDataDirectories, StaticPeriod.ValidationOldEventsData);

        }
        public async Task RemoveOldEventsDBData()
        {
            List<string> EventsDataDirectories = StaticDirectoryList.FilesRemovingFromDB;
            await _removingData.RemoveFromDirectoriesDBCheckByPeriod(EventsDataDirectories, StaticPeriod.ValidationOldEventsData);

        }

        public async Task RemoveOldEventsDataBackup()
        {
            string EventsDataBackupDirectory = StaticDirectory.EventsDataBackup;
            List<string> EventsDataBackupDirectories = new List<string>() { EventsDataBackupDirectory };

            await _removingData.RemoveFromDirectoriesByPeriod(EventsDataBackupDirectories, StaticPeriod.ValidationOldEventsDataBackup);

        }
    }
}
