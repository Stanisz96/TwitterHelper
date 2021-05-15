using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterHelper.Web.Tools
{
    public interface IHelper
    {
        public string ToTwitterTimeStamp(DateTime dateTime);
    }
}
