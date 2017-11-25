using Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.PipelineSteps
{
    /// <summary>
    /// Converter for pipeline step <see cref="BasePipelineStepWithEndpointAndIdentifier"/>.
    /// </summary>
    public abstract class BasePipelineStepWithEndpointAndIdentifierConverter : BasePipelineStepConverter
    {
        protected BasePipelineStepWithEndpointAndIdentifierConverter(IItemModelRepository repository)
            : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            AddEndpointSettings(source, pipelineStep);
            AddIdentifierSettings(source, pipelineStep);
        }

        public virtual void AddEndpointSettings(ItemModel source, PipelineStep pipelineStep)
        {
            var endpointSettings = new EndpointSettings
            {
                EndpointFrom = ConvertReferenceToModel<Endpoint>(source, "EndpointFrom")
            };
            pipelineStep.Plugins.Add(endpointSettings);
        }

        public virtual void AddIdentifierSettings(ItemModel source, PipelineStep pipelineStep)
        {
            var identifierValueAccessor = ConvertReferenceToModel<IValueAccessor>(source, "IdentifierValueAccessor");
            var valueReaderToConvertIdentifierValueForComparison = ConvertReferenceToModel<IValueReader>(source, "ValueReaderToConvertIdentifierValueForComparison");
            var identifierSettings = new ResolveIdentifierSettings()
            {
                IdentifierObjectLocation = GetStringValue(source, "IdentifierObjectLocation"),
                IdentifierValueAccessor = identifierValueAccessor,
                ValueReaderToConvertIdentifierValueForComparison = valueReaderToConvertIdentifierValueForComparison
            };
            pipelineStep.Plugins.Add(identifierSettings);
        }
    }
}