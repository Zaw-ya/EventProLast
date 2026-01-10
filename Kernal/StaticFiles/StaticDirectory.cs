
namespace EventPro.Kernal.StaticFiles
{
    public static class StaticDirectory
    {
        static StaticDirectory()
        {
            Card = ConfigurationHelper.config.GetSection("Uploads").GetSection("Card").Value;
            Barcode = ConfigurationHelper.config.GetSection("Uploads").GetSection("Barcode").Value;
            Guestcode = ConfigurationHelper.config.GetSection("Uploads").GetSection("Guestcode").Value;
            Guestcard = ConfigurationHelper.config.GetSection("Uploads").GetSection("Guestcard").Value;
            Cardpreview = ConfigurationHelper.config.GetSection("Uploads").GetSection("Cardpreview").Value;
            Invoice = ConfigurationHelper.config.GetSection("Uploads").GetSection("Invoice").Value;
            Excel = ConfigurationHelper.config.GetSection("Uploads").GetSection("Excel").Value;

            CurrentDirectory = ConfigurationHelper.config.GetSection("Uploads").GetSection("CurrentDirectory").Value;

            Backup = ConfigurationHelper.config.GetSection("Backup").GetSection("BackupDirectory").Value;
            DatabaseBackup = ConfigurationHelper.config.GetSection("Backup").GetSection("DatabaseBackupDirectory").Value;
            EventsDataBackup = ConfigurationHelper.config.GetSection("Backup").GetSection("EventsDataBackupDirectory").Value;
        }

        public static string Card { get; set; }
        public static string Barcode { get; set; }
        public static string Guestcode { get; set; }
        public static string Guestcard { get; set; }
        public static string Cardpreview { get; set; }
        public static string Invoice { get; set; }
        public static string Excel { get; set; }

        public static string CurrentDirectory { get; set; }


        public static string Backup { get; set; }
        public static string DatabaseBackup { get; set; }
        public static string EventsDataBackup { get; set; }

    }
}
