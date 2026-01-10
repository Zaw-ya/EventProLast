using System.Collections.Generic;

namespace EventPro.DAL.Common
{
    public class PinacleButtons
    {
        public int index { get; set; }
        public string type { get; set; }
    }
    public class PinacleMediaMessage
    {
        public string templateid { get; set; }
        public string url { get; set; }
        public string[] placeholders { get; set; }
    }
    public class PinacleMessage
    {
        public string templateid { get; set; }
        public string url { get; set; }
        public string[] placeholders { get; set; }
        public List<PinacleButtons> buttons { get; set; }
    }
    public class PinacleMediaBody
    {
        public string from { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public PinacleMediaMessage message { get; set; }
    }
    public class PinacleBody
    {
        public string from { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public PinacleMessage message { get; set; }

        public int gotomodule { get; set; }
    }
}
