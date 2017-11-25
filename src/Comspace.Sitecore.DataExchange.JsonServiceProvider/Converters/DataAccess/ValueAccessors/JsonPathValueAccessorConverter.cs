using Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.DataAccess.Readers;
using Comspace.Sitecore.DataExchange.JsonServiceProvider.Models;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.DataAccess.ValueAccessors;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;

namespace Comspace.Sitecore.DataExchange.JsonServiceProvider.Converters.DataAccess.ValueAccessors
{
    [SupportedIds("{D5EE3E8B-E49A-4C88-8E98-F5BA7C535A73}")]
    public class JsonPathValueAccessorConverter : ValueAccessorConverter
    {
        public JsonPathValueAccessorConverter(IItemModelRepository repository)
            : base(repository)
        {
        }

        public override IValueAccessor Convert(ItemModel source)
        {
            var valueAccessor = base.Convert(source);
            if (valueAccessor != null && valueAccessor.ValueReader == null)
            {
                var jsonPath = GetStringValue(source, JsonPathValueAccessorItem.JsonPath);
                if (!string.IsNullOrEmpty(jsonPath))
                {
                    valueAccessor.ValueReader = new JsonPathValueReader(jsonPath);
                }
            }
            return valueAccessor;
        }
    }
}