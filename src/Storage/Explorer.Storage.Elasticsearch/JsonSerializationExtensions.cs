
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Explorer.Storage.Elasticsearch
{
    public static class JsonSerializationExtensions
    {
        public static T FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }
}