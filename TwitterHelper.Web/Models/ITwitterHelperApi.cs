using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Web.Models
{
    public interface ITwitterHelperApi
    {
        public string BaseUrl { get; set; }

        public Task<string> GetRandomUser();
        public Task<string> GetUserTweets();
        public Task<string> GetUserFollowing();
    }
}
