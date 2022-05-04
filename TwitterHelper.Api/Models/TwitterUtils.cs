using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using TwitterHelper.Api.Models.Configurations;

namespace TwitterHelper.Api.Models
{
    public class TwitterUtils : ITwitterUtils
    {
        public RestClient Client { get; set; }
        public RestRequest Request { get; set; }
        public TwitterConfig TwitterConf { get; set; }
        public string KeyFileName { get; set; }

        public TwitterUtils()
        {
            this.KeyFileName = Path.Combine(Environment.CurrentDirectory, @"App_Data\", "keys.txt");

            TwitterConf = new TwitterConfig()
            {
                ApiConfig = new ApiConfig() { 
                    Uri = new Uri("https://api.twitter.com/2") 
                },
                ConsumerKey = GetLine(this.KeyFileName, 1),
                ConsumerSecret = GetLine(this.KeyFileName, 2),
                AccessToken = GetLine(this.KeyFileName, 4),
                AccessSecret = GetLine(this.KeyFileName, 5),
                BearerToken = GetLine(this.KeyFileName, 3)
            };

            Client = new RestClient(new RestClientOptions
            {
                Timeout = -1,
                BaseUrl = TwitterConf.ApiConfig.Uri
            });

            Request = new RestRequest();
        }


        public void Configurate(string oauth, string resource, Method method)
        {
            if (oauth == "oauth1")
            {
                this.Client.Authenticator = OAuth1Authenticator
                                .ForProtectedResource(
                                    consumerKey: TwitterConf.ConsumerKey,
                                    consumerSecret: TwitterConf.ConsumerSecret,
                                    accessToken: TwitterConf.AccessToken,
                                    accessTokenSecret: TwitterConf.AccessSecret);
            }
            if (oauth == "oauth2")
            {
                setBearerToken();
                this.Client.AddDefaultHeader(
                    name: "Authorization",
                    value: string.Format("Bearer {0}", this.TwitterConf.BearerToken));
            }
            this.Request.Resource = resource;
            this.Request.Method = method;
        }

        public void AddParameter(string parameterName, string parameterValue)
        {
            this.Request.AddOrUpdateParameter(parameterName, parameterValue);
        }

        public void AddQuery(string queryValue)
        {
            this.Request.AddOrUpdateParameter("query", queryValue, ParameterType.QueryString);
        }

        public void AddParameters(string parameterName, List<string> parameterValues)
        {
            var strValues = "";
            for (int i = 0; i < parameterValues.Count; i++)
            {
                strValues += parameterValues[i];
                if (i != parameterValues.Count - 1) strValues += ",";
            }

            this.Request.AddOrUpdateParameter(parameterName, strValues);
        }

        public void RemoveParameters()
        {
            foreach (RestSharp.Parameter parameter in 
                this.Request.Parameters.GetParameters(ParameterType.GetOrPost))
            {
                this.Request.Parameters.RemoveParameter(parameter);
            }

            foreach (RestSharp.Parameter parameter in
                this.Request.Parameters.GetParameters(ParameterType.QueryString))
            {
                this.Request.Parameters.RemoveParameter(parameter);
            }
        }

        private void setBearerToken()
        {
            string strBearerRequest = HttpUtility.UrlEncode(this.TwitterConf.ConsumerKey) +
                                        ":" + HttpUtility.UrlEncode(this.TwitterConf.ConsumerSecret);

            strBearerRequest = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(strBearerRequest));

            WebRequest request = WebRequest.Create("https://api.twitter.com/oauth2/token");
            request.Headers.Add("Authorization", "Basic " + strBearerRequest);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            string strRequestContent = "grant_type=client_credentials";
            byte[] bytearrayRequestContent = System.Text.Encoding.UTF8.GetBytes(strRequestContent);
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytearrayRequestContent, 0, bytearrayRequestContent.Length);
            requestStream.Close();

            string responseJson = string.Empty;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                responseJson = new StreamReader(responseStream).ReadToEnd();
            }

            JObject jobjectResponse = JObject.Parse(responseJson);

            this.TwitterConf.BearerToken = jobjectResponse["access_token"].ToString();
        }


        public string GetLine(string fileName, int line)
        {
            using (var sr = new StreamReader(fileName))
            {
                for (int i = 1; i < line; i++)
                    sr.ReadLine();
                return sr.ReadLine();
            }
        }
    }
}
