
using System;

namespace EventPro.DAL.Extensions
{
    public static class HelperExtensions
    {
        public static DateTime ToDateTime(this TimeOnly? timeOnly)
        {
            var currentDate = DateTime.MinValue;
            return currentDate += timeOnly.GetValueOrDefault().ToTimeSpan();
        }
    }
}
