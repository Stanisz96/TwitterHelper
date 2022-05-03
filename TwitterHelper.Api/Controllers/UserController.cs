using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitterHelper.Api.Data;
using TwitterHelper.Api.Models;
using TwitterHelper.Api.Tools;

namespace TwitterHelper.Api.Controllers
{
    [ApiController]
    public class UserController
    {
        private readonly ITwitterUtils twitterUtils;
        private readonly IWebHostEnvironment hostingEnv;
        private readonly TwitterContext context;
        private readonly string rootPath;
        private readonly IHelper helper;

        public UserController(
            ITwitterUtils twitterUtils,
            IWebHostEnvironment hostingEnv,
            TwitterContext context,
            IHelper helper)
        {
            this.twitterUtils = twitterUtils;
            this.hostingEnv = hostingEnv;
            this.context = context;
            this.rootPath = "D:\\Magisterka";
            this.helper = helper;
        }

        [HttpGet("~/api/[controller]/{id}")]
        public async Task<IActionResult> Get(string id, DateTimeReference refTime)
        {
            //string userId = "1352246343939592192";
            this.twitterUtils.Configurate("oauth1", $"/users/{id}", Method.Get);

            List<string> parametersValue = await context.Parameters
                                    .Where(p => p.Selected == true && p.TwitterObjectId == 1)
                                    .Select(p => p.Value).ToListAsync();

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("user.fields", parametersValue);

            this.helper.WaitCalculatedTime(20, refTime.UsersLookupTime);

            RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            string userPath = Path.Combine(this.rootPath, $"Data\\users\\{id}");
            DirectoryInfo userDirectory = Directory.CreateDirectory(userPath);
            userDirectory.CreateSubdirectory("tweeted");
            userDirectory.CreateSubdirectory("retweeted");
            userDirectory.CreateSubdirectory("replied_to");
            userDirectory.CreateSubdirectory("quoted");

            string followingPath = Path.Combine(this.rootPath, $"Data\\following\\{id}");
            DirectoryInfo followingDirectory = Directory.CreateDirectory(followingPath);

            string dataPath = Path.Combine(userPath, "data.json");
            File.WriteAllText(dataPath, jsonResponse);

            refTime.UsersLookupTime = DateTime.Now;

            if (!jsonResponse.Any())
            {
                return new BadRequestResult();
            }

            return new JsonResult(refTime);
        }

        [HttpGet("~/api/[controller]/{id}/[action]")]
        public async Task<string> Tweets(string id, DateTimeReference refTime)
        {
            this.twitterUtils.Configurate("oauth1", $"/users/{id}/tweets", Method.Get);

            List<string> parametersValue = await context.Parameters
                        .Where(p => p.Selected == true && p.TwitterObjectId == 3)
                        .Select(p => p.Value).ToListAsync();

            var x = parametersValue;

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("tweet.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "100");
            this.twitterUtils.AddParameter("expansions", "referenced_tweets.id");


            string subPath = $"Data\\users\\{id}";
            string tweetsPath = Path.Combine(this.rootPath, subPath);

            int tweetsCount = 0;
            int count = 100;

            while (!(tweetsCount >= 1000))
            {
                this.helper.WaitCalculatedTime(100, refTime.UsersLookupTime);

                RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
                var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

                Tweets tweets = new(jsonResponse);

                if (tweets.TweetsData is null || tweets.AllTweets is null)
                {
                    refTime.TimelinesTime = DateTime.Now;
                    break;
                }

                count = Int32.Parse(tweets.Meta.result_count);
                tweetsCount += count;

                if (tweets.Meta.next_token is not null)
                {
                    this.twitterUtils.AddParameter("pagination_token", tweets.Meta.next_token);
                }

                this.helper.SaveTweets(tweets, tweetsPath);

                refTime.TimelinesTime = DateTime.Now;
            }

            return tweetsCount.ToString();
        }


        [HttpGet("~/api/[controller]/[action]")]
        public async Task<string> Random()
        {
            this.twitterUtils.Configurate("oauth1", $"/tweets/search/recent", Method.Get);

            List<string> parametersValue = await context.Parameters
                                    .Where(p => p.Selected == true && p.TwitterObjectId == 3)
                                    .Select(p => p.Value).ToListAsync();

            int randomHour = new Random().Next(0, 23);
            DateTime startTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, randomHour, 0, 0);
            DateTime endTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, randomHour + 1, 0, 0);

            this.twitterUtils.AddQuery("lang:en the -the");
            this.twitterUtils.AddParameter("start_time", this.helper.ToTwitterTimeStamp(startTime));
            this.twitterUtils.AddParameter("end_time", this.helper.ToTwitterTimeStamp(endTime));
            this.twitterUtils.AddParameter("max_results", "100");
            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("tweet.fields", parametersValue);

            RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            Tweets tweets = new(jsonResponse);

            int result_count = Int32.Parse(tweets.Meta.result_count);
            int randomTweet = new Random().Next(1, result_count);
            var userId = JToken.Parse(response.Content)["data"][randomTweet]["author_id"].ToString();

            this.twitterUtils.RemoveParameters();
            this.twitterUtils.Configurate("oauth1", $"/users/by", Method.Get);
            this.twitterUtils.AddParameters("id", tweets.AllTweets.Select(tweet => tweet.Author_id).ToList());

            RestResponse response2 = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse2 = JToken.Parse(response2.Content).ToString(Formatting.Indented);

            return jsonResponse2;
        }

        [HttpGet("~/api/[controller]/[action]")]
        public string Test()
        {
            return "Hello, I am Test Reqest :P.";
        }
    }
}
