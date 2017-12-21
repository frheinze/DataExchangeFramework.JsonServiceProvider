using System;
using System.Net;
using System.Net.Http;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Services;
using Newtonsoft.Json.Linq;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.Services.Core.Diagnostics;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors
{
    [RequiredEndpointPlugins(typeof(JsonServiceEndpointSettings))]
    [RequiredPipelineStepPlugins(typeof(EndpointSettings), typeof(ReadJsonObjectsSettings))]
    public class ReadJsonObjectsStepProcessor : BaseReadDataStepProcessor
    {
        protected override void ReadData(Endpoint endpoint, PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            var logger = pipelineContext.PipelineBatchContext.Logger;
            var readJsonObjectsSettings = pipelineStep.GetPlugin<ReadJsonObjectsSettings>();

            //read data
            JToken jToken = ReadJsonData(endpoint, pipelineContext, readJsonObjectsSettings.Api);

            //extract array
            JArray result = ExtractArray(readJsonObjectsSettings, logger, jToken);
            logger.Info("{0} json objects were read from endpoint. (pipeline step: {1}, endpoint: {2})", result?.Count ?? 0, pipelineStep.Name, endpoint.Name);

            //add the data that was read to a plugin
            var dataSettings = new IterableDataSettings(result);

            //add the plugin to the pipeline context
            pipelineContext.Plugins.Add(dataSettings);
        }

        private JArray ExtractArray(ReadJsonObjectsSettings readJsonObjectsSettings, ILogger logger, JToken jToken)
        {
            var result = jToken == null ? new JArray() : jToken as JArray;
            if (result == null)
            {
                //select root node
                if (!string.IsNullOrEmpty(readJsonObjectsSettings?.RootJsonPath))
                {
                    try
                    {
                        result = jToken.SelectToken(readJsonObjectsSettings.RootJsonPath) as JArray;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error using '{readJsonObjectsSettings.RootJsonPath}': {ex.Message}");
                    }
                }
            }
            return result;
        }

        public override bool CanProcess(PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            var canProcess = false;
            var logger = pipelineContext.PipelineBatchContext.Logger;
            if (base.CanProcess(pipelineStep, pipelineContext))
            {
                var endpointSettings = pipelineStep.GetEndpointSettings();
                var endpointFrom = endpointSettings.EndpointFrom;
                if (endpointFrom == null)
                {
                    logger.Error("Pipeline processing will abort because the pipeline step is missing an endpoint to read from. (pipeline step: {0}, plugin: {1}, property: {2})", pipelineStep.Name, typeof(EndpointSettings).FullName, "EndpointFrom");
                }
                else if (IsEndpointValid(endpointFrom, pipelineStep, pipelineContext))
                {
                    var jsonServiceSettings = endpointFrom.GetPlugin<JsonServiceEndpointSettings>();
                    if (jsonServiceSettings.Host == null || jsonServiceSettings.Protocol == null)
                    {
                        logger.Error("No 'Host' or 'Protocol' is specified on the endpoint. (pipeline step: {0}, endpoint: {1})", pipelineStep.Name, endpointFrom.Name);
                    }
                    else
                    {
                        var readJsonObjectsSettings = pipelineStep.GetPlugin<ReadJsonObjectsSettings>();
                        if (readJsonObjectsSettings.Api == null)
                        {
                            logger.Error("No 'Api' is specified on the reader. (pipeline step: {0}, endpoint: {1})", pipelineStep.Name, endpointFrom.Name);
                        }
                        else
                        {
                            canProcess = true;
                        }
                    }
                }
            }
            if (!canProcess)
            {
                logger.Error("Pipeline processing will abort because the pipeline step cannot be processed. (pipeline step: {0})", pipelineStep.Name);
                pipelineContext.CriticalError = true;
            }
            return canProcess;
        }


        private JToken ReadJsonData(Endpoint endpoint, PipelineContext pipelineContext, string api)
        {
            var logger = pipelineContext.PipelineBatchContext.Logger;

            var endpointSettings = endpoint.GetPlugin<JsonServiceEndpointSettings>();

            JToken result = null;
            try
            {
                using (var client = new JsonRequestService().GetHttpClient(endpointSettings))
                {
                    var response = client.GetAsync(api).Result;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            result = response.Content.ReadAsAsync<JToken>().Result;
                            break;
                        default:
                            logger.Warn($"Error accessing json service: {(int)response.StatusCode}-{response.ReasonPhrase} (url={client.BaseAddress.AbsolutePath}, api={api}, scheme={endpointSettings.Scheme}, parameter={endpointSettings.Parameter})");
                            pipelineContext.CriticalError = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal("Error accessing json service. (exception={0}, host={1}, protocol={2}, api={3})",
                    ex.GetBaseException().Message, endpointSettings.Host, endpointSettings.Protocol, api);
                pipelineContext.CriticalError = true;
            }
            return result;
        }
    }
}