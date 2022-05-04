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
using System.Threading;
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
        public async Task<IActionResult> Get(string id)
        {
            //string userId = "1352246343939592192";
            this.twitterUtils.Configurate("oauth1", $"/users/{id}", Method.Get);

            List<string> parametersValue = await context.Parameters
                                    .Where(p => p.Selected == true && p.TwitterObjectId == 1)
                                    .Select(p => p.Value).ToListAsync();

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("user.fields", parametersValue);

            var refTime = await context.DateTimeReferences.FirstAsync();
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

            context.Update(refTime);
            await context.SaveChangesAsync();

            if (!jsonResponse.Any())
            {
                return new BadRequestResult();
            }

            return new JsonResult(refTime);
        }

        [HttpGet("~/api/[controller]/{id}/[action]")]
        public async Task<string> Tweets(string id)
        {
            this.twitterUtils.Configurate("oauth1", $"/users/{id}/tweets", Method.Get);

            List<string> parametersValue = await context.Parameters
                        .Where(p => p.Selected == true && p.TwitterObjectId == 3)
                        .Select(p => p.Value).ToListAsync();

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("tweet.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "100");
            this.twitterUtils.AddParameter("expansions", "referenced_tweets.id");


            string subPath = $"Data\\users\\{id}";
            string tweetsPath = Path.Combine(this.rootPath, subPath);

            int tweetsCount = 0;
            int count = 100;


            while (!(tweetsCount >= 10000))
            {
                var refTime = await context.DateTimeReferences.FirstAsync();
                this.helper.WaitCalculatedTime(100, refTime.TimelinesTime);

                RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
                var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

                Tweets tweets = new(jsonResponse);

                if (tweets.TweetsData is null || tweets.AllTweets is null)
                {
                    refTime.TimelinesTime = DateTime.Now;

                    context.Update(refTime);
                    await context.SaveChangesAsync();
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

                context.Update(refTime);
                await context.SaveChangesAsync();
            }

            return tweetsCount.ToString();
        }


        [HttpGet("~/api/[controller]/[action]")]
        public async Task<string> Random()
        {
            this.twitterUtils.Configurate("oauth1", $"/tweets/search/recent", Method.Get);

            List<string> parametersTweetsValue = await context.Parameters
                                    .Where(p => p.Selected == true && p.TwitterObjectId == 3)
                                    .Select(p => p.Value).ToListAsync();

            int randomHour = new Random().Next(0, 23);
            DateTime startTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, randomHour, 0, 0);
            DateTime endTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, randomHour + 1, 0, 0);

            this.twitterUtils.AddQuery("lang:en the -the");
            this.twitterUtils.AddParameter("start_time", this.helper.ToTwitterTimeStamp(startTime));
            this.twitterUtils.AddParameter("end_time", this.helper.ToTwitterTimeStamp(endTime));
            this.twitterUtils.AddParameter("max_results", "100");
            if (parametersTweetsValue.Count != 0)
                this.twitterUtils.AddParameters("tweet.fields", parametersTweetsValue);

            var refTime = await context.DateTimeReferences.FirstAsync();
            this.helper.WaitCalculatedTime(12, refTime.TweetsSearchTime);

            RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            refTime.TweetsSearchTime = DateTime.Now;

            Tweets tweets = new(jsonResponse);

            int result_count = Int32.Parse(tweets.Meta.result_count);
            int randomTweet = new Random().Next(1, result_count);
            var userIds = tweets.AllTweets.Select(tweet => tweet.Author_id).ToList();

            string foundedUserId = null;
            int countEnglishTweets = 0;
            int countAllTweets = 0;
            foreach (string userId in userIds)
            {
                this.twitterUtils.RemoveParameters();
                this.twitterUtils.Configurate("oauth1", $"/users/{userId}/tweets", Method.Get);
                this.twitterUtils.AddParameter("max_results", "100");
                if (parametersTweetsValue.Count != 0)
                    this.twitterUtils.AddParameters("tweet.fields", parametersTweetsValue);

                this.helper.WaitCalculatedTime(100, refTime.TimelinesTime);
                response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
                jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);
                tweets = new(jsonResponse);
                countEnglishTweets = tweets.AllTweets.Count(t => t.Lang == "en");
                countAllTweets = tweets.AllTweets.Count();

                refTime.TimelinesTime = DateTime.Now;

                if (countEnglishTweets / countAllTweets > 0.5)
                {
                    foundedUserId = userId;
                    break;
                }
            }

            context.Update(refTime);
            await context.SaveChangesAsync();

            return foundedUserId;
        }

        [HttpGet("~/api/[controller]/[action]")]
        public async Task<string> Test()
        {
            DateTimeReference dateTimeReference = new();
            context.Update(dateTimeReference);
            await context.SaveChangesAsync();

            return "Hello, I am Test Reqest :P.";
        }
    }
}
