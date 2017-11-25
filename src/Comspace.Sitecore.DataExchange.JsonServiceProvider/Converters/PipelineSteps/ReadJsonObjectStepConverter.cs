using Comspace.Sitecore.DataExchange.JsonServiceProvider.Processors;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Repositories;

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
    }
}