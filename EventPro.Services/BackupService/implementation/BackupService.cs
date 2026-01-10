using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EventPro.Kernal.StaticFiles;
using EventPro.Services.BackupService.Interface;
using System.IO.Compression;


namespace EventPro.Services.BackupService.implementation
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        public BackupService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task GetDatabaseBackup()
        {
            SqlConnection connnection = new SqlConnection();
            SqlCommand SQLCommand = new SqlCommand();

            connnection.ConnectionString = _configuration.GetSection("Database:ConnectionString").Value;
            string DatabaseBackupDirectory = StaticDirectory.DatabaseBackup;

            if (!Directory.Exists(DatabaseBackupDirectory)) { Directory.CreateDirectory(DatabaseBackupDirectory); }

            ExecuteDatabaseBackupCommand(connnection, DatabaseBackupDirectory);

            return Task.CompletedTask;
        }

        private static void ExecuteDatabaseBackupCommand(SqlConnection connnection, string DatabaseBackupDirectory)
        {
            SqlCommand SQLCommand;
            try
            {
                connnection.Open();
                SQLCommand = new SqlCommand("backup database EventProUAT to disk='" + DatabaseBackupDirectory + "\\" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".Bak'", connnection);
                SQLCommand.ExecuteNonQuery();
                connnection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetEventsDataBackup()
        {
            await Task.Run(() =>
            {
                List<string> EventsDataBackupSourceDirectories = StaticDirectoryList.FilesBackup;
                string EventsDataBackupDirectory = _configuration.GetSection("Backup:EventsDataBackupDirectory").Value ?? "";

                if (!Directory.Exists(EventsDataBackupDirectory)) { Directory.CreateDirectory(EventsDataBackupDirectory); }

                SaveEventsDataInZipFile(EventsDataBackupSourceDirectories, EventsDataBackupDirectory);
            });
        }

        private static void SaveEventsDataInZipFile(List<string> EventsDatasourceFolders, string EventsDataBackupDirectory)
        {
            using (ZipArchive zip = ZipFile.Open(EventsDataBackupDirectory + "\\" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".zip", ZipArchiveMode.Create))
            {
                foreach (string folder in EventsDatasourceFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        string folderName = Path.GetFileName(folder);
                        zip.CreateEntry($"./{folderName}/");
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            zip.CreateEntryFromFile(file, $"./{folderName}/{Path.GetFileName(file)}");
                        }
                    }
                }
            }
        }

        public string FindLastCreatedEventsDataBackup()
        {
            string EventsDataBackupDirctory = StaticDirectory.EventsDataBackup;
            return FindLastCreatedFileInDirectory(EventsDataBackupDirctory);

        }

        private string FindLastCreatedFileInDirectory(string EventsDataBackup)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(EventsDataBackup);
            if (!directoryInfo.Exists)
            {
                return null;
            }
            FileInfo[] files = directoryInfo.GetFiles().OrderByDescending(f => f.CreationTime).ToArray();

            if (files.Length > 0)
            {
                FileInfo lastCreatedFile = files[0];
                return lastCreatedFile.FullName;
            }
            return null;
        }

        public string FindLastCreatedDatabaseBackup()
        {

            string EventsDataBackupDirctory = StaticDirectory.DatabaseBackup;
            return FindLastCreatedFileInDirectory(EventsDataBackupDirctory);

        }
    }
}
