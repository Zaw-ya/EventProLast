namespace EventPro.Services.FreeupSpaceService.Interface
{
    public interface IFreeupSpaceService
    {
        Task RemoveOldEventsDataBackup();
        Task RemoveOldEventsData();
        Task RemoveOldEventsDBData();
    }
}
