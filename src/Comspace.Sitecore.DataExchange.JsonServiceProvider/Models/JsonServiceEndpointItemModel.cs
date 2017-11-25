using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Models
{
    public class JsonServiceEndpointItemModel : ItemModel
    {
        public const string Host = "Host";
        public const string Protocol = "Protocol";
        public const string ApiGetAll = "GetAll";
        public const string ApiGetById = "GetById";
        public const string AuthorizationScheme = "Scheme";
        public const string AuthorizationParameter = "Parameter";
    }
}