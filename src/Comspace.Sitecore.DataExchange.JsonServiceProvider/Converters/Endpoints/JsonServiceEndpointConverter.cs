using Comspace.Sitecore.DataExchange.JsonServiceProvider.Models;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.Endpoints;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.Endpoints
{
    [SupportedIds("{F5E106FC-6A3C-41EB-8C34-A896628155C2}")]
    public class JsonServiceEndpointConverter : BaseEndpointConverter
    {
        public JsonServiceEndpointConverter(IItemModelRepository repository)
            : base(repository)
        {
        }

        protected override void AddPlugins(ItemModel source, Endpoint endpoint)
        {
            //create the plugin & populate the plugin using values from item
            var settings = new JsonServiceEndpointSettings
            {
                Host = GetStringValue(source, JsonServiceEndpointItemModel.Host),
                Protocol = GetStringValue(source, JsonServiceEndpointItemModel.Protocol),
                GetAll = GetStringValue(source, JsonServiceEndpointItemModel.ApiGetAll),
                GetById = GetStringValue(source, JsonServiceEndpointItemModel.ApiGetById),
                Scheme = GetStringValue(source, JsonServiceEndpointItemModel.AuthorizationScheme),
                Parameter = GetStringValue(source, JsonServiceEndpointItemModel.AuthorizationParameter)
            };

            //add the plugin to the endpoint
            endpoint.Plugins.Add(settings);
        }
    }
}