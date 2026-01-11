using EventPro.DAL.Models;
using EventPro.Services.Repository;
using EventPro.Services.Repository.Interface;
using System;
using System.Threading.Tasks;

namespace EventPro.Services.UnitOFWorkService
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EventProContext _context;

        public UnitOfWork(EventProContext context)
        {
            _context = context;
        }

        private IBaseRepository<ReportDeletedEventsByGk>? _ReportDeletedEventsByGk;
        public IBaseRepository<ReportDeletedEventsByGk> ReportDeletedEventsByGk => _ReportDeletedEventsByGk ??= new BaseRepository<ReportDeletedEventsByGk>(_context);

        private IBaseRepository<ConfirmationMessageResponsesKeyword>? _ConfirmationMessageResponsesKeyword;
        public IBaseRepository<ConfirmationMessageResponsesKeyword> ConfirmationMessageResponsesKeyword => _ConfirmationMessageResponsesKeyword ??= new BaseRepository<ConfirmationMessageResponsesKeyword>(_context);

        private IBaseRepository<GKEventHistory>? _GKEventHistory;
        public IBaseRepository<GKEventHistory> GKEventHistory => _GKEventHistory ??= new BaseRepository<GKEventHistory>(_context);

        private IBaseRepository<EventLocation>? _EventLocations;
        public IBaseRepository<EventLocation> EventLocations => _EventLocations ??= new BaseRepository<EventLocation>(_context);

        private IBaseRepository<Country>? _Country;
        public IBaseRepository<Country> Country => _Country ??= new BaseRepository<Country>(_context);

        private IBaseRepository<City>? _City;
        public IBaseRepository<City> City => _City ??= new BaseRepository<City>(_context);

        private IBaseRepository<SerilogScan>? _SerilogScan;
        public IBaseRepository<SerilogScan> SerilogScan => _SerilogScan ??= new BaseRepository<SerilogScan>(_context);

        private IBaseRepository<GuestsDeliveredourServiceMessage>? _GuestsDeliveredourServiceMessage;
        public IBaseRepository<GuestsDeliveredourServiceMessage> GuestsDeliveredourServiceMessage => _GuestsDeliveredourServiceMessage ??= new BaseRepository<GuestsDeliveredourServiceMessage>(_context);

        private IBaseRepository<CardInfo>? _CardInfo;
        public IBaseRepository<CardInfo> CardInfo => _CardInfo ??= new BaseRepository<CardInfo>(_context);

        private IBaseRepository<EventCategory>? _EventCategory;
        public IBaseRepository<EventCategory> EventCategory => _EventCategory ??= new BaseRepository<EventCategory>(_context);

        private IBaseRepository<EventGatekeeperMapping>? _EventGatekeeperMapping;
        public IBaseRepository<EventGatekeeperMapping> EventGatekeeperMapping => _EventGatekeeperMapping ??= new BaseRepository<EventGatekeeperMapping>(_context);

        private IBaseRepository<Events>? _Events;
        public IBaseRepository<Events> Events => _Events ??= new BaseRepository<Events>(_context);

        private IBaseRepository<Guest>? _Guest;
        public IBaseRepository<Guest> Guest => _Guest ??= new BaseRepository<Guest>(_context);

        private IBaseRepository<InvoiceDetails>? _InvoiceDetails;
        public IBaseRepository<InvoiceDetails> InvoiceDetails => _InvoiceDetails ??= new BaseRepository<InvoiceDetails>(_context);

        private IBaseRepository<Invoices>? _Invoices;
        public IBaseRepository<Invoices> Invoices => _Invoices ??= new BaseRepository<Invoices>(_context);

        private IBaseRepository<LocallizationMaster>? _LocallizationMaster;
        public IBaseRepository<LocallizationMaster> LocallizationMaster => _LocallizationMaster ??= new BaseRepository<LocallizationMaster>(_context);

        private IBaseRepository<Roles>? _Roles;
        public IBaseRepository<Roles> Roles => _Roles ??= new BaseRepository<Roles>(_context);

        private IBaseRepository<BulkOperatorEvents>? _BulkOperatorEvents;
        public IBaseRepository<BulkOperatorEvents> BulkOperatorEvents => _BulkOperatorEvents ??= new BaseRepository<BulkOperatorEvents>(_context);

        private IBaseRepository<EventOperator>? _EventOperator;
        public IBaseRepository<EventOperator> EventOperator => _EventOperator ??= new BaseRepository<EventOperator>(_context);

        private IBaseRepository<ScanHistory>? _ScanHistory;
        public IBaseRepository<ScanHistory> ScanHistory => _ScanHistory ??= new BaseRepository<ScanHistory>(_context);

        private IBaseRepository<Users>? _Users;
        public IBaseRepository<Users> Users => _Users ??= new BaseRepository<Users>(_context);

        private IBaseRepository<VwEventCategory>? _VwEventCategory;
        public IBaseRepository<VwEventCategory> VwEventCategory => _VwEventCategory ??= new BaseRepository<VwEventCategory>(_context);

        private IBaseRepository<VwEventGatekeeper>? _VwEventGatekeeper;
        public IBaseRepository<VwEventGatekeeper> VwEventGatekeeper => _VwEventGatekeeper ??= new BaseRepository<VwEventGatekeeper>(_context);

        private IBaseRepository<VwEvents>? _VwEvents;
        public IBaseRepository<VwEvents> VwEvents => _VwEvents ??= new BaseRepository<VwEvents>(_context);

        private IBaseRepository<VMDashboardCount>? _VMDashboardCount;
        public IBaseRepository<VMDashboardCount> VMDashboardCount => _VMDashboardCount ??= new BaseRepository<VMDashboardCount>(_context);

        private IBaseRepository<VwGateKeeperData>? _VwGateKeeperData;
        public IBaseRepository<VwGateKeeperData> VwGateKeeperData => _VwGateKeeperData ??= new BaseRepository<VwGateKeeperData>(_context);

        private IBaseRepository<VwGateKeeperScheduled>? _VwGateKeeperScheduled;
        public IBaseRepository<VwGateKeeperScheduled> VwGateKeeperScheduled => _VwGateKeeperScheduled ??= new BaseRepository<VwGateKeeperScheduled>(_context);

        private IBaseRepository<VwGuestList>? _VwGuestList;
        public IBaseRepository<VwGuestList> VwGuestList => _VwGuestList ??= new BaseRepository<VwGuestList>(_context);

        private IBaseRepository<VwGuestReportWa>? _VwGuestReportWa;
        public IBaseRepository<VwGuestReportWa> VwGuestReportWa => _VwGuestReportWa ??= new BaseRepository<VwGuestReportWa>(_context);

        private IBaseRepository<VwMiagregationReport>? _VwMiagregationReport;
        public IBaseRepository<VwMiagregationReport> VwMiagregationReport => _VwMiagregationReport ??= new BaseRepository<VwMiagregationReport>(_context);

        private IBaseRepository<vw_ConfirmationReport>? _vw_ConfirmationReport;
        public IBaseRepository<vw_ConfirmationReport> vw_ConfirmationReport => _vw_ConfirmationReport ??= new BaseRepository<vw_ConfirmationReport>(_context);

        private IBaseRepository<VwScanForCount>? _VwScanForCount;
        public IBaseRepository<VwScanForCount> VwScanForCount => _VwScanForCount ??= new BaseRepository<VwScanForCount>(_context);

        private IBaseRepository<VwScanHistory>? _VwScanHistory;
        public IBaseRepository<VwScanHistory> VwScanHistory => _VwScanHistory ??= new BaseRepository<VwScanHistory>(_context);

        private IBaseRepository<VwScanHistoryLogs>? _VwScanHistoryLogs;
        public IBaseRepository<VwScanHistoryLogs> VwScanHistoryLogs => _VwScanHistoryLogs ??= new BaseRepository<VwScanHistoryLogs>(_context);

        private IBaseRepository<VwScanLogs>? _VwScanLogs;
        public IBaseRepository<VwScanLogs> VwScanLogs => _VwScanLogs ??= new BaseRepository<VwScanLogs>(_context);

        private IBaseRepository<VwScannedInfo>? _VwScannedInfo;
        public IBaseRepository<VwScannedInfo> VwScannedInfo => _VwScannedInfo ??= new BaseRepository<VwScannedInfo>(_context);

        private IBaseRepository<vwGuestInfo>? _vwGuestInfo;
        public IBaseRepository<vwGuestInfo> vwGuestInfo => _vwGuestInfo ??= new BaseRepository<vwGuestInfo>(_context);

        private IBaseRepository<VwUsers>? _VwUsers;
        public IBaseRepository<VwUsers> VwUsers => _VwUsers ??= new BaseRepository<VwUsers>(_context);

        private IBaseRepository<WhatsappResponseLogs>? _WhatsappResponseLogs;
        public IBaseRepository<WhatsappResponseLogs> WhatsappResponseLogs => _WhatsappResponseLogs ??= new BaseRepository<WhatsappResponseLogs>(_context);

        private IBaseRepository<ValidateQRCodeResult>? _ValidateQRCodeResult;
        public IBaseRepository<ValidateQRCodeResult> ValidateQRCodeResult => _ValidateQRCodeResult ??= new BaseRepository<ValidateQRCodeResult>(_context);

        private IBaseRepository<EventsStatsByGK>? _EventsStatsByGK;
        public IBaseRepository<EventsStatsByGK> EventsStatsByGK => _EventsStatsByGK ??= new BaseRepository<EventsStatsByGK>(_context);

        private IBaseRepository<AppSettings>? _AppSettings;
        public IBaseRepository<AppSettings> AppSettings => _AppSettings ??= new BaseRepository<AppSettings>(_context);

        private IBaseRepository<DefaultWhatsappSettings>? _DefaultWhatsappSettings;
        public IBaseRepository<DefaultWhatsappSettings> DefaultWhatsappSettings => _DefaultWhatsappSettings ??= new BaseRepository<DefaultWhatsappSettings>(_context);

        private IBaseRepository<TwilioProfileSettings>? _TwilioProfileSettings;
        public IBaseRepository<TwilioProfileSettings> TwilioProfileSettings => _TwilioProfileSettings ??= new BaseRepository<TwilioProfileSettings>(_context);

        private IBaseRepository<ScanSummary>? _ScanSummary;
        public IBaseRepository<ScanSummary> ScanSummary => _ScanSummary ??= new BaseRepository<ScanSummary>(_context);

        private IBaseRepository<MobileLog>? _MobileLog;
        public IBaseRepository<MobileLog> MobileLog => _MobileLog ??= new BaseRepository<MobileLog>(_context);

        private IBaseRepository<AuditLog>? _AuditLog;
        public IBaseRepository<AuditLog> AuditLog => _AuditLog ??= new BaseRepository<AuditLog>(_context);


        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
