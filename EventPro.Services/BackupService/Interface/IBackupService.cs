namespace EventPro.Services.BackupService.Interface
{
    public interface IBackupService
    {
        Task GetDatabaseBackup();
        Task GetEventsDataBackup();
        string FindLastCreatedDatabaseBackup();
        string FindLastCreatedEventsDataBackup();
    }
}
