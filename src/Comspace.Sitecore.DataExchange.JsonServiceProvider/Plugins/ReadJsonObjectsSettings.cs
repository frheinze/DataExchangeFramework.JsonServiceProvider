using Sitecore.DataExchange;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Plugins
{
    public class ReadJsonObjectsSettings : IPlugin
    {
        public string Api { get; set; }

        public string RootJsonPath { get; set; }
    }
}
