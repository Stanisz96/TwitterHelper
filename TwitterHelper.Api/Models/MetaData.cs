using System;
using System.Collections.Generic;

namespace TwitterHelper.Api.Models
{
    public class MetaData
    {
        public string UserType { get; set; }
        public DateTime OldestAnyTweetDate { get; set; }
        public List<string> Following { get; set; }
        public List<string> Followers { get; set; }

        public MetaData()
        {
            this.OldestAnyTweetDate = DateTime.Now;
            this.Following = new List<string>();
            this.Followers = new List<string>();
        }
    }
}
