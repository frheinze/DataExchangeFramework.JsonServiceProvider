using System;
using System.Net;
using System.Net.Http;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Services;
using Newtonsoft.Json.Linq;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors
{
    [RequiredEndpointPlugins(typeof(JsonServiceEndpointSettings))]
    public class ReadJsonObjectsStepProcessor : BaseReadDataStepProcessor
    {
        protected override void ReadData(Endpoint endpoint, PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            var logger = pipelineContext.PipelineBatchContext.Logger;

            var endpointSettings = endpoint.GetPlugin<JsonServiceEndpointSettings>();

            JToken jToken = null;
            try
            {
                using (var client = new JsonRequestService().GetHttpClient(endpointSettings))
                {
                    var response = client.GetAsync(endpointSettings.GetAll).Result;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            jToken = response.Content.ReadAsAsync<JToken>().Result;
                            break;
                        default:
                            logger.Warn($"Error accessing json service: {(int)response.StatusCode}-{response.ReasonPhrase} (url={client.BaseAddress.AbsolutePath}, api={endpointSettings.GetAll}, scheme={endpointSettings.Scheme}, parameter={endpointSettings.Parameter})");
                            pipelineContext.CriticalError = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal("Error accessing json service. (exception={0}, host={1}, protocol={2}, api={3})",
                             ex.GetBaseException().Message, endpointSettings.Host, endpointSettings.Protocol, endpointSettings.GetAll);
                pipelineContext.CriticalError = true;
            }

            //extract array
            var result = jToken == null ? new JArray() : jToken as JArray;
            if (result == null)
            {
                //select root node
                var readJsonObjectsSetting = pipelineStep.GetPlugin<ReadJsonObjectsSettings>();
                if (!string.IsNullOrEmpty(readJsonObjectsSetting?.RootJsonPath))
                {
                    try
                    {
                        result = jToken.SelectToken(readJsonObjectsSetting.RootJsonPath) as JArray;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error using '{readJsonObjectsSetting.RootJsonPath}': {ex.Message}");
                    }
                }
            }
            logger.Info("{0} json objects were read from endpoint. (pipeline step: {1}, endpoint: {2})", result?.Count ?? 0, pipelineStep.Name, endpoint.Name);

            //add the data that was read from the file to a plugin
            var dataSettings = new IterableDataSettings(result);

            //add the plugin to the pipeline context
            pipelineContext.Plugins.Add(dataSettings);
        }
    }
}