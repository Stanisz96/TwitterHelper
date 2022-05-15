using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            this.Client.BaseAddress = new Uri(this.BaseUrl);
            this.Client.DefaultRequestHeaders.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.Client.Timeout = TimeSpan.FromHours(12);

        }


        public async Task<List<string>> GetRandomUsers()
        {
            HttpResponseMessage Response = await this.Client.GetAsync("api/User/Randoms");

            if (Response.IsSuccessStatusCode)
            {
                string jsonResponse = Response.Content.ReadAsStringAsync().Result;

                var list = jsonResponse.TrimStart('[')
                            .TrimEnd(']')
                            .Split(new[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                return list;
            }

            return null;
        }

        public async Task<string> SaveUserData(string id)
        {
            HttpResponseMessage Response = await this.Client.GetAsync($"api/User/{id}");

            if (Response.IsSuccessStatusCode)
            {
                var jsonResponse = Response.Content.ReadAsStringAsync().Result;
                if (jsonResponse == "null")
                    return null;
                return jsonResponse;
            }

            return null;
        }

        public async Task<string> SaveUserTweetsData(string id)
        {
            HttpResponseMessage Response = await this.Client.GetAsync($"api/User/{id}/Tweets");

            if (Response.IsSuccessStatusCode)
            {
                var jsonResponse = Response.Content.ReadAsStringAsync().Result;
                return jsonResponse;
            }

            return null;
        }

        public async Task<string> SaveFollowingData(string id)
        {
            HttpResponseMessage Response = await this.Client.GetAsync($"api/Following/{id}");

            if (Response.IsSuccessStatusCode)
            {
                var jsonResponse = Response.Content.ReadAsStringAsync().Result;
                return jsonResponse;
            }

            return null;
        }

        public async Task<string> SaveFollowingTweetsData(string id)
        {
            HttpResponseMessage Response = await this.Client.GetAsync($"api/Following/{id}/Tweets");

            if (Response.IsSuccessStatusCode)
            {
                var jsonResponse = Response.Content.ReadAsStringAsync().Result;
                return jsonResponse;
            }

            return null;
        }

    }
}
