using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class Users
    {
        public Users(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jUsers = jObject["data"];
            UsersData = jUsers.ToObject<List<JToken>>();
            AllUsers = jUsers.ToObject<List<User>>();
        }

        public IEnumerable<User> AllUsers { get; set; }
        public List<JToken> UsersData{ get; set; }
    }
}
