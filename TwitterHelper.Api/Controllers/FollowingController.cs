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

namespace TwitterHelper.Api.Controllers
{
    [ApiController]
    public class FollowingController
    {
        private readonly ITwitterUtils twitterUtils;
        private readonly IWebHostEnvironment hostingEnv;
        private readonly TwitterContext context;
        private readonly string rootPath;

        public FollowingController(ITwitterUtils twitterUtils, IWebHostEnvironment hostingEnv, TwitterContext context)
        {
            this.twitterUtils = twitterUtils;
            this.hostingEnv = hostingEnv;
            this.context = context;
            this.rootPath = this.hostingEnv.ContentRootPath;
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

            foreach (string userDirPath in Directory.GetDirectories(followingUsersDirPath))
            {
                string subUserId = userDirPath.Remove(0, followingUsersDirPath.Length + 1);
                this.twitterUtils.Configurate("oauth1", $"/users/{subUserId}/tweets", Method.GET);

                IRestResponse response = this.twitterUtils.Client.Execute(this.twitterUtils.Request);
                var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

                Tweets tweets = new Tweets(jsonResponse);
                if (tweets.TweetsData is null || tweets.AllTweets is null)
                {
                    continue;
                }
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

                System.Threading.Thread.Sleep(900);
            }

            return new JsonResult(id);
        }
    }
}
