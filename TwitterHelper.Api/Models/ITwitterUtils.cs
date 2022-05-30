﻿using RestSharp;
using System.Collections.Generic;

namespace TwitterHelper.Api.Models
{
    public interface ITwitterUtils
    {
        public RestClient Client { get; set; }
        public RestRequest Request { get; set; }

        public void Configurate(string oauth, string resource, Method method);
        public void AddParameter(string parameterName, string parameterValue);
        public void AddValuesForParameter(string parameterName, List<string> parameterValues);
        public void AddParameters(params (string, string)[] parameters);
        public void RemoveParameters();
        public void RemoveParameter(string parameterName);
        public void AddQuery(string queryValue);
        public string GetLine(string fileName, int line);
    }
}
