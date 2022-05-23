using System;
using System.Collections.Generic;

namespace TwitterHelper.Api.Models
{
    public class MetaData
    {
        public string UserType { get; set; }
        public DateTime OldestRetweetDate { get; set; }
        public DateTime OldestTweetDate { get; set; }
        public List<string> Languages { get; set; }
        public List<string> Following { get; set; }
        public List<string> Followers { get; set; }

        public MetaData()
        {
            this.OldestRetweetDate = DateTime.Now;
            this.OldestTweetDate = DateTime.Now;
            this.Languages = new List<string>();
            this.Following = new List<string>();
            this.Followers = new List<string>();
        }
    }
}
