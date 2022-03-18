using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class Tweet
    {
        public string Author_id { get; set; }
        public string Id { get; set; }
        public string Text { get; set; }
        public string Created_at { get; set; }
        public string Lang { get; set; }
        public string Attachments { get; set; }
        public string Entities { get; set; }
        public PublicMetrics Public_metrics { get; set; }
        public string Referenced_tweets { get; set; }

    }
}
