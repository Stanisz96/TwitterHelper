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
        public void SaveTweets(Tweets tweets, string userDirPath);
        public void WaitCalculatedTime(int limitReqPerMin, DateTime dateTimeReference);


    }
}
