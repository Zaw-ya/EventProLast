using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class DefaultWhatsappSettings
    {
        [Key]
        public int Id { get; set; }
        public string MessageTextBox { get; set; }
        public string SendMessageButton { get; set; }
        public string MediaOptions { get; set; }
        public string ImageOption { get; set; }
        public string SendImageButton { get; set; }
        public string ImageTextBox { get; set; }
        public string VideoTextBox { get; set; }
        public string AddNewChatButton { get; set; }
        public string SearchNewChatButton { get; set; }
        public string NewContactButton { get; set; }
    }
}
