using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TwitterHelper.Web.Models
{
    public class TwitterHelperApi : ITwitterHelperApi
    {
        public string BaseUrl { get; set; }
        public HttpClient Client { get; set; }

        public TwitterHelperApi()
        {
            this.BaseUrl = "https://localhost:44324/";
            this.Client = new HttpClient();
        }


        public async Task<string> GetRandomUser()
        {
            this.Client.BaseAddress = new Uri(this.BaseUrl);
            this.Client.DefaultRequestHeaders.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //43932737
            //1352246343939592192
            HttpResponseMessage Response = await this.Client.GetAsync("api/User/43932737");

            if (Response.IsSuccessStatusCode)
            {
                var jsonResponse = Response.Content.ReadAsStringAsync().Result;
                return jsonResponse;
            }

            return null;
        }

        public async Task<string> GetUserTweets()
        {
            return null;
        }

        public async Task<string> GetUserFollowing()
        {
            return null;
        }

    }
}
