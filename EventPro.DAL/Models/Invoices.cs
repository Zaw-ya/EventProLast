using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class Invoices
    {
        public Invoices()
        {
            InvoiceDetails = new HashSet<InvoiceDetails>();
        }

        public int Id { get; set; }
        public int? EventId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string BillTo { get; set; }
        public string BillingAddress { get; set; }
        public string BillingContactNo { get; set; }
        public string EventCode { get; set; }
        public string EventLocation { get; set; }
        public string EventName { get; set; }
        public string EventPlace { get; set; }
        public decimal? TaxPer { get; set; }
        public decimal? TotalDue { get; set; }
        public decimal? NetDue { get; set; }
        public bool? IsPublished { get; set; }

        public virtual Events Event { get; set; }
        public virtual ICollection<InvoiceDetails> InvoiceDetails { get; set; }
    }
}
