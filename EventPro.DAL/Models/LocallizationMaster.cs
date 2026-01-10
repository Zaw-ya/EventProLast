// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class LocallizationMaster
    {
        public int Id { get; set; }
        public string LabelName { get; set; }
        public string RegionCode { get; set; }
        public string Translation { get; set; }
    }
}
