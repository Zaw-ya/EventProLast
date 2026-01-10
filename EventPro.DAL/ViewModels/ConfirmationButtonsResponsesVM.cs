using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{
    public class ConfirmationButtonsResponsesVM
    {
        public ConfirmationButtonsResponsesVM(ConfirmationMessageResponsesKeyword model)
        {
            Id = model.Id;
            KeywordKey = model.KeywordKey;
            LanguageCode = model.LanguageCode;
            KeywordValue = model.KeywordValue;
            CreatedBy = model.CreatedByUser.FirstName + " " + model.CreatedByUser.LastName;
            CreatedOn = model.CreatedOn.ToString();
            UpdatedBy = string.IsNullOrWhiteSpace(string.Concat(model.UpdatedByUser?.FirstName , " " , model.UpdatedByUser?.LastName))? "no data" :string.Concat(model.UpdatedByUser?.FirstName , " " , model.UpdatedByUser?.LastName) ;
            UpdatedOn = model.UpdatedOn?.ToString() ?? "no data";
        }


        public int Id { get; set; }
        public int counter { get; set; }
        public string KeywordKey { get; set; }
        public string LanguageCode { get; set; }
        public string KeywordValue { get; set; }
        public string CreatedOn { get; set; }
        public string? UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
