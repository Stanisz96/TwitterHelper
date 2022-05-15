using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace TwitterHelper.Web.Tools
{
    public class Helper : IHelper
    {

        public string ToTwitterTimeStamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }


        public void OpenFolder(string folderPath, string fileName)
        {
            try
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = folderPath,
                    FileName = fileName
                };

                Process.Start(startInfo);
            }
            catch (Exception)
            { throw; }
        }
    }
}
