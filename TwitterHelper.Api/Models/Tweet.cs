using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class Tweet
    {
        public string author_id { get; set; }
        public string id { get; set; }
        public string text { get; set; }
        public string created_at { get; set; }
        public string type { get; set; }
    }
}
