using System;

namespace EventPro.Web.Models
{
    /// <summary>
    /// ViewModel for displaying error information to users
    /// Contains both user-friendly messages and technical details for debugging
    /// </summary>
    public class ErrorViewModel
    {
        #region Request Information

        /// <summary>
        /// Unique identifier for this request - used for tracking and support
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Whether to display the RequestId to the user
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// The URL path where the error occurred
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// HTTP Status Code (404, 500, etc.)
        /// </summary>
        public int StatusCode { get; set; }

        #endregion

        #region Error Details

        /// <summary>
        /// User-friendly error title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Technical error message from the exception
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Inner exception message if available
        /// </summary>
        public string InnerExceptionMessage { get; set; }

        /// <summary>
        /// Full stack trace for debugging (only shown in development)
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Exception type name
        /// </summary>
        public string ExceptionType { get; set; }

        #endregion

        #region Display Options

        /// <summary>
        /// Whether to show technical details (should be false in production)
        /// </summary>
        public bool ShowDetails { get; set; }

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets user-friendly title based on status code
        /// </summary>
        public string GetStatusTitle()
        {
            return StatusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Access Denied",
                404 => "Page Not Found",
                408 => "Request Timeout",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                504 => "Gateway Timeout",
                _ => "Error"
            };
        }

        /// <summary>
        /// Gets user-friendly message based on status code (Arabic)
        /// </summary>
        public string GetStatusMessageArabic()
        {
            return StatusCode switch
            {
                400 => "الطلب غير صالح. يرجى التحقق من البيانات المدخلة.",
                401 => "يجب تسجيل الدخول للوصول إلى هذه الصفحة.",
                403 => "ليس لديك صلاحية للوصول إلى هذه الصفحة.",
                404 => "الصفحة التي تبحث عنها غير موجودة.",
                408 => "انتهت مهلة الطلب. يرجى المحاولة مرة أخرى.",
                500 => "حدث خطأ في الخادم. يرجى المحاولة لاحقاً.",
                502 => "خطأ في البوابة. يرجى المحاولة لاحقاً.",
                503 => "الخدمة غير متاحة حالياً. يرجى المحاولة لاحقاً.",
                504 => "انتهت مهلة البوابة. يرجى المحاولة لاحقاً.",
                _ => "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً."
            };
        }

        /// <summary>
        /// Gets user-friendly message based on status code (English)
        /// </summary>
        public string GetStatusMessageEnglish()
        {
            return StatusCode switch
            {
                400 => "The request was invalid. Please check your input.",
                401 => "You need to sign in to access this page.",
                403 => "You don't have permission to access this page.",
                404 => "The page you're looking for doesn't exist.",
                408 => "The request timed out. Please try again.",
                500 => "Something went wrong on our end. Please try again later.",
                502 => "Bad gateway error. Please try again later.",
                503 => "Service is temporarily unavailable. Please try again later.",
                504 => "Gateway timeout. Please try again later.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        /// <summary>
        /// Gets icon class based on status code
        /// </summary>
        public string GetIconClass()
        {
            return StatusCode switch
            {
                401 or 403 => "fa-lock",
                404 => "fa-search",
                408 or 504 => "fa-clock",
                500 => "fa-server",
                502 or 503 => "fa-plug",
                _ => "fa-exclamation-triangle"
            };
        }

        /// <summary>
        /// Gets color theme based on status code
        /// </summary>
        public string GetColorTheme()
        {
            return StatusCode switch
            {
                400 => "warning",
                401 or 403 => "purple",
                404 => "info",
                500 => "danger",
                _ => "danger"
            };
        }

        #endregion
    }
}
