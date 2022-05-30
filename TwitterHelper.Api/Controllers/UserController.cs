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
            this.helper = helper;
        }

        [HttpGet("~/api/[controller]/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (this.helper.IsUserIdDuplicate(id, "A"))
                return new JsonResult(null);

            //string userId = "1352246343939592192";
            this.twitterUtils.Configurate("oauth1", $"/users/{id}", Method.Get);

            List<string> parametersValue = await this.helper.GetContextParameterValues(1, this.context);

            if (parametersValue.Count != 0)
                this.twitterUtils.AddValuesForParameter("user.fields", parametersValue);

            var refTime = await context.DateTimeReferences.FirstAsync();
            this.helper.WaitCalculatedTime(20, refTime.UsersLookupTime);

            RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            bool isProtected = JObject.Parse(jsonResponse)["data"].ToObject<User>().Protected;

            if (isProtected)
                return new JsonResult(null);

            this.helper.SaveUserData(id, jsonResponse, "A");
            this.helper.SaveUserId(id, "A");

            refTime.UsersLookupTime = DateTime.Now;

            context.Update(refTime);
            await context.SaveChangesAsync();

            if (!jsonResponse.Any())
                return new BadRequestResult();

            return new JsonResult(id);
        }

        [HttpGet("~/api/[controller]/{id}/[action]")]
        public async Task<string> Tweets(string id)
        {
            this.twitterUtils.Configurate("oauth1", $"/users/{id}/tweets", Method.Get);

            List<string> parametersValue = await this.helper.GetContextParameterValues(3, this.context);

            if (parametersValue.Count != 0)
                this.twitterUtils.AddValuesForParameter("tweet.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "100");
            this.twitterUtils.AddParameter("expansions", "referenced_tweets.id");



            int tweetsCount = 0;
            int count = 100;
            DateTimeReference refTime;

            while (!(tweetsCount >= 3000 || count == 0))
            {
                refTime = await context.DateTimeReferences.FirstAsync();
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

                count = Int32.Parse(tweets.Meta.Result_count);
                tweetsCount += count;

                if (tweets.Meta.Next_token is not null)
                    this.twitterUtils.AddParameter("pagination_token", tweets.Meta.Next_token);
                else
                    count = 0;

                this.helper.SaveTweets(tweets, id, out bool shouldContinue);

                refTime.TimelinesTime = DateTime.Now;

                context.Update(refTime);
                await context.SaveChangesAsync();

                if (!shouldContinue)
                    break;
            }

            return tweetsCount.ToString();
        }


        [HttpGet("~/api/[controller]/[action]")]
        public async Task<List<string>> Randoms()
        {
            List<string> userIdList = new();

            this.twitterUtils.Configurate("oauth1", $"/tweets/search/recent", Method.Get);

            List<string> parametersTweetsValue = await this.helper.GetContextParameterValues(3, this.context);

            (DateTime startTime, DateTime endTime) = this.helper.GetRandomTimeWindow(10);

            this.twitterUtils.AddQuery("lang:en the -the");
            this.twitterUtils.AddParameters(("start_time", this.helper.ToTwitterTimeStamp(startTime)),
                                            ("end_time", this.helper.ToTwitterTimeStamp(endTime)),
                                            ("max_results", "20"));

            if (parametersTweetsValue.Count != 0)
                this.twitterUtils.AddValuesForParameter("tweet.fields", parametersTweetsValue);

            var refTime = await context.DateTimeReferences.FirstAsync();
            this.helper.WaitCalculatedTime(12, refTime.TweetsSearchTime);

            RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            refTime.TweetsSearchTime = DateTime.Now;

            Tweets tweets = new(jsonResponse);

            int Result_count = Int32.Parse(tweets.Meta.Result_count);
            int randomTweet = new Random().Next(1, Result_count);
            var userIds = tweets.AllTweets.Select(tweet => tweet.Author_id).ToList();
            userIds = userIds.Distinct().ToList();
            int countEnglishTweets = 0;
            int countAllTweets = 0;

            foreach (string userId in userIds)
            {
                if (!this.helper.IsUserIdDuplicate(userId, "A"))
                {
                    this.twitterUtils.RemoveParameters();
                    this.twitterUtils.Configurate("oauth1", $"/users/{userId}/tweets", Method.Get);
                    this.twitterUtils.AddParameter("max_results", "50");

                    if (parametersTweetsValue.Count != 0)
                        this.twitterUtils.AddValuesForParameter("tweet.fields", parametersTweetsValue);

                    this.helper.WaitCalculatedTime(100, refTime.TimelinesTime);
                    response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
                    jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);
                    tweets = new(jsonResponse);
                    countEnglishTweets = tweets.AllTweets.Count(t => t.Lang == "en");
                    countAllTweets = tweets.AllTweets.Count();

                    refTime.TimelinesTime = DateTime.Now;

                    if (countEnglishTweets / countAllTweets > 0.5)
                        userIdList.Add(userId);
                }
            }

            context.Update(refTime);
            await context.SaveChangesAsync();

            return userIdList;
        }

        [HttpGet("~/api/[controller]/[action]")]
        public Task<string> Test()
        {
            //DateTimeReference dateTimeReference = new();
            //context.Update(dateTimeReference);
            //await context.SaveChangesAsync();

            return Task.FromResult(String.Empty);
        }
    }
}
