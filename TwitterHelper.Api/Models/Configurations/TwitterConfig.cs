namespace TwitterHelper.Api.Models.Configurations
{
    public class TwitterConfig
    {
        public ApiConfig ApiConfig { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
        public string BearerToken { get; set; }
    }
}
