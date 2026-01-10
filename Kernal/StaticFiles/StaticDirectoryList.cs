

namespace EventPro.Kernal.StaticFiles
{
    public static class StaticDirectoryList
    {
        //list of directories that events data backup will be made from its files
        public static List<string> FilesBackup { get; set; } = new List<string>()
        {
                StaticDirectory.Card,
                StaticDirectory.Cardpreview,
                StaticDirectory.Barcode,
                StaticDirectory.Guestcode,
                StaticDirectory.Guestcard,
                StaticDirectory.Excel,
        };


        //list of directories that will be checked by checking period to remove its old files
        public static List<string> FilesRemoving { get; set; } = new List<string>()
        {
                StaticDirectory.Barcode,
                StaticDirectory.Guestcode,
                StaticDirectory.Guestcard,
                StaticDirectory.Excel
        };



        //list of directories that will be checked by checking period to remove its old files
        public static List<string> FilesRemovingFromDB { get; set; } = new List<string>()
        {
                StaticDirectory.Card,
                StaticDirectory.Cardpreview
        };

    }
}
