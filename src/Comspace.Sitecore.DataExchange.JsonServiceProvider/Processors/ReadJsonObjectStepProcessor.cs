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
using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors
{
    [RequiredEndpointPlugins(typeof(JsonServiceEndpointSettings))]
    [RequiredPipelineStepPlugins(typeof(ResolveIdentifierSettings))]
    public class ReadJsonObjectStepProcessor : BasePipelineStepWithEndpointAndIdentifier
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
                    logger.Error("Pipeline processing will abort because the pipeline step is missing an endpoint to read from. (pipeline step: {0}, plugin: {1}, property: {2})", (object)pipelineStep.Name, (object)typeof(EndpointSettings).FullName, (object)"EndpointFrom");
                }
                else if (IsEndpointValid(endpointFrom, pipelineStep, pipelineContext))
                {
                    var synchronizationSettings = pipelineContext.GetSynchronizationSettings();
                    if (!(synchronizationSettings.Source is ItemModel))
                    {
                        logger.Error("Pipeline processing will abort because the pipeline context has no (valid) source assigned. (pipeline step: {0}, plugin: {1}, property: {2})", (object)pipelineStep.Name, (object)typeof(SynchronizationSettings).FullName, (object)"Source");
                    }
                    else
                    {
                        var jsonServiceSettings = endpointFrom.GetPlugin<JsonServiceEndpointSettings>();
                        if (jsonServiceSettings.Host == null)
                        {
                            logger.Error("No 'Host' is specified on the endpoint. (pipeline step: {0}, endpoint: {1})", pipelineStep.Name, endpointFrom.Name);
                        }
                        else if (jsonServiceSettings.GetById == null)
                        {
                            logger.Error("No 'GetById' is specified on the endpoint. (pipeline step: {0}, endpoint: {1})", pipelineStep.Name, endpointFrom.Name);
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

        public override void Process(PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            ILogger logger = pipelineContext.PipelineBatchContext.Logger;
            if (CanProcess(pipelineStep, pipelineContext))
            {
                string identifierValue = GetIdentifierValue(pipelineStep, pipelineContext);
                if (string.IsNullOrWhiteSpace(identifierValue))
                {
                    logger.Error("Pipeline step processing will abort because no identifier value was resolved. (pipeline step: {0})", (object)pipelineStep.Name);
                }
                else
                {
                    JObject resolvedObject = ResolveObject(identifierValue, pipelineStep.GetEndpointSettings().EndpointFrom, pipelineStep, pipelineContext);

                    SynchronizationSettings synchronizationSettings = pipelineContext.GetSynchronizationSettings();
                    synchronizationSettings.Target = resolvedObject;
                }
            }
        }

        protected virtual JObject ResolveObject(string identifier, Endpoint endpoint, PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            var logger = pipelineContext.PipelineBatchContext.Logger;

            var endpointSettings = endpoint.GetPlugin<JsonServiceEndpointSettings>();

            JObject result = null;
            try
            {
                using (var client = new JsonRequestService().GetHttpClient(endpointSettings))
                {
                    var response = client.GetAsync(endpointSettings.GetById.Replace("{0}", identifier)).Result;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            result = response.Content.ReadAsAsync<JObject>().Result;
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
                logger.Fatal("Error accessing json service. (exception={0}, host={1}, protocol={2}, api={3})",
                             ex.GetBaseException().Message, endpointSettings.Host, endpointSettings.Protocol, endpointSettings.GetById);
                pipelineContext.CriticalError = true;
            }
            return result;
        }
    }
}