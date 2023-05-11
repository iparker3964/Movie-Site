using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Models.Settings
{
    public class QueryOptions
    {
        public string Language { get; set; }
        public string AppendToResponse { get; set; }
        public string Page { get; set; }
    }
}
