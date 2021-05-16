using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitterHelper.Api.Data;
using TwitterHelper.Api.Models;

namespace TwitterHelper.Api.Controllers
{
    [ApiController]
    public class UserController
    {
        private readonly ITwitterUtils twitterUtils;
        private readonly IWebHostEnvironment hostingEnv;
        private readonly TwitterContext context;
        private readonly string rootPath;

        public UserController(ITwitterUtils twitterUtils, IWebHostEnvironment hostingEnv, TwitterContext context)
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
            this.twitterUtils.Configurate("oauth1", $"/users/{id}", Method.GET);

            List<string> parametersValue = await context.Parameters
                                    .Where(p => p.Selected == true && p.TwitterObjectId == 1)
                                    .Select(p => p.Value).ToListAsync();

            var x = parametersValue;

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("user.fields", parametersValue);

            IRestResponse response = this.twitterUtils.Client.Execute(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            string userPath = Path.Combine(this.rootPath, $"Data\\users\\{id}");
            DirectoryInfo userDirectory = Directory.CreateDirectory(userPath);
            userDirectory.CreateSubdirectory("tweets");

            string followingPath = Path.Combine(this.rootPath, $"Data\\following\\{id}");
            DirectoryInfo followingDirectory = Directory.CreateDirectory(followingPath);

            string dataPath = Path.Combine(userPath, "data.json");
            File.WriteAllText(dataPath, jsonResponse);


            if (!jsonResponse.Any())
            {
                return new BadRequestResult();
            }

            return new JsonResult(id);
        }

        [HttpGet("~/api/[controller]/{id}/[action]")]
        public async Task<string> Tweets(string id)
        {
            this.twitterUtils.Configurate("oauth1", $"/users/{id}/tweets", Method.GET);

            List<string> parametersValue = await context.Parameters
                        .Where(p => p.Selected == true && p.TwitterObjectId == 3)
                        .Select(p => p.Value).ToListAsync();

            var x = parametersValue;

            if (parametersValue.Count != 0)
                this.twitterUtils.AddParameters("tweet.fields", parametersValue);

            this.twitterUtils.AddParameter("max_results", "100");

            IRestResponse response = this.twitterUtils.Client.Execute(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);

            Tweets tweets = new Tweets(jsonResponse);

            return jsonResponse;
        }


        [HttpGet("~/api/[controller]/[action]")]
        public string Test()
        {
            return "Hello, I am Test Reqest :P.";
        }
    }
}
