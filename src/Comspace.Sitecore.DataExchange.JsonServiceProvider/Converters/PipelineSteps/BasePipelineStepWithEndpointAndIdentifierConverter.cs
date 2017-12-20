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
    /// Converter for pipeline step <see cref="BasePipelineStepWithEndpointAndIdentifierProcessor"/>.
    /// </summary>
    public abstract class BasePipelineStepWithEndpointAndIdentifierConverter : BaseResolveObjectFromEndpointStepConverter
    {
        protected BasePipelineStepWithEndpointAndIdentifierConverter(IItemModelRepository repository)
            : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            base.AddPlugins(source, pipelineStep);
            AddIdentifierSettings(source, pipelineStep);
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