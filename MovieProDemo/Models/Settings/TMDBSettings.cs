using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Models.Settings
{
    public class TMDBSettings
    {
        public string BaseUrl { get; set; }
        public string BaseImagePath { get; set; }
        public string BaseYouTubePath { get; set; }
        public QueryOptions QueryOptions { get; set; }
    }
}
