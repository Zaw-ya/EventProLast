using System.Collections.Generic;

namespace EventPro.DAL.Models
{
    public class MessageRequestTokens
    {
        public int EventID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public List<string> Tokens { get; set; }
    }
}