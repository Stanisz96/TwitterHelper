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
        public void SaveTweets(Tweets tweets, string userId, out bool shouldContinue);
        public void WaitCalculatedTime(double limitReqPerMin, DateTime dateTimeReference);
        public Task<List<string>> GetContextParameterValues(int twitterObjectId, TwitterContext twitterContext);
        public void SaveUserData(string userId, string jsonResponse, string userType);
        public bool IsUserIdDuplicate(string userId, string userType);
        public DateTime ConvertStringToDateTime(string dateTimeString);
        public void SaveMetaData(string userDirPath, MetaData metadata);
        public void SaveUserId(string userId, string userType);
        public MetaData UpdateMetaDataOrStopSavingTweets(MetaData metaData, Tweet tweet, out bool shouldSaveTweet);
        public void SaveFollowingUserToUserMetaData(string userAId, string userBId);
        public void SaveFollowerUserToUserMetaData(string userAId, string userBId);
        public MetaData GetMetaData(string userId);

    }
}
