// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class InvoiceDetails
    {
        public int Idid { get; set; }
        public int? InvoiceId { get; set; }
        public string Product { get; set; }
        public string NoFguest { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Total { get; set; }

        public virtual Invoices Invoice { get; set; }
    }
}
