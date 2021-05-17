using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Api.Models
{
    public class Tweets
    {
        public Tweets(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jTweets = jObject["data"];
            if (jTweets is not null)
            {
                TweetsData = jTweets.ToObject<List<JToken>>();
                AllTweets = jTweets.ToObject<List<Tweet>>();
                Meta = jObject["meta"].ToObject<TweetsMeta>();
            }
            else
            {
                TweetsData = null;
                AllTweets = null;
                Meta = null;

            }
        }

        public IEnumerable<Tweet> AllTweets { get; set; }
        public List<JToken> TweetsData { get; set; }
        public TweetsMeta Meta { get; set; }
        public int MyProperty { get; set; }
    }
}
