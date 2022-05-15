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
    }
}
