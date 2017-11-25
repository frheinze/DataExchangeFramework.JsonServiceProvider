using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Services
{
    public class JsonRequestService
    {
        public HttpClient GetHttpClient(JsonServiceEndpointSettings endpointSettings)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(endpointSettings.Protocol.ToLowerInvariant() + "://" + endpointSettings.Host)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Authentication
            if (!string.IsNullOrEmpty(endpointSettings.Scheme) && !string.IsNullOrEmpty(endpointSettings.Parameter))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(endpointSettings.Scheme, endpointSettings.Parameter);
            }

            return client;
        }
    }
}
