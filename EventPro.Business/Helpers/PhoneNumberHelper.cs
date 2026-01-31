using System.Text.RegularExpressions;

namespace EventPro.Business.Helpers
{
    /// <summary>
    /// Helper class for phone number validation and normalization.
    /// Supports Egypt, Saudi Arabia, Kuwait, and Bahrain phone number formats.
    /// </summary>
    public static class PhoneNumberHelper
    {
        // Country codes
        public const string EGYPT_CODE = "20";
        public const string SAUDI_CODE = "966";
        public const string KUWAIT_CODE = "965";
        public const string BAHRAIN_CODE = "973";

        /// <summary>
        /// Normalizes a phone number by removing invalid characters and fixing common issues.
        /// Handles:
        /// - Duplicate + signs (++ -> +)
        /// - Multiple leading zeros
        /// - Spaces, dashes, and other separators
        /// - Removes country code if present at the beginning
        /// </summary>
        /// <param name="phoneNumber">The phone number to normalize</param>
        /// <param name="countryCode">The country code (without +)</param>
        /// <returns>Normalized local phone number (digits only, WITHOUT country code)</returns>
        public static string NormalizePhoneNumber(string phoneNumber, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(countryCode))
                countryCode = string.Empty;

            // Remove all non-digit characters except + at the beginning
            var cleanPhone = phoneNumber.Trim();

            // Fix duplicate + signs (++, +++, etc. -> single +)
            while (cleanPhone.Contains("++"))
            {
                cleanPhone = cleanPhone.Replace("++", "+");
            }

            // Remove + sign if present
            cleanPhone = cleanPhone.TrimStart('+');

            // Remove all non-digit characters
            cleanPhone = Regex.Replace(cleanPhone, @"[^\d]", "");

            // Clean the country code too
            countryCode = countryCode.Trim().TrimStart('+');
            countryCode = Regex.Replace(countryCode, @"[^\d]", "");

            // If the phone number starts with the country code, remove it
            if (!string.IsNullOrEmpty(countryCode) && cleanPhone.StartsWith(countryCode))
            {
                cleanPhone = cleanPhone.Substring(countryCode.Length);
            }

            // Handle country-specific local number formats (remove leading zeros, etc.)
            cleanPhone = NormalizeLocalNumber(cleanPhone, countryCode);

            return cleanPhone;
        }

        /// <summary>
        /// Normalizes a local phone number based on the country code.
        /// Removes leading zeros appropriately for each country.
        /// </summary>
        private static string NormalizeLocalNumber(string localNumber, string countryCode)
        {
            if (string.IsNullOrEmpty(localNumber))
                return localNumber;

            switch (countryCode)
            {
                case EGYPT_CODE:
                    // Egypt: Local numbers start with 0 (e.g., 010, 011, 012, 015)
                    // Remove the leading 0 when adding country code
                    // 010xxxxxxxx -> 10xxxxxxxx (then becomes 2010xxxxxxxx)
                    if (localNumber.StartsWith("0"))
                    {
                        localNumber = localNumber.TrimStart('0');
                        // For Egypt mobile, after removing leading 0, should start with 1
                        // If it starts with 0 again (like 00), keep removing
                        while (localNumber.StartsWith("0"))
                        {
                            localNumber = localNumber.Substring(1);
                        }
                    }
                    break;

                case SAUDI_CODE:
                    // Saudi Arabia: Local mobile numbers start with 05 (e.g., 05xxxxxxxx)
                    // Remove the leading 0 when adding country code
                    // 05xxxxxxxx -> 5xxxxxxxx (then becomes 9665xxxxxxxx)
                    if (localNumber.StartsWith("0"))
                    {
                        localNumber = localNumber.TrimStart('0');
                    }
                    break;

                case KUWAIT_CODE:
                    // Kuwait: No leading 0 for local numbers
                    // Numbers start directly with 5, 6, or 9
                    // But if user entered with 0, remove it
                    if (localNumber.StartsWith("0"))
                    {
                        localNumber = localNumber.TrimStart('0');
                    }
                    break;

                case BAHRAIN_CODE:
                    // Bahrain: No leading 0 for local numbers
                    // Mobile numbers start with 3 or 6
                    if (localNumber.StartsWith("0"))
                    {
                        localNumber = localNumber.TrimStart('0');
                    }
                    break;

                default:
                    // For unknown countries, just remove leading zeros
                    if (localNumber.StartsWith("0"))
                    {
                        localNumber = localNumber.TrimStart('0');
                    }
                    break;
            }

            return localNumber;
        }

        /// <summary>
        /// Validates if a local phone number is valid for the specified country.
        /// </summary>
        /// <param name="phoneNumber">The normalized local phone number (WITHOUT country code)</param>
        /// <param name="countryCode">The country code</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidPhoneNumber(string phoneNumber, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Clean the phone number
            var cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");
            countryCode = Regex.Replace(countryCode ?? "", @"[^\d]", "");

