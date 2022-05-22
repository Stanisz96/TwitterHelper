using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TwitterHelper.Web.Data;
using TwitterHelper.Web.Models;
using TwitterHelper.Web.Tools;

namespace TwitterHelper.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly TwitterContext context;
        private readonly ITwitterHelperApi twitterHelperApi;
        private readonly IHelper helper;

        public HomeController(
                    ILogger<HomeController> logger,
                    TwitterContext context,
                    ITwitterHelperApi twitterHelperApi,
                    IHelper helper)
        {
            this.logger = logger;
            this.context = context;
            this.twitterHelperApi = twitterHelperApi;
            this.helper = helper;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Parameter> Parameters = await context.Parameters.ToListAsync();

            return View(Parameters);
        }

        public async Task<IActionResult> GetData()
        {
            List<string> userIds = await this.twitterHelperApi.GetRandomUsers();
            foreach (string userId in userIds)
            {
                var response = await this.twitterHelperApi.SaveUserData(userId);
                if (response is not null)
                {
                    await this.twitterHelperApi.SaveUserTweetsData(userId);
                    await this.twitterHelperApi.SaveFollowingData(userId);
                    await this.twitterHelperApi.SaveFollowingTweetsData(userId);
                }
            }
              return RedirectToAction("GetData");
        }


        [HttpPost]
        public async Task<IActionResult> SelectedParametersPartial(string[] dataString, int twitterObjectId)
        {
            List<int> dataArray = new();

            foreach (string str in dataString)
            {
                if (str == "true") dataArray.Add(1);
                else if (str == "false") dataArray.Add(0);
                else dataArray.Add(Int32.Parse(str));
            }

            bool isEven;
            List<Parameter> parameters = new();
            Parameter tempParameter;

            for (int i = 0; i < dataArray.Count; i++)
            {
                isEven = i % 2 == 0;

                if (isEven)
                {
                    tempParameter = await context.Parameters
                                    .FirstOrDefaultAsync(p => p.Id == dataArray[i]);
                    tempParameter.Selected = dataArray[i + 1] == 1;
                    parameters.Add(tempParameter);
                }

            }
            context.UpdateRange(parameters);
            await context.SaveChangesAsync();

            ViewData["TwitterObjectId"] = twitterObjectId;

            return PartialView(parameters);
        }


        public async Task<IActionResult> Test()
        {
            string baseUrl = "https://localhost:44324/";

            using var client = new HttpClient();

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage Res = await client.GetAsync("api/User/43932737");

            if (Res.IsSuccessStatusCode)
            {
                var jsonRes = Res.Content.ReadAsStringAsync().Result;
                return View();
            }

            return NotFound();
        }
    }
}
