using System;

namespace TwitterHelper.Api.Models
{
    public class Parameter
    {
        public int Id { get; set; }
        public String Value { get; set; }
        public String Text { get; set; }
        public bool Selected { get; set; }
        public int TwitterObjectId { get; set; }
    }
}
