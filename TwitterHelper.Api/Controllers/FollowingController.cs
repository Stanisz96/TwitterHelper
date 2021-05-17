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
        private readonly string rootPath;
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
            this.rootPath = this.hostingEnv.ContentRootPath;
            this.helper = helper;
        }

        [HttpGet("~/api/[controller]/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            //string userId = "1352246343939592192";
            this.twitterUtils.Configurate("oauth1", $"/users/{id}/following", Method.GET);

            List<string> parametersValue = await context.Parameters
                                    .Where(p => p.Selected == true && p.TwitterObjectId == 2)
                                    .Select(p => p.Value).ToListAsync();

            var x = parametersValue;

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("user.fields", parametersValue);

            IRestResponse response = this.twitterUtils.Client.Execute(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            string followingPath = Path.Combine(this.rootPath, $"Data\\following\\{id}");
            DirectoryInfo followingDirectory = Directory.CreateDirectory(followingPath);

            Users users = new Users(jsonResponse);
            var count = 0;

            foreach (User user in users.AllUsers)
            {
                string userPath = Path.Combine(followingPath, $"{users.UsersData.ElementAt(count)["id"]}");
                DirectoryInfo userDirectory = Directory.CreateDirectory(userPath);
                userDirectory.CreateSubdirectory("tweeted");
                userDirectory.CreateSubdirectory("retweeted");
                userDirectory.CreateSubdirectory("replied_to");
                userDirectory.CreateSubdirectory("quoted");

                string jsonData = users.UsersData.ElementAt(count).ToString(Formatting.Indented);

                string dataPath = Path.Combine(userPath, "data.json");
                File.WriteAllText(dataPath, jsonData);

                count += 1;
            }


            if (!jsonResponse.Any())
            {
                return new BadRequestResult();
            }

            return new JsonResult(id);
        }

        [HttpGet("~/api/[controller]/{id}/[action]")]
        public async Task<IActionResult> Tweets(string id)
        {
            string followingUsersDirPath = Path.Combine(this.rootPath, $"Data\\following\\{id}");

            List<string> parametersValue = await context.Parameters
                                .Where(p => p.Selected == true && p.TwitterObjectId == 4)
                                .Select(p => p.Value).ToListAsync();

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("tweet.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "100");
            this.twitterUtils.AddParameter("expansions", "referenced_tweets.id");

            string resultString = $"MAIN USER ID: {id}\n\n";

            foreach (string userDirPath in Directory.GetDirectories(followingUsersDirPath))
            {
                string subUserId = userDirPath.Remove(0, followingUsersDirPath.Length + 1);
                this.twitterUtils.Configurate("oauth1", $"/users/{subUserId}/tweets", Method.GET);

                int tweetsCount = 0;
                int count = 100;

                while (!(count < 100 || tweetsCount > 1000))
                {
                    IRestResponse response = this.twitterUtils.Client.Execute(this.twitterUtils.Request);
                    var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);


                    Tweets tweets = new Tweets(jsonResponse);

                    if (tweets.TweetsData is null || tweets.AllTweets is null)
                    {
                        System.Threading.Thread.Sleep(900);
                        break;
                    }


                    count = Int32.Parse(tweets.Meta.result_count);
                    tweetsCount += count;
                    
                    if (tweets.Meta.next_token is not null)
                    {
                        this.twitterUtils.AddParameter("pagination_token", tweets.Meta.next_token);
                    }

                    this.helper.SaveFollowingTweets(tweets, userDirPath);

                    System.Threading.Thread.Sleep(900);
                }

                resultString += $"UserID: {userDirPath} ||| Collected Tweets: {tweetsCount}\n";
            }

            return new JsonResult(resultString);
        }
    }
}
