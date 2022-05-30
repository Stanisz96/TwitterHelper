using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitterHelper.Api.Data;
using TwitterHelper.Api.Models;

namespace TwitterHelper.Api.Tools
{
    public class Helper : IHelper
    {

        public string ToTwitterTimeStamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }


        public void SaveTweets(Tweets tweets, string userId, out bool shouldContinue)
        {
            var countTweets = 0;
            shouldContinue = true;
            MetaData metaData = GetMetaData(userId);

            foreach (Tweet tweet in tweets.AllTweets)
            {
                if (tweet.Referenced_tweets is null)
                {
                    tweet.Referenced_tweets = new List<ReferencedTweet>
                    {
                        new ReferencedTweet() { Id = tweet.Id, Type = "tweeted" }
                    };
                }
                var convertedTweetTime = ConvertStringToDateTime(tweet.Created_at);
                metaData = this.UpdateMetaDataOrStopSavingTweets(metaData,
                                                                 tweet,
                                                                 out shouldContinue);

                if (!shouldContinue)
                    break;

                var tweetRefType = tweet.Referenced_tweets.FirstOrDefault().Type;
                string tweetTypePath = Path.Combine(Globals.USERS_PATH,$"{userId}\\tweets\\{tweetRefType}");
                string tweetDataPath = Path.Combine(tweetTypePath, $"{tweet.Id}.json");

                string jsonData = JsonConvert.SerializeObject(tweet);
                File.WriteAllText(tweetDataPath, jsonData);

                countTweets++;
            }

            SaveMetaData(userId, metaData);

        }

        public void WaitCalculatedTime(double limitReqPerMin, DateTime dateTimeReference)
        {
            double minDiffMSec = ((60 / limitReqPerMin) + 0.2) * 1000;
            double diffMSeconds = minDiffMSec - (DateTime.Now - dateTimeReference).TotalMilliseconds;
            double waitMSeconds = diffMSeconds < 0 ? 0 : diffMSeconds;
            if (waitMSeconds > 0)
                System.Threading.Thread.Sleep((int)waitMSeconds);
        }

        public async Task<List<string>> GetContextParameterValues(int twitterObjectId, TwitterContext twitterContext)
        {
            List<string> parametersValue = await twitterContext.Parameters
                        .Where(p => p.Selected == true && p.TwitterObjectId == twitterObjectId)
                        .Select(p => p.Value).ToListAsync();

            return parametersValue;
        }

        public void SaveUserData(string userId, string jsonResponse, string userType)
        {
            string userPath = Path.Combine(Globals.USERS_PATH, $"{userId}");
            string userDataPath = Path.Combine(userPath, "userData.json");
            string userMetaPath = Path.Combine(userPath, "metaData.json");

            DirectoryInfo userDirectory = Directory.CreateDirectory(userPath);
            userDirectory.CreateSubdirectory($"tweets\\tweeted");
            userDirectory.CreateSubdirectory($"tweets\\retweeted");
            userDirectory.CreateSubdirectory($"tweets\\replied_to");
            userDirectory.CreateSubdirectory($"tweets\\quoted");

            File.WriteAllText(userDataPath, jsonResponse);

            MetaData meta = new();
            meta.UserType = userType;
            string jsonMetaData = JsonConvert.SerializeObject(meta);

            File.WriteAllText(userMetaPath, jsonMetaData);

        }

        public bool IsUserIdDuplicate(string userId, string userType)
        {
            var usersListPath = Path.Combine(Globals.DATA_PATH, "usersList.dat");
            bool isUserIdDuplicate = false;

            if (File.Exists(usersListPath))
            {
                var usersListFile = File.ReadAllLines(usersListPath);
                var usersList = new List<string>(usersListFile);
                isUserIdDuplicate = usersList.Contains(string.Concat("A", userId)) ||
                    usersList.Contains(string.Concat("B", userId));
            }

            return isUserIdDuplicate;
        }
    
        public void SaveUserId(string userId, string userType)
        {
            var usersListPath = Path.Combine(Globals.DATA_PATH, "usersList.dat");

            if (!File.Exists(usersListPath))
            {
                using StreamWriter sw = File.CreateText(usersListPath);
                sw.WriteLine(string.Concat(userType, userId));
            }
            else
            {
                using StreamWriter sw = File.AppendText(usersListPath);
                sw.WriteLine(string.Concat(userType, userId));
            }
        }

        public DateTime ConvertStringToDateTime(string dateTimeString)
        {
            DateTime dateTime = DateTime.ParseExact(dateTimeString,
                                         "MM'/'dd'/'yyyy HH:mm:ss",
                                         null);
            return dateTime;
        }

        public MetaData GetMetaData(string userId)
        {
            string metaDataPath = Path.Combine(Globals.USERS_PATH, $"{userId}\\metaData.json");
            string jsonFile = File.ReadAllText(metaDataPath);
            MetaData metaData = JsonConvert.DeserializeObject<MetaData>(jsonFile);

            return metaData;
        }

        public void SaveMetaData(string userId, MetaData metadata)
        {
            string jsonMetaData = JsonConvert.SerializeObject(metadata);
            string metaDataPath = Path.Combine(Globals.USERS_PATH, $"{userId}\\metaData.json");
            File.WriteAllText(metaDataPath, jsonMetaData);
        }
        public MetaData UpdateMetaDataOrStopSavingTweets(MetaData metaData, Tweet tweet, out bool shouldSaveTweet)
        {
            var convertedTweetTime = ConvertStringToDateTime(tweet.Created_at);
            var tweetRefType = tweet.Referenced_tweets.FirstOrDefault().Type;
            shouldSaveTweet = true;

            if (tweetRefType == "retweeted" && metaData.UserType == "A")
            {
                if (DateTime.Compare(convertedTweetTime, metaData.OldestRetweetDate) < 0)
                    metaData.OldestRetweetDate = convertedTweetTime;
            }

            if (tweetRefType == "tweeted" && metaData.UserType == "B")
            {
                shouldSaveTweet = false;

                if (DateTime.Compare(convertedTweetTime, metaData.OldestTweetDate) < 0)
                    metaData.OldestTweetDate = convertedTweetTime;

                foreach (var follower in metaData.Followers)
                {
                    MetaData metaDataFollower = GetMetaData(follower);
                    if (DateTime.Compare(convertedTweetTime, metaDataFollower.OldestRetweetDate) > 0)
                    {
                        shouldSaveTweet = true;
                        break;
                    }
                }
            }

            return metaData;
        }

        public void SaveFollowingUserToUserMetaData(string followerId, string followingId)
        {
            MetaData metaDataFollower = GetMetaData(followerId);
            metaDataFollower.Following.Add(followingId);
            SaveMetaData(followerId, metaDataFollower);
        }

        public void SaveFollowerUserToUserMetaData(string followerId, string followingId)
        {
            MetaData Following = GetMetaData(followingId);
            Following.Followers.Add(followerId);
            SaveMetaData(followingId, Following);
        }

        public bool IsFollowerOldestRetweetOlderThenFollowingOldestTweet(string followerId, string followingId)
        {
            MetaData metaDataFollower = GetMetaData(followerId);
            MetaData metaDataFollowing = GetMetaData(followingId);
            var followerDate = metaDataFollower.OldestRetweetDate;
            var followingDate = metaDataFollowing.OldestTweetDate;

            return DateTime.Compare(followerDate, followingDate) < 0;
        }

        public (DateTime, DateTime) GetRandomTimeWindow(int windowLengthMinute)
        {
            int randomHour = new Random().Next(0, 23);
            int randomMinuteStart = new Random().Next(0, 59 - windowLengthMinute);
            int randomMinuteEnd = randomMinuteStart + windowLengthMinute;
            int randomSecond = new Random().Next(0, 59);

            DateTime startTime = new(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day - 1,
                        randomHour,
                        randomMinuteStart,
                        randomSecond);
            DateTime endTime = new(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day - 1,
                        randomHour,
                        randomMinuteEnd,
                        randomSecond);

            return (startTime, endTime);
        }
    }
}
