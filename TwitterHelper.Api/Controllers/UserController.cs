using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using TwitterHelper.Api.Models;

namespace TwitterHelper.Api.Controllers
{
    [ApiController]
    public class UserController
    {
        private readonly ITwitterUtils twitterUtils;

        public UserController(ITwitterUtils twitterUtils)
        {
            this.twitterUtils = twitterUtils;
        }

        [HttpGet("~/api/[controller]/{id}")]
        public IActionResult Get(int id)
        {
            //string userId = "43932737";

            this.twitterUtils.Configurate("oauth1", $"/users/{id}", Method.GET);

            /*            List<string> parametersValue = await context.Parameters
                                                .Where(p => p.Selected == true && p.TwitterObjectId == 1)
                                                .Select(p => p.Value).ToListAsync();*/


            /*            if (parametersValue.Count != 0)
                            this.twitterApi.AddParameters("user.fields", parametersValue);*/

            IRestResponse response = this.twitterUtils.Client.Execute(this.twitterUtils.Request);
            var jsonResponse = JToken.Parse(response.Content).ToString(Formatting.Indented);


            if (!jsonResponse.Any())
            {
                return new EmptyResult();
            }

            return new JsonResult(id);
        }




        [HttpGet("~/api/[controller]/[action]")]
        public string Test()
        {
            return "Hello, I am Test Reqest :P.";
        }
    }
}