            switch (countryCode)
            {
                case EGYPT_CODE:
                    return IsValidEgyptPhone(cleanPhone);
                case SAUDI_CODE:
                    return IsValidSaudiPhone(cleanPhone);
                case KUWAIT_CODE:
                    return IsValidKuwaitPhone(cleanPhone);
                case BAHRAIN_CODE:
                    return IsValidBahrainPhone(cleanPhone);
                default:
                    // For unknown countries, just check minimum length
                    return cleanPhone.Length >= 8;
            }
        }

        /// <summary>
        /// Validates Egypt local phone number (without country code).
        /// Format: 10/11/12/15 + 8 digits = 10 digits total
        /// Example: 1012345678
        /// </summary>
        private static bool IsValidEgyptPhone(string phone)
        {
            // Total length should be 10 digits
            if (phone.Length != 10)
                return false;

            // Should start with 1 (10, 11, 12, 15)
            if (!phone.StartsWith("1"))
                return false;

            // Valid prefixes: 10, 11, 12, 15
            var prefix = phone.Substring(0, 2);
            var validPrefixes = new[] { "10", "11", "12", "15" };
            return validPrefixes.Contains(prefix);
        }

        /// <summary>
        /// Validates Saudi Arabia local phone number (without country code).
        /// Format: 5 + 8 digits = 9 digits total
        /// Example: 512345678
        /// </summary>
        private static bool IsValidSaudiPhone(string phone)
        {
            // Total length should be 9 digits
            if (phone.Length != 9)
                return false;

            // Mobile numbers start with 5
            return phone.StartsWith("5");
        }

        /// <summary>
        /// Validates Kuwait local phone number (without country code).
        /// Format: 8 digits total
        /// Mobile numbers start with 5, 6, or 9
        /// Example: 51234567
        /// </summary>
        private static bool IsValidKuwaitPhone(string phone)
        {
            // Total length should be 8 digits
            if (phone.Length != 8)
                return false;

            // Mobile numbers start with 5, 6, or 9
            var firstDigit = phone[0];
            return firstDigit == '5' || firstDigit == '6' || firstDigit == '9';
        }

        /// <summary>
        /// Validates Bahrain local phone number (without country code).
        /// Format: 8 digits total
        /// Mobile numbers start with 3 or 6
        /// Example: 32123456
        /// </summary>
        private static bool IsValidBahrainPhone(string phone)
        {
            // Total length should be 8 digits
            if (phone.Length != 8)
                return false;

            // Mobile numbers start with 3 or 6
            var firstDigit = phone[0];
            return firstDigit == '3' || firstDigit == '6';
        }

        /// <summary>
        /// Detects the country based on the country code.
        /// </summary>
        /// <param name="countryCode">Country code (with or without +)</param>
        /// <returns>Country name or "UNKNOWN"</returns>
        public static string DetectCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return "UNKNOWN";

            countryCode = Regex.Replace(countryCode.Trim().TrimStart('+'), @"[^\d]", "");

            return countryCode switch
            {
                EGYPT_CODE => "EGYPT",
                SAUDI_CODE => "SAUDI",
                KUWAIT_CODE => "KUWAIT",
                BAHRAIN_CODE => "BAHRAIN",
                _ => "UNKNOWN"
            };
        }

        /// <summary>
        /// Normalizes the country code by removing invalid characters.
        /// </summary>
        /// <param name="countryCode">Raw country code input</param>
        /// <returns>Clean country code (digits only)</returns>
        public static string NormalizeCountryCode(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return string.Empty;

            // Remove + signs (including duplicates) and any non-digit characters
            var clean = countryCode.Trim();

            // Fix duplicate + signs
            while (clean.Contains("++"))
            {
                clean = clean.Replace("++", "+");
            }

            clean = clean.TrimStart('+');
            return Regex.Replace(clean, @"[^\d]", "");
        }

        /// <summary>
        /// Validates and normalizes both phone number and country code together.
        /// Returns a result object with the normalized values and validation status.
        /// </summary>
        public static PhoneValidationResult ValidateAndNormalize(string phoneNumber, string countryCode)
        {
            var result = new PhoneValidationResult();

            // Normalize country code
            result.NormalizedCountryCode = NormalizeCountryCode(countryCode);

            // Normalize phone number
            result.NormalizedPhoneNumber = NormalizePhoneNumber(phoneNumber, result.NormalizedCountryCode);

            // Detect country
            result.Country = DetectCountry(result.NormalizedCountryCode);

            // Validate
            result.IsValid = IsValidPhoneNumber(result.NormalizedPhoneNumber, result.NormalizedCountryCode);

            return result;
        }
    }

    /// <summary>
    /// Result object for phone number validation and normalization.
    /// </summary>
    public class PhoneValidationResult
    {
        public string? NormalizedPhoneNumber { get; set; }
        public string? NormalizedCountryCode { get; set; }
        public string? Country { get; set; }
        public bool IsValid { get; set; }
    }
}
