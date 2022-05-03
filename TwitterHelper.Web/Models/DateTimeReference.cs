using System;
using System.ComponentModel;

namespace TwitterHelper.Web.Models
{
    public class DateTimeReference
    {
        public int Id { get; set; }
        public DateTime UsersLookupTime 
        { 
            get
            {
                return this.usersLookupTime.HasValue
                    ? this.usersLookupTime.Value
                    : DateTime.Now;
            }
            
            set { this.usersLookupTime = value; } 
        }
        public DateTime TweetsLookupTime
        {
            get
            {
                return this.tweetsLookupTime.HasValue
                    ? this.tweetsLookupTime.Value
                    : DateTime.Now;
            }

            set { this.tweetsLookupTime = value; }
        }

        public DateTime TimelinesTime
        {
            get
            {
                return this.timelinesTime.HasValue
                    ? this.timelinesTime.Value
                    : DateTime.Now;
            }

            set { this.timelinesTime = value; }
        }
        public DateTime FilteredStreamTime
        {
            get
            {
                return this.filteredStreamTime.HasValue
                    ? this.filteredStreamTime.Value
                    : DateTime.Now;
            }

            set { this.filteredStreamTime = value; }
        }
        public DateTime FollowsTime
        {
            get
            {
                return this.followsTime.HasValue
                    ? this.followsTime.Value
                    : DateTime.Now;
            }

            set { this.followsTime = value; }
        }
         
        private DateTime? usersLookupTime = null; 
        private DateTime? tweetsLookupTime = null; 
        private DateTime? timelinesTime = null; 
        private DateTime? filteredStreamTime = null; 
        private DateTime? followsTime = null; 
    }
}
