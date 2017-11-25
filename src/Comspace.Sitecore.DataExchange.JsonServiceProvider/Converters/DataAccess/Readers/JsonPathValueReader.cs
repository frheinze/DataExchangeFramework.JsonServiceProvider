using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
using Sitecore.DataExchange;
using Sitecore.DataExchange.DataAccess;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.DataAccess.Readers
{
    /// <summary>
    /// Reader for JSON Objects by JSON Path (see https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm). <br/>    
    /// Test path: http://jsonpath.com/
    /// </summary>
    public class JsonPathValueReader : IValueReader
    {
        public readonly string JsonPath;

        public JsonPathValueReader(string jsonPath)
        {
            JsonPath = jsonPath;
        }

        public CanReadResult CanRead(object source, DataAccessContext context)
        {
            return new CanReadResult()
            {
                CanReadValue = source is JObject
            };
        }

        public virtual ReadResult Read(object source, DataAccessContext context)
        {
            JToken value = null;
            try
            {
                value = ((JObject)source).SelectToken(JsonPath);
            }
            catch (Exception ex)
            {
                Context.Logger.Error($"Error using {JsonPath}: {ex.Message}");
            }

            return new ReadResult(DateTime.Now)
            {
                WasValueRead = value != null,
                ReadValue = value?.ToString()
            };
        }
    }
}