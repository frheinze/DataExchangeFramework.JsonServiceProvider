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
using Sitecore.Services.Core.Diagnostics;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors
{
    [RequiredEndpointPlugins(typeof(JsonServiceEndpointSettings))]
    [RequiredPipelineStepPlugins(typeof(EndpointSettings), typeof(ResolveIdentifierSettings), typeof(ResolveObjectSettings), typeof(ReadJsonObjectsSettings))]
    public class ReadJsonObjectStepProcessor : BasePipelineStepWithEndpointAndIdentifierProcessor
    {
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
                    var synchronizationSettings = pipelineContext.GetSynchronizationSettings();
                    if (synchronizationSettings.Source == null)
                    {
                        logger.Error("Pipeline processing will abort because the pipeline context has no source assigned. (pipeline step: {0}, plugin: {1}, property: {2})", pipelineStep.Name, typeof(SynchronizationSettings).FullName, "Source");
                    }
                    else
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
            }
            if (!canProcess)
            {
                logger.Error("Pipeline processing will abort because the pipeline step cannot be processed. (pipeline step: {0})", pipelineStep.Name);
                pipelineContext.CriticalError = true;
            }
            return canProcess;
        }

        public override void Process(PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            var logger = pipelineContext.PipelineBatchContext.Logger;
            if (CanProcess(pipelineStep, pipelineContext))
            {
                var identifierValue = GetIdentifierValue(pipelineStep, pipelineContext);
                if (string.IsNullOrWhiteSpace(identifierValue))
                {
                    logger.Error("Pipeline step processing will abort because no identifier value was resolved. (pipeline step: {0})", (object)pipelineStep.Name);
                }
                else
                {
                    var readJsonObjectsSettings = pipelineStep.GetPlugin<ReadJsonObjectsSettings>();
                    var endpoint = pipelineStep.GetEndpointSettings().EndpointFrom;

                    JObject jObject = ReadJsonData(endpoint, pipelineContext, readJsonObjectsSettings.Api, identifierValue);

                    var resolvedObject = ExtractObject(readJsonObjectsSettings, logger, jObject);



                    SaveResolvedObject(pipelineStep, pipelineContext, resolvedObject);
                }
            }
        }

        protected virtual void SaveResolvedObject(PipelineStep pipelineStep, PipelineContext pipelineContext, JObject resolvedObject)
        {
            var resolveObjectSettings = pipelineStep.GetResolveObjectSettings();
            var synchronizationSettings = pipelineContext.GetSynchronizationSettings();

            var resolvedObjectLocation = resolveObjectSettings?.ResolvedObjectLocation;
            if (!string.IsNullOrWhiteSpace(resolvedObjectLocation)
                && resolvedObjectLocation.Equals("Pipeline Context Source"))
            {
                synchronizationSettings.Source = resolvedObject;
            }
            else
            {
                synchronizationSettings.Target = resolvedObject; //Default
            }
        }

        private JObject ReadJsonData(Endpoint endpoint, PipelineContext pipelineContext, string api, string identifier)
        {
            var logger = pipelineContext.PipelineBatchContext.Logger;
            var endpointSettings = endpoint.GetPlugin<JsonServiceEndpointSettings>();

            JObject jObject = null;
            try
            {
                using (var client = new JsonRequestService().GetHttpClient(endpointSettings))
                {
                    var response = client.GetAsync(api.Replace("{0}", identifier)).Result;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            jObject = response.Content.ReadAsAsync<JObject>().Result;
                            logger.Debug($"Object loaded (Identifier={identifier})");
                            break;
                        case HttpStatusCode.NotFound:
                            logger.Info($"Object not found (Identifier={identifier})");
                            break;
                        default:
                            logger.Warn($"Error reading object information: {(int)response.StatusCode}-{response.ReasonPhrase} (Identifier={identifier})");
                            pipelineContext.CriticalError = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal("Error accessing json service. (exception={0}, host={1}, protocol={2}, api={3})", ex.GetBaseException().Message, endpointSettings.Host, endpointSettings.Protocol, api);
                pipelineContext.CriticalError = true;
            }
            return jObject;
        }


        private JObject ExtractObject(ReadJsonObjectsSettings readJsonObjectsSettings, ILogger logger, JObject jObject)
        {
            if (jObject != null)
            {
                //select root node
                if (!string.IsNullOrEmpty(readJsonObjectsSettings?.RootJsonPath))
                {
                    try
                    {
                        jObject = jObject.SelectToken(readJsonObjectsSettings.RootJsonPath) as JObject;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error using '{readJsonObjectsSettings.RootJsonPath}': {ex.Message}");
                    }
                }
            }
            return jObject;
        }
    }
}