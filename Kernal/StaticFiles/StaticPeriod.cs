
namespace EventPro.Kernal.StaticFiles
{
    public static class StaticPeriod
    {

        //Configure the time period of creating new backup of database and events data
        public static TimeSpan CreationDataBaseBackup = TimeSpan.FromDays(7);
        public static TimeSpan CreationEventsDataBackup = TimeSpan.FromDays(7);



        //Configure the time period of checking for deleting old events data and old backup of events data
        public static TimeSpan CheckingEventsDataBackupValidity = TimeSpan.FromDays(6);
        public static TimeSpan CheckingEventsDataValidity = TimeSpan.FromDays(6);



        //Configure the time period of validation of events data and backup of events data
        public static TimeSpan ValidationOldEventsDataBackup = TimeSpan.FromDays(14);
        public static TimeSpan ValidationOldEventsData = TimeSpan.FromDays(100);



        //Configure the time period of validation of events data and backup of events data
        public static TimeSpan CheckingCurrentPinnacleBalance = TimeSpan.FromHours(6);


    }
}
