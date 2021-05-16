using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class TweetsMeta
    {
        public string result_count { get; set; }
        public string next_token { get; set; }
    }
}
