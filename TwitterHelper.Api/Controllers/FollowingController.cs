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
    public class FollowingController
    {
        private readonly ITwitterUtils twitterUtils;
        private readonly IWebHostEnvironment hostingEnv;
        private readonly TwitterContext context;
        private readonly IHelper helper;

        public FollowingController(
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
            //string userId = "1352246343939592192";
            this.twitterUtils.Configurate("oauth1", $"/users/{id}/following", Method.Get);

            List<string> parametersValue = await this.helper.GetContextParameterValues(2, this.context);

            if (parametersValue.Count != 0)
                this.twitterUtils.AddValuesForParameter("user.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "300");

            var refTime = await context.DateTimeReferences.FirstAsync();
            this.helper.WaitCalculatedTime(1, refTime.FollowsTime);

            RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            Users users = new (jsonResponse);
            var count = 0;

            if (users.AllUsers is null)
                return new JsonResult(id);

            foreach (User user in users.AllUsers)
            {
                bool isProtected = user.Protected;
                bool isUserIdDuplicate = this.helper.IsUserIdDuplicate(user.Id, "B");
                bool shouldSaveUser = true;

                if (isUserIdDuplicate)
                    shouldSaveUser = this.helper.IsFollowerOldestRetweetOlderThenFollowingOldestTweet(id, user.Id);
                if (isProtected)
                    shouldSaveUser = false;

                if (shouldSaveUser)
                {
                    string jsonData = JsonConvert.SerializeObject(user);

                    this.helper.SaveUserData(user.Id, jsonData, "B");
                    this.helper.SaveUserId(user.Id, "B");
                    this.helper.SaveFollowingUserToUserMetaData(id, user.Id);
                    this.helper.SaveFollowerUserToUserMetaData(id, user.Id);

                    count += 1;
                }
            }

            refTime.FollowsTime = DateTime.Now;

            context.Update(refTime);
            await context.SaveChangesAsync();

            if (!jsonResponse.Any())
                return new BadRequestResult();

            return new JsonResult(id);
        }

        [HttpGet("~/api/[controller]/{id}/[action]")]
        public async Task<IActionResult> Tweets(string id)
        {
            List<string> parametersValue = await this.helper.GetContextParameterValues(4, this.context);

            if (parametersValue.Count != 0)
                this.twitterUtils.AddValuesForParameter("tweet.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "100");
            this.twitterUtils.AddParameter("expansions", "referenced_tweets.id");

            int allTweetsCount = 0;

            MetaData metaData = this.helper.GetMetaData(id);

            foreach (string userId in metaData.Following)
            {
                MetaData metaDataFollowing = this.helper.GetMetaData(userId);
                this.twitterUtils.Configurate("oauth1", $"/users/{userId}/tweets", Method.Get);
                this.twitterUtils.RemoveParameter("pagination_token");
                this.twitterUtils.RemoveParameter("start_time");

                this.twitterUtils.AddParameter("start_time", 
                        this.helper.ToTwitterTimeStamp(metaDataFollowing.OldestTweetDate));

                int tweetsCount = 0;
                int count = 100;
                DateTimeReference refTime;

                while (!(tweetsCount >= 1500 || count == 0))
                {
                    refTime = await context.DateTimeReferences.FirstAsync();
                    this.helper.WaitCalculatedTime(100, refTime.TimelinesTime);

                    RestResponse response = await this.twitterUtils.Client.ExecuteAsync(this.twitterUtils.Request);
                    var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);


                    Tweets tweets = new (jsonResponse);

                    if (tweets.TweetsData is null || tweets.AllTweets is null)
                    {
                        refTime.TimelinesTime = DateTime.Now;

                        context.Update(refTime);
                        await context.SaveChangesAsync();
                        break;
                    }



                    count = Int32.Parse(tweets.Meta.Result_count);
                    tweetsCount += count;
                    allTweetsCount += tweetsCount;

                    if (tweets.Meta.Next_token is not null)
                        this.twitterUtils.AddParameter("pagination_token", tweets.Meta.Next_token);
                    else
                        count = 0;

                    this.helper.SaveTweets(tweets, userId, out bool shouldContinue);

                    refTime.TimelinesTime = DateTime.Now;

                    context.Update(refTime);
                    await context.SaveChangesAsync();

                    if (!shouldContinue)
                        break;
                }
            }

            return new JsonResult(allTweetsCount.ToString());
        }
    }
}
