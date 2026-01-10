// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class CardInfo
    {
        public int CardId { get; set; }
        public int? EventId { get; set; }
        public string BackgroundImage { get; set; }
        public double? BarcodeXaxis { get; set; }
        public double? BarcodeYaxis { get; set; }
        public int? BarcodeWidth { get; set; }
        public int? BarcodeHeight { get; set; }
        public string BarcodeColorCode { get; set; }
        public double? ContactNameXaxis { get; set; }
        public double? ContactNameYaxis { get; set; }
        public string FontName { get; set; }
        public double? FontSize { get; set; }
        public string FontColor { get; set; }
        public double? ContactNoXaxis { get; set; }
        public double? ContactNoYaxis { get; set; }
        public string ContactNoFontName { get; set; }
        public double? ContactNoFontSize { get; set; }
        public string ContactNoFontColor { get; set; }
        public double? AltTextXaxis { get; set; }
        public double? AltTextYaxis { get; set; }
        public string AltTextFontName { get; set; }
        public double? AltTextFontSize { get; set; }
        public string AltTextFontColor { get; set; }
        public double? Nosxaxis { get; set; }
        public double? Nosyaxis { get; set; }
        public string NosfontName { get; set; }
        public double? NosfontSize { get; set; }
        public string NosfontColor { get; set; }
        public string Status { get; set; }
        public int? CardWidth { get; set; }
        public int? CardHeight { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public bool? TransparentBackground { get; set; }
        public string DefaultFont { get; set; }
        public string SelectedPlaceHolder { get; set; }
        public string FontAlignment { get; set; }
        public string AddTextFontAlignment { get; set; }
        public string ContactNoAlignment { get; set; }
        public string NosAlignment { get; set; }
        public double? NameRightAxis { get; set; }
        public double? ContactRightAxis { get; set; }
        public double? NosRightAxis { get; set; }
        public double? AddTextRightAxis { get; set; }
        public bool? BarcodeBorder { get; set; }
        public bool? RightAlignment { get; set; }
        public string FontStyleName { get; set; }
        public string FontStyleMobNo { get; set; }
        public string FontStyleAddText { get; set; }
        public string FontStyleNos { get; set; }

        public virtual Events Event { get; set; }
    }
}
