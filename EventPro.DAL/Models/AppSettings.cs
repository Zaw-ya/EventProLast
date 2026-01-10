using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EventPro.DAL.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        [Required]
        [NotNull]
        public string GateKeeperCheckNotificationsNumber { get; set; }
        [Required]
        [NotNull]
        public int GateKeeperReminderPeriodForEvent { get; set; }
        [Required]
        [NotNull]
        public string WhatsappServiceProvider { get; set; }
        [Required]
        [NotNull]
        public string WhatsappDefaultTwilioProfile { get; set; }
        [Required]
        [NotNull]
        public int BulkSendingLimit { get; set; }
        [Required]
        [NotNull]
        public int NumberOfOpertorToSendBulkOnSameTime { get; set; }
        [Required]
        [NotNull]
        public int NumberOfWebHookRequestsDbCanHandleOnSameTime { get; set; }

        [Required]
        [NotNull]
        public decimal TwilioBalanceEmailAlertThreshold { get; set; }

    }
}
