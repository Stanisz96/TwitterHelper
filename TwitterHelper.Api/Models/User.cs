using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string created_at { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public string public_metrics { get; set; }
        public string verified { get; set; }

    }
}
