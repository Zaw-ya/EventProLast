using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace EventPro.DAL.Extensions
{
    public class TimeOnlyConverter : ValueConverter<TimeOnly?, DateTime>
    {
        public TimeOnlyConverter() :
            base(timeOnly => timeOnly.ToDateTime()
            , dateTime => TimeOnly.FromDateTime(dateTime))
        {

        }


    }
}
