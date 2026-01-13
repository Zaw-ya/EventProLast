using Microsoft.EntityFrameworkCore;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class EventProContext : DbContext
    {

        //public EventProContext(DbContextOptions<EventProContext> options)
        //    : base(options)
        //{

        //}

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            /*
            builder.Properties<TimeOnly?>()
                .HaveConversion<TimeOnlyConverter>()
                .HaveColumnType("time");
            */

            base.ConfigureConventions(builder);
        }
        public DbSet<ReportDeletedEventsByGk> ReportDeletedEventsByGk { get; set; }
        public DbSet<ConfirmationMessageResponsesKeyword> ConfirmationMessageResponsesKeyword { get; set; }

        public DbSet<GKEventHistory> GKEventHistory { get; set; }
        public DbSet<EventLocation> EventLocations { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<SerilogScan> SeriLog { get; set; }
        public DbSet<GuestsDeliveredourServiceMessage> GuestsDeliveredourServiceMessage { get; set; }

        public virtual DbSet<CardInfo> CardInfo { get; set; }
        public virtual DbSet<EventCategory> EventCategory { get; set; }
        public virtual DbSet<EventGatekeeperMapping> EventGatekeeperMapping { get; set; }
        public virtual DbSet<Events> Events { get; set; }
        public virtual DbSet<Guest> Guest { get; set; }
        public virtual DbSet<InvoiceDetails> InvoiceDetails { get; set; }
        public virtual DbSet<Invoices> Invoices { get; set; }
        public virtual DbSet<LocallizationMaster> LocallizationMaster { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<BulkOperatorEvents> BulkOperatorEvents { get; set; }
        public virtual DbSet<EventOperator> EventOperator { get; set; }
        public virtual DbSet<ScanHistory> ScanHistory { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<VwEventCategory> VwEventCategory { get; set; }
        public virtual DbSet<VwEventGatekeeper> VwEventGatekeeper { get; set; }
        public DbSet<VwEvents> VwEvents { get; set; }
        public virtual DbSet<VMDashboardCount> VMDashboardCount { get; set; }
        public virtual DbSet<VwGateKeeperData> VwGateKeeperData { get; set; }
        public virtual DbSet<VwGateKeeperScheduled> VwGateKeeperScheduled { get; set; }
        public virtual DbSet<VwGuestList> VwGuestList { get; set; }
        public virtual DbSet<VwGuestReportWa> VwGuestReportWa { get; set; }
        public virtual DbSet<VwMiagregationReport> VwMiagregationReport { get; set; }
        public virtual DbSet<vw_ConfirmationReport> vw_ConfirmationReport { get; set; }
        public virtual DbSet<VwScanForCount> VwScanForCount { get; set; }
        public virtual DbSet<VwScanHistory> VwScanHistory { get; set; }
        public virtual DbSet<VwScanHistoryLogs> VwScanHistoryLogs { get; set; }
        public virtual DbSet<VwScanLogs> VwScanLogs { get; set; }
        public virtual DbSet<VwScannedInfo> VwScannedInfo { get; set; }
        public virtual DbSet<vwGuestInfo> vwGuestInfo { get; set; }
        public virtual DbSet<VwUsers> VwUsers { get; set; }
        public virtual DbSet<WhatsappResponseLogs> WhatsappResponseLogs { get; set; }
        public virtual DbSet<ValidateQRCodeResult> ValidateQRCodeResult { get; set; }
        public virtual DbSet<EventsStatsByGK> EventsStatsByGK { get; set; }
        public virtual DbSet<AppSettings> AppSettings { get; set; }
        public virtual DbSet<DefaultWhatsappSettings> DefaultWhatsappSettings { get; set; }
        public virtual DbSet<TwilioProfileSettings> TwilioProfileSettings { get; set; }
        public virtual DbSet<ScanSummary> ScanSummary { get; set; }
        public virtual DbSet<MobileLog> MobileLog { get; set; }
        public virtual DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ScanSummary>().HasNoKey();
            modelBuilder.Entity<ValidateQRCodeResult>().HasNoKey();
            modelBuilder.Entity<EventsStatsByGK>().HasNoKey();
            modelBuilder.Entity<CardInfo>(entity =>
            {
                entity.HasKey(e => e.CardId)
                    .HasName("PK__CardInfo__55FECDAE4B958A2D");

                entity.Property(e => e.AddTextFontAlignment)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.AltTextFontColor)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.AltTextFontName)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.AltTextXaxis).HasColumnName("AltTextXAxis");

                entity.Property(e => e.AltTextYaxis).HasColumnName("AltTextYAxis");

                entity.Property(e => e.BackgroundColor)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.BackgroundImage)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.BarcodeColorCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.BarcodeXaxis).HasColumnName("BarcodeXAxis");

                entity.Property(e => e.BarcodeYaxis).HasColumnName("BarcodeYAxis");

                entity.Property(e => e.ContactNameXaxis).HasColumnName("ContactNameXAxis");

                entity.Property(e => e.ContactNameYaxis).HasColumnName("ContactNameYAxis");

                entity.Property(e => e.ContactNoAlignment)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNoFontColor)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNoFontName)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.ContactNoXaxis).HasColumnName("ContactNoXAxis");

                entity.Property(e => e.ContactNoYaxis).HasColumnName("ContactNoYAxis");

                entity.Property(e => e.DefaultFont)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FontAlignment)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.FontColor)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FontName)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FontStyleAddText)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FontStyleMobNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FontStyleName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FontStyleNos)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.ForegroundColor)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.NosAlignment)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.NosfontColor)
                    .HasColumnName("NOSFontColor")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.NosfontName)
                    .HasColumnName("NOSFontName")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.NosfontSize).HasColumnName("NOSFontSize");

                entity.Property(e => e.Nosxaxis).HasColumnName("NOSXAxis");

                entity.Property(e => e.Nosyaxis).HasColumnName("NOSYAxis");

                entity.Property(e => e.SelectedPlaceHolder)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.CardInfo)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__CardInfo__EventI__160F4887");
            });

            modelBuilder.Entity<EventOperator>()
            .HasKey(ue => new { ue.OperatorId, ue.EventId });

            modelBuilder.Entity<EventOperator>()
                .HasOne(ue => ue.Operator)
                .WithMany(u => u.EventOperators)
                .HasForeignKey(ue => ue.OperatorId);

            modelBuilder.Entity<EventOperator>()
                .HasOne(ue => ue.Event)
                .WithMany(e => e.EventOperators)
                .HasForeignKey(ue => ue.EventId);
        

        modelBuilder.Entity<EventCategory>(entity =>
            {
                entity.HasKey(e => e.EventId)
                    .HasName("PK__EventCat__7944C810DB34F5B1");

                entity.Property(e => e.Category).HasMaxLength(100);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Icon).HasMaxLength(500);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.EventCategory)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__EventCate__Creat__412EB0B6");
            });

            modelBuilder.Entity<EventGatekeeperMapping>(entity =>
            {
                entity.HasKey(e => e.TaskId)
                    .HasName("PK__EventGat__7C6949B1EFFB8CEC");

                entity.Property(e => e.AsssignedOn).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.HasOne(d => d.AssignedByNavigation)
                    .WithMany(p => p.EventGatekeeperMappingAssignedByNavigation)
                    .HasForeignKey(d => d.AssignedBy)
                    .HasConstraintName("FK__EventGate__Assig__51300E55");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.EventGatekeeperMapping)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__EventGate__Event__4F47C5E3");

                entity.HasOne(d => d.Gatekeeper)
                    .WithMany(p => p.EventGatekeeperMappingGatekeeper)
                    .HasForeignKey(d => d.GatekeeperId)
                    .HasConstraintName("FK__EventGate__Gatek__503BEA1C");
            });

            modelBuilder.Entity<Events>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventTo).HasColumnType("datetime");

                entity.Property(e => e.EventVenue).HasMaxLength(1000);

                entity.Property(e => e.GmapCode)
                    .HasColumnName("GMapCode")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.ParentTitle).HasMaxLength(200);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.EventsCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Events__CreatedB__44FF419A");

                entity.HasOne(d => d.CreatedForNavigation)
                    .WithMany(p => p.EventsCreatedForNavigation)
                    .HasForeignKey(d => d.CreatedFor)
                    .HasConstraintName("FK__Events__CreatedF__45F365D3");

                entity.HasOne(d => d.ModifiedByNavigation)
                    .WithMany(p => p.EventsModifiedByNavigation)
                    .HasForeignKey(d => d.ModifiedBy)
                    .HasConstraintName("FK__Events__Modified__46E78A0C");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.Type)
                    .HasConstraintName("FK__Events__Type__440B1D61");
            });

            modelBuilder.Entity<Guest>(entity =>
            {
                entity.Property(e => e.AdditionalText).HasMaxLength(500);

                entity.Property(e => e.Address).HasMaxLength(1000);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Cypertext)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.ImgSenOn)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.ImgSentMsgId)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.MessageId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ModeOfCommunication)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MsgResponse).IsUnicode(false);

                entity.Property(e => e.PrimaryContactNo).HasMaxLength(40);

                entity.Property(e => e.Qrresponse)
                    .HasColumnName("QRResponse")
                    .IsUnicode(false);

                entity.Property(e => e.Response).HasMaxLength(400);

                entity.Property(e => e.SecondaryContactNo)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.WaresponseTime)
                    .HasColumnName("WAResponseTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.WasentOn)
                    .HasColumnName("WASentOn")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.WhatsappStatus)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.GuestCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Guest__CreatedBy__2DE6D218");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Guest)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__Guest__EventId__2EDAF651");

                entity.HasOne(d => d.GateKeeperNavigation)
                    .WithMany(p => p.GuestGateKeeperNavigation)
                    .HasForeignKey(d => d.GateKeeper)
                    .HasConstraintName("FK__Guest__GateKeepe__2FCF1A8A");
            });

            modelBuilder.Entity<InvoiceDetails>(entity =>
            {
                entity.HasKey(e => e.Idid)
                    .HasName("PK__InvoiceD__B87DF1C449EDCD95");

                entity.Property(e => e.Idid).HasColumnName("IDId");

                entity.Property(e => e.NoFguest)
                    .HasColumnName("NoFGuest")
                    .HasMaxLength(1000);

                entity.Property(e => e.Product).HasMaxLength(1000);

                entity.Property(e => e.Qty).HasColumnType("money");

                entity.Property(e => e.Rate).HasColumnType("money");

                entity.Property(e => e.Total).HasColumnType("money");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("FK__InvoiceDe__Invoi__1F63A897");
            });

            modelBuilder.Entity<Invoices>(entity =>
            {
                entity.HasIndex(e => e.EventCode)
                    .HasName("UQ__Invoices__640F671637A0326E")
                    .IsUnique();

                entity.Property(e => e.BillTo).HasMaxLength(40);

                entity.Property(e => e.BillingAddress).HasMaxLength(200);

                entity.Property(e => e.BillingContactNo).HasMaxLength(200);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.EventCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.EventLocation).HasMaxLength(100);

                entity.Property(e => e.EventName).HasMaxLength(100);

                entity.Property(e => e.EventPlace).HasMaxLength(100);

                entity.Property(e => e.InvoiceDate).HasColumnType("date");

                entity.Property(e => e.NetDue).HasColumnType("money");

                entity.Property(e => e.TaxPer).HasColumnType("money");

                entity.Property(e => e.TotalDue).HasColumnType("money");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__Invoices__EventI__1C873BEC");
            });

            modelBuilder.Entity<LocallizationMaster>(entity =>
            {
                entity.Property(e => e.LabelName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.RegionCode)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Translation).HasMaxLength(500);
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasIndex(e => e.RoleName)
                    .HasName("UQ__Roles__8A2B6160339910C6")
                    .IsUnique();

                entity.Property(e => e.RoleName)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ScanHistory>(entity =>
            {
                entity.HasKey(e => e.ScanId)
                    .HasName("PK__ScanHist__63B326812054FE6F");

                entity.Property(e => e.Response)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResponseCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedOn).HasColumnType("datetime");

                entity.HasOne(d => d.Guest)
                    .WithMany(p => p.ScanHistory)
                    .HasForeignKey(d => d.GuestId)
                    .HasConstraintName("FK__ScanHisto__Guest__3D2915A8");

                entity.HasOne(d => d.ScanByNavigation)
                    .WithMany(p => p.ScanHistory)
                    .HasForeignKey(d => d.ScanBy)
                    .HasConstraintName("FK__ScanHisto__ScanB__3C34F16F");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__Users__1788CC4CDDBA614D");

                entity.HasIndex(e => e.UserName)
                    .HasName("UQ__Users__C9F2845664DD6561")
                    .IsUnique();

                entity.Property(e => e.Address).HasMaxLength(500);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName).HasMaxLength(30);

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Ibnnumber)
                    .HasColumnName("IBNNumber")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.LastName).HasMaxLength(30);

                entity.Property(e => e.LockedOn).HasColumnType("datetime");

                entity.Property(e => e.ModeOfCommunication)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PreferedTimeZone)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryContactNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.SecondaryContantNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.TemporaryPass)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.InverseCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Users__CreatedBy__3A81B327");

                entity.HasOne(d => d.ModifiedByNavigation)
                    .WithMany(p => p.InverseModifiedByNavigation)
                    .HasForeignKey(d => d.ModifiedBy)
                    .HasConstraintName("FK__Users__ModifiedB__3B75D760");

                entity.HasOne(d => d.RoleNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.Role)
                    .HasConstraintName("FK__Users__Role__4F7CD00D");
            });

            modelBuilder.Entity<VwEventCategory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_EventCategory");

                entity.Property(e => e.Category).HasMaxLength(500);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Icon)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwEventGatekeeper>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_EventGatekeeper");

                entity.Property(e => e.Address)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FullName)
                    .HasMaxLength(81)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Ibnnumber)
                    .HasColumnName("IBNNumber")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LockedOn).HasColumnType("datetime");

                entity.Property(e => e.ModeOfCommunication)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PreferedTimeZone)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryContactNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SecondaryContantNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.TemporaryPass)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwEvents>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_Events");

                entity.Property(e => e.Category).HasMaxLength(500);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventTo).HasColumnType("datetime");

                entity.Property(e => e.EventVenue).HasMaxLength(1000);

                entity.Property(e => e.FirstName).HasMaxLength(30);

                entity.Property(e => e.Glocation)
                    .HasColumnName("GLocation")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.GmapCode)
                    .IsRequired()
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.Icon)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                // Ghrabawy : Deleted IconUrl property from Events table, as it is not used anywhere.
                // We used the Icon property to store the Event Icon URL.
                //entity.Property(e => e.IconUrl)
                //    .HasMaxLength(76)
                //    .IsUnicode(false);

                entity.Property(e => e.LastName).HasMaxLength(30);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwGateKeeperData>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_GateKeeperData");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventTo).HasColumnType("datetime");

                entity.Property(e => e.EventVenue).HasMaxLength(1000);

                entity.Property(e => e.FullName).HasMaxLength(61);

                entity.Property(e => e.GmapCode)
                    .HasColumnName("GMapCode")
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwGateKeeperScheduled>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_GateKeeperScheduled");

                entity.Property(e => e.AsssignedOn).HasColumnType("datetime");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventTo).HasColumnType("datetime");

                entity.Property(e => e.EventVenue).HasMaxLength(1000);

                entity.Property(e => e.IsActive).HasColumnName("isActive");
            });

            modelBuilder.Entity<vwGuestInfo>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_GuestInfo");

                entity.Property(e => e.Address).HasMaxLength(1000);
                entity.Property(e => e.AdditionalText).HasMaxLength(500);
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");
                entity.Property(e => e.Cypertext)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(80)
                    .IsUnicode(false);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.MessageId)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.ModeOfCommunication)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.PrimaryContactNo).HasMaxLength(40);
                entity.Property(e => e.SecondaryContactNo)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Source)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.WaresponseTime).HasColumnType("datetime");
                entity.Property(e => e.WhatsappStatus)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.whatsappMessageId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.whatsappMessageImgId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.whatsappWatiEventLocationId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.ConguratulationMsgId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.WatiConguratulationMsgId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.ReminderMessageId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.ReminderMessageWatiId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.Response).HasMaxLength(400);
                entity.Property(e => e.ImgSentMsgId)
                    .HasMaxLength(40)
                    .IsUnicode(false);
                entity.Property(e => e.waMessageEventLocationForSendingToAll)
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<VwGuestList>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_GuestList");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventTo).HasColumnType("datetime");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.PrimarycontactNo).HasMaxLength(40);
            });

            modelBuilder.Entity<VwGuestReportWa>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_GuestReportWA");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.MessageId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryContactNo).HasMaxLength(40);

                entity.Property(e => e.Response)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.WaresponseTime)
                    .HasColumnName("WAResponseTime")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<VwMiagregationReport>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_MIAgregationReport");

                entity.Property(e => e.EventTitle)
                    .HasColumnName("eventTitle")
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<VwScanForCount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_ScanForCount");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventVenue).HasMaxLength(1000);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.Response)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResponseCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwScanHistory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_ScanHistory");

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventVenue).HasMaxLength(1000);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.Response)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResponseCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwScanHistoryLogs>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_ScanHistoryLogs");

                entity.Property(e => e.Category).HasMaxLength(500);

                entity.Property(e => e.EventCode)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.EventFrom).HasColumnType("datetime");

                entity.Property(e => e.EventTitle).HasMaxLength(400);

                entity.Property(e => e.EventTo).HasColumnType("datetime");

                entity.Property(e => e.GuestName).HasMaxLength(100);

                entity.Property(e => e.Icon)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Response)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResponseCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedBy)
                    .HasMaxLength(81)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedCode)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ScannedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwScannedInfo>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_ScannedInfo");

                entity.Property(e => e.AdditionalText).HasMaxLength(500);

                entity.Property(e => e.Address).HasMaxLength(1000);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Cypertext)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.ImageTime).HasColumnType("datetime");

                entity.Property(e => e.ImgSentMsgId)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.MessageId)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.ModeOfCommunication)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryContactNo).HasMaxLength(40);

                entity.Property(e => e.Response).HasMaxLength(50);

                entity.Property(e => e.SecondaryContactNo)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.TextTime).HasColumnType("datetime");

                entity.Property(e => e.WaresponseTime)
                    .HasColumnName("WAResponseTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.WhatsappStatus)
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwUsers>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Vw_Users");

                entity.Property(e => e.Address)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountNo)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FullName)
                    .HasMaxLength(81)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Ibnnumber)
                    .HasColumnName("IBNNumber")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LockedOn).HasColumnType("datetime");

                entity.Property(e => e.ModeOfCommunication)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PreferedTimeZone)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryContactNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SecondaryContantNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.TemporaryPass)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<WhatsappResponseLogs>(entity =>
            {
                entity.HasKey(e => e.WaId)
                    .HasName("PK__Whatsapp__81F5B101D6599776");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.RecepientNo)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Response).HasMaxLength(40);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Wakey)
                    .HasColumnName("WAKey")
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<BulkOperatorEvents>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.OperatorAssignedFrom)
                    .WithMany()
                    .HasForeignKey(e => e.OperatorAssignedFromId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.OperatorAssignedTo)
                    .WithMany()
                    .HasForeignKey(e => e.OperatorAssignedToId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.AssignedBy)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedById)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            OnModelCreatingPartial(modelBuilder);
        }
            
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
