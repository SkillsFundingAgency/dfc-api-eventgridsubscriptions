using System.Collections.Generic;

namespace DFC.EventGridSubscriptions.Data.Models
{
    public class ApiAdvancedFilter
    {
        public string? Property { get; set; }

        public FilterTypeEnum Type { get; set; }

        public List<string>? Values { get; set; }
    }
}
