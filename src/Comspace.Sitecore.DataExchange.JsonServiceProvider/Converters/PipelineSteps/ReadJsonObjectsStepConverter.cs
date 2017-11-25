using Comspace.Sitecore.DataExchange.JsonServiceProvider.Models;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.PipelineSteps
{
    /// <summary>
    /// Converter for pipeline step <see cref="ReadJsonObjectsStepProcessor"/>.
    /// </summary>
    [SupportedIds("{A909A821-9537-4591-B769-92E43AF2402B}")]
    public class ReadJsonObjectsStepConverter : BasePipelineStepConverter
    {
        public ReadJsonObjectsStepConverter(IItemModelRepository repository)
            : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            AddEndpointSettings(source, pipelineStep);

            var settings = new ReadJsonObjectsSettings
            {
                RootJsonPath = GetStringValue(source, ReadJsonServiceStepItemModel.RootJsonPath)
            };
            pipelineStep.Plugins.Add(settings);
        }

        private void AddEndpointSettings(ItemModel source, PipelineStep pipelineStep)
        {
            var settings = new EndpointSettings();
            var endpointFrom = ConvertReferenceToModel<Endpoint>(source, ReadJsonServiceStepItemModel.EndpointFrom);
            if (endpointFrom != null)
            {
                settings.EndpointFrom = endpointFrom;
            }
            pipelineStep.Plugins.Add(settings);
        }
    }
}