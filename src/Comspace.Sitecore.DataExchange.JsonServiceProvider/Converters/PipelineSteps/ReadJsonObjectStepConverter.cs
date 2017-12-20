using Comspace.Sitecore.DataExchange.JsonServiceProvider.Models;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.PipelineSteps
{
    /// <summary>
    /// Converter for pipeline step <see cref="ReadJsonObjectStepProcessor"/>.
    /// </summary>
    [SupportedIds("{8655A7CE-C734-48CC-B06B-265E7892A537}")]
    public class ReadJsonObjectStepConverter : BasePipelineStepWithEndpointAndIdentifierConverter
    {
        public ReadJsonObjectStepConverter(IItemModelRepository repository)
            : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            base.AddPlugins(source, pipelineStep);

            var settings = new ReadJsonObjectsSettings
            {
                RootJsonPath = GetStringValue(source, ReadJsonServiceStepItemModel.RootJsonPath)
            };
            pipelineStep.Plugins.Add(settings);
        }
    }
}