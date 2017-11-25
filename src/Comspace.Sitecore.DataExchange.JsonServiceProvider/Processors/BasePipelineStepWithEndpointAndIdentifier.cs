using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors
{
    /// <summary>
    /// Base class for pipeline steps with endpoint and identifier property.
    /// </summary>
    [RequiredPipelineStepPlugins(typeof(EndpointSettings), typeof(ResolveIdentifierSettings))]
    public abstract class BasePipelineStepWithEndpointAndIdentifier : BasePipelineStepWithEndpointsProcessor
    {
        protected virtual string GetIdentifierValue(PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            string result = null;
            var identifierObject = GetIdentifierObject(pipelineStep, pipelineContext);
            if (identifierObject != null)
            {
                var identifierSettings = pipelineStep.GetResolveIdentifierSettings();
                var valueReader = identifierSettings.IdentifierValueAccessor.ValueReader;
                result = Read(valueReader, identifierObject, pipelineStep, pipelineContext);

                var valueReaderForComparison = identifierSettings.ValueReaderToConvertIdentifierValueForComparison;
                if (valueReaderForComparison != null)
                {
                    result = Read(valueReaderForComparison, result, pipelineStep, pipelineContext);
                }
            }
            else
            {
                pipelineContext.Logger.Error("Unable to get identifier object. (pipeline step: {0})", pipelineStep.Name);
            }
            return result;
        }

        private string Read(IValueReader valueReader, object identifierObject, PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            string result = null;
            var logger = pipelineContext.PipelineBatchContext.Logger;

            if (!valueReader.CanRead(identifierObject, new DataAccessContext()).CanReadValue)
            {
                logger.Error("Cannot read value from identifier object. (pipeline step: {0})", pipelineStep.Name);
            }
            else
            {
                var readResult = valueReader.Read(identifierObject, new DataAccessContext());
                if (!readResult.WasValueRead)
                {
                    logger.Error("No value was read from identifier object. (pipeline step: {0})", pipelineStep.Name);
                }
                else
                {
                    result = readResult.ReadValue.ToString();
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
                var identifierSettings = pipelineStep.GetResolveIdentifierSettings(); //NOTE FHE RequiredPipelineStepPlugins 
                if (identifierSettings.IdentifierValueAccessor == null)
                {
                    logger.Error("Unable to get value accessor for identifier. (pipeline step: {0})", pipelineStep.Name);
                }
                else
                {
                    var identifierValueReader = identifierSettings.IdentifierValueAccessor.ValueReader;
                    if (identifierValueReader == null)
                    {
                        logger.Error("Unable to get value reader for identifier. (pipeline step: {0})", pipelineStep.Name);
                    }
                    else
                    {
                        canProcess = true;
                    }
                }
            }
            return canProcess;
        }

        protected object GetIdentifierObject(PipelineStep pipelineStep, PipelineContext pipelineContext)
        {
            var synchronizationSettings = pipelineContext.GetSynchronizationSettings();
            var identifierSettings = pipelineStep.GetResolveIdentifierSettings();

            switch (identifierSettings.IdentifierObjectLocation)
            {
                case "Pipeline Context Source":
                    return synchronizationSettings.Source;
                case "Pipeline Context Target":
                    return synchronizationSettings.Target;
            }
            return null;
        }
    }
}