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


        public void SaveTweets(Tweets tweets, string userDirPath)
        {
            var countTweets = 0;

            MetaData metaData = GetMetaData(userDirPath);


            foreach (Tweet tweet in tweets.AllTweets)
            {
                string jsonData = JsonConvert.SerializeObject(tweet);
                var tweetRef = tweets.TweetsData.ElementAt(countTweets)["referenced_tweets"];

                string tweetRefType = "";

                if (tweetRef is null)
                    tweetRefType = "tweeted";
                else
                    tweetRefType = tweetRef[0]["type"].ToString();

                var convertedTweetTime = ConvertStringToDateTime(tweet.Created_at);
                
                if (tweetRefType == "retweeted")
                {
                    if (DateTime.Compare(convertedTweetTime,metaData.OldestRetweetData) < 0)
                        metaData.OldestRetweetData = convertedTweetTime;
                }

                string tweetTypePath = Path.Combine(userDirPath, $"tweets\\{tweetRefType}");
                string dataPath = Path.Combine(tweetTypePath, $"{tweets.TweetsData.ElementAt(countTweets)["id"]}.json");

                File.WriteAllText(dataPath, jsonData);

                countTweets += 1;
            }

            UpdateMetaData(userDirPath, metaData);

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

        public void SaveUserData(string userPath, string id, string jsonResponse, string userType)
        {
            DirectoryInfo userDirectory = Directory.CreateDirectory(userPath);
            userDirectory.CreateSubdirectory($"tweets\\tweeted");
            userDirectory.CreateSubdirectory($"tweets\\retweeted");
            userDirectory.CreateSubdirectory($"tweets\\replied_to");
            userDirectory.CreateSubdirectory($"tweets\\quoted");

            string dataPath = Path.Combine(userPath, "userData.json");
            File.WriteAllText(dataPath, jsonResponse);

            string metaPath = Path.Combine(userPath, "metaData.json");
            MetaData meta = new();
            meta.UserType = userType;
            string jsonMetaData = JsonConvert.SerializeObject(meta);
            File.WriteAllText(metaPath, jsonMetaData);

        }

        public bool IsUserIdDuplicate(string id, string rootPath, string userType)
        {
            var usersListPath = Path.Combine(rootPath, $"Data\\usersList.dat");
            bool isUserIdDuplicate = false;

            if (File.Exists(usersListPath))
            {
                var usersListFile = File.ReadAllLines(usersListPath);
                var usersList = new List<string>(usersListFile);
                isUserIdDuplicate = usersList.Contains(string.Concat("A", id)) ||
                    usersList.Contains(string.Concat("B", id));
            }

            return isUserIdDuplicate;
        }
    
        public void SaveUserId(string id, string rootPath, string userType)
        {
            var usersListPath = Path.Combine(rootPath, $"Data\\usersList.dat");

            if (!File.Exists(usersListPath))
            {
                using StreamWriter sw = File.CreateText(usersListPath);
                sw.WriteLine(string.Concat(userType, id));
            }
            else
            {
                using StreamWriter sw = File.AppendText(usersListPath);
                sw.WriteLine(string.Concat(userType, id));
            }
        }

        public DateTime ConvertStringToDateTime(string dateTimeString)
        {
            DateTime dateTime;
            if (DateTime.TryParse(dateTimeString, out dateTime))
                return dateTime;
            return DateTime.MinValue;
        }

        public MetaData GetMetaData(string userDirPath)
        {
            string metaDataPath = Path.Combine(userDirPath, "metaData.json");
            string jsonFile = File.ReadAllText(metaDataPath);
            MetaData metaData = JsonConvert.DeserializeObject<MetaData>(jsonFile);

            return metaData;
        }

        public void UpdateMetaData(string userDirPath, MetaData metadata)
        {
            string jsonMetaData = JsonConvert.SerializeObject(metadata);
            string metaDataPath = Path.Combine(userDirPath, "metaData.json");
            File.WriteAllText(metaDataPath, jsonMetaData);
        }
    }
}
