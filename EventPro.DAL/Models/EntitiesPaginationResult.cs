using System.Collections.Generic;

namespace EventPro.DAL.Models
{
    public class EntityListResult<T> where T : class
    {
        public List<T> EntityList { get; set; }
        public long NoOfPages { get; set; }
    }
}
