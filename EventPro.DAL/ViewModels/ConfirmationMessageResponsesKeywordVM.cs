using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPro.DAL.ViewModels
{
    public class ConfirmationMessageResponsesKeywordVM
    {
        public int? Id { get; set; }

        private string _keywordKey;
        [Required, MaxLength(100)]
        public string KeywordKey
        {
            get => _keywordKey;
            set => _keywordKey = value?.Trim();
        }

        private string _languageCode;
        [Required, MaxLength(20)]
        public string LanguageCode
        {
            get => _languageCode;
            set => _languageCode = value?.Trim();
        }

        private string _keywordValue;
        [Required, MaxLength(200)]
        public string KeywordValue
        {
            get => _keywordValue;
            set => _keywordValue = value?.Trim();
        }
    }
}
