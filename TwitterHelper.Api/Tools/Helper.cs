﻿using Microsoft.EntityFrameworkCore;
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

            foreach (Tweet tweet in tweets.AllTweets)
            {
                string jsonData = JsonConvert.SerializeObject(tweet);
                var tweetRef = tweets.TweetsData.ElementAt(countTweets)["referenced_tweets"];

                string tweetRefType = "";

                if (tweetRef is null)
                {
                    tweetRefType = "tweeted";
                }
                else
                {
                    tweetRefType = tweetRef[0]["type"].ToString();
                }

                string tweetTypePath = Path.Combine(userDirPath, $"{tweetRefType}");

                string dataPath = Path.Combine(tweetTypePath, $"{tweets.TweetsData.ElementAt(countTweets)["id"]}.json");

                File.WriteAllText(dataPath, jsonData);

                countTweets += 1;
            }
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

        public void SaveUserData(string userPath, string id, string jsonResponse)
        {
            DirectoryInfo userDirectory = Directory.CreateDirectory(userPath);
            userDirectory.CreateSubdirectory($"tweets\\tweeted");
            userDirectory.CreateSubdirectory($"tweets\\retweeted");
            userDirectory.CreateSubdirectory($"tweets\\replied_to");
            userDirectory.CreateSubdirectory($"tweets\\quoted");

            string dataPath = Path.Combine(userPath, "userData.json");
            File.WriteAllText(dataPath, jsonResponse);
        }
    }
}
