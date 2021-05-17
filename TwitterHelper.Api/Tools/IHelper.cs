using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterHelper.Api.Models;

namespace TwitterHelper.Api.Tools
{
    public interface IHelper
    {
        public string ToTwitterTimeStamp(DateTime dateTime);
        public void SaveFollowingTweets(Tweets tweets, string userDirPath);
    }
}
