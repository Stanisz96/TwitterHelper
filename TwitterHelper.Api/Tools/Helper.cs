using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            int diffMSeconds = (int)minDiffMSec - (DateTime.Now - dateTimeReference).Milliseconds;
            int waitMSeconds = diffMSeconds < 0 ? 0 : diffMSeconds;
            if (waitMSeconds > 0)
                System.Threading.Thread.Sleep(waitMSeconds);
        }
    }
}
