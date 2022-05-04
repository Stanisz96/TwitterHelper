using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Web.Models
{
    public interface ITwitterHelperApi
    {
        public string BaseUrl { get; set; }

        public Task<string> GetRandomUsers();
        public Task<string> SaveUserData(string id);
        public Task<string> SaveUserTweetsData(string id);
        public Task<string> SaveFollowingData(string id);
        public Task<string> SaveFollowingTweetsData(string id);
    }
}
