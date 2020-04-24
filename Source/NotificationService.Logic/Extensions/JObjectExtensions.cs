using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NotificationService.Logic.Extensions
{
    /// <summary>
    /// Методы расширения класса <see cref="JObject"/>.
    /// </summary>
    public static class JObjectExtensions
    {
        /// <summary>
        /// Преобразовать json объект в словарь.
        /// </summary>
        /// <param name="obj">Json объект.</param>
        /// <returns>Словарь.</returns>
        public static IDictionary<string, object> ToDictionary(this JObject obj)
        {
            return ToCollections(obj) as IDictionary<string, object>;
        }

        private static object ToCollections(object obj)
        {
            switch (obj)
            {
                case JObject jo:
                    return jo
                        .ToObject<IDictionary<string, object>>()
                        .ToDictionary(k => k.Key, v => ToCollections(v.Value));
                case JArray ja:
                    return ja
                        .ToObject<List<object>>()
                        .Select(ToCollections)
                        .ToList();
                default:
                    return obj;
            }
        }
    }
}