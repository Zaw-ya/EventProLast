using Microsoft.EntityFrameworkCore;

namespace EventPro.DAL.Models
{
    [Keyless]
    public class VMDashboardCount
    {
        public int CategoryCount { get; set; }
        public int EventsCount { get; set; }
        public int UsersCount { get; set; }
        public int GatekeeperCount { get; set; }
    }
}
