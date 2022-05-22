using System;
using System.Collections.Generic;

namespace TwitterHelper.Api.Models
{
    public class MetaData
    {
        public string UserType { get; set; }
        public DateTime OldestRetweetData { get; set; }
        public List<string> Languages { get; set; }
        public List<string> Following { get; set; }
        public List<string> Followers { get; set; }

        public MetaData()
        {
            this.OldestRetweetData = DateTime.Now;
            this.Languages = new List<string>();
            this.Following = new List<string>();
            this.Followers = new List<string>();
        }
    }
}
