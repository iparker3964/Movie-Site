using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Models.Settings
{
    public class MovieProSettings
    {
        public string TmDbApiKey { get; set; }
        public string DefaultBackdropSize { get; set; }
        public string DefaultPosterSize { get; set; }
        public string DefaultYoutubeKey { get; set; }
        public string DefaultCastImage { get; set; }
        public DefaultCollection DefaultCollection { get; set; }
        public DefaultCredentials DefaultCredentials { get; set; }
    }
}
