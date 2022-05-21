using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterHelper.Api.Data;
using TwitterHelper.Api.Models;

namespace TwitterHelper.Api.Tools
{
    public interface IHelper
    {
        public string ToTwitterTimeStamp(DateTime dateTime);
        public void SaveTweets(Tweets tweets, string userDirPath);
        public void WaitCalculatedTime(double limitReqPerMin, DateTime dateTimeReference);
        public Task<List<string>> GetContextParameterValues(int twitterObjectId, TwitterContext twitterContext);
        public void SaveUserData(string rootPath, string id, string jsonResponse);


    }
}
