using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Created_at { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public UserPublicMetrics Public_metrics { get; set; }
        public string Verified { get; set; }

    }
}
