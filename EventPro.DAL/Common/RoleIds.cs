namespace EventPro.DAL.Common
{
    public static class RoleIds
    {
        public const int Administrator = 1; // Expected by Seeds (order 0)
        public const int Client = 2;        // Expected by UserVM & Seeds (order 1)
        public const int GateKeeper = 3;    // Expected by UserVM, AssignGatekeeper, AdminController (ScanLogs)
        public const int Operator = 4;      // Expected by UserVM, AssignOperator, ReportsController
        public const int Agent = 5;         // Expected by UserVM
        public const int Supervisor = 6;    // Expected by UserVM
        public const int Accounting = 7;    // Expected by UserVM
    }
}
