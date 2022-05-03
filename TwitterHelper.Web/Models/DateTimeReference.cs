using System;

namespace TwitterHelper.Web.Models
{
    public class DateTimeReference
    {
        public int Id { get; set; }
        public DateTime UsersLookupTime { get; set; }
        public DateTime TweetsLookupTime { get; set; }
        public DateTime TimelinesTime { get; set; }
        public DateTime FilteredStreamTime { get; set; }
        public DateTime FollowsTime { get; set; }

        public DateTimeReference()
        {
            this.UsersLookupTime = DateTime.Now;
            this.TweetsLookupTime = DateTime.Now;
            this.TimelinesTime = DateTime.Now;
            this.FilteredStreamTime = DateTime.Now;
            this.FollowsTime = DateTime.Now;
        }
    }
}
