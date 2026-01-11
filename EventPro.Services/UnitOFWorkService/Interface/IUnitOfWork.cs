using EventPro.DAL.Models;
using EventPro.Services.Repository;
using System;
using System.Threading.Tasks;

namespace EventPro.Services.UnitOFWorkService
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<ReportDeletedEventsByGk> ReportDeletedEventsByGk { get; }
        IBaseRepository<ConfirmationMessageResponsesKeyword> ConfirmationMessageResponsesKeyword { get; }
        IBaseRepository<GKEventHistory> GKEventHistory { get; }
        IBaseRepository<EventLocation> EventLocations { get; }
        IBaseRepository<Country> Country { get; }
        IBaseRepository<City> City { get; }
        IBaseRepository<SerilogScan> SerilogScan { get; }
        IBaseRepository<GuestsDeliveredourServiceMessage> GuestsDeliveredourServiceMessage { get; }
        IBaseRepository<CardInfo> CardInfo { get; }
        IBaseRepository<EventCategory> EventCategory { get; }
        IBaseRepository<EventGatekeeperMapping> EventGatekeeperMapping { get; }
        IBaseRepository<Events> Events { get; }
        IBaseRepository<Guest> Guest { get; }
        IBaseRepository<InvoiceDetails> InvoiceDetails { get; }
        IBaseRepository<Invoices> Invoices { get; }
        IBaseRepository<LocallizationMaster> LocallizationMaster { get; }
        IBaseRepository<Roles> Roles { get; }
        IBaseRepository<BulkOperatorEvents> BulkOperatorEvents { get; }
        IBaseRepository<EventOperator> EventOperator { get; }
        IBaseRepository<ScanHistory> ScanHistory { get; }
        IBaseRepository<Users> Users { get; }
        IBaseRepository<VwEventCategory> VwEventCategory { get; }
        IBaseRepository<VwEventGatekeeper> VwEventGatekeeper { get; }
        IBaseRepository<VwEvents> VwEvents { get; }
        IBaseRepository<VMDashboardCount> VMDashboardCount { get; }
        IBaseRepository<VwGateKeeperData> VwGateKeeperData { get; }
        IBaseRepository<VwGateKeeperScheduled> VwGateKeeperScheduled { get; }
        IBaseRepository<VwGuestList> VwGuestList { get; }
        IBaseRepository<VwGuestReportWa> VwGuestReportWa { get; }
        IBaseRepository<VwMiagregationReport> VwMiagregationReport { get; }
        IBaseRepository<vw_ConfirmationReport> vw_ConfirmationReport { get; }
        IBaseRepository<VwScanForCount> VwScanForCount { get; }
        IBaseRepository<VwScanHistory> VwScanHistory { get; }
        IBaseRepository<VwScanHistoryLogs> VwScanHistoryLogs { get; }
        IBaseRepository<VwScanLogs> VwScanLogs { get; }
        IBaseRepository<VwScannedInfo> VwScannedInfo { get; }
        IBaseRepository<vwGuestInfo> vwGuestInfo { get; }
        IBaseRepository<VwUsers> VwUsers { get; }
        IBaseRepository<WhatsappResponseLogs> WhatsappResponseLogs { get; }
        IBaseRepository<ValidateQRCodeResult> ValidateQRCodeResult { get; }
        IBaseRepository<EventsStatsByGK> EventsStatsByGK { get; }
        IBaseRepository<AppSettings> AppSettings { get; }
        IBaseRepository<DefaultWhatsappSettings> DefaultWhatsappSettings { get; }
        IBaseRepository<TwilioProfileSettings> TwilioProfileSettings { get; }
        IBaseRepository<ScanSummary> ScanSummary { get; }
        IBaseRepository<MobileLog> MobileLog { get; }
        IBaseRepository<AuditLog> AuditLog { get; }

        Task<int> Complete();
    }
}
