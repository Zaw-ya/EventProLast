namespace EventPro.DAL.Models
{
    public class ValidateQRCodeResult
    {
        public bool succeed { get; set; }
        public string message { get; set; }
        public string Name { get; set; }
        public int No { get; set; }
        public int Scanned { get; set; }
    }
}
