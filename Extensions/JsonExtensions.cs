using Newtonsoft.Json;

namespace BattleCity.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings =
             new JsonSerializerSettings() { 
                 NullValueHandling = NullValueHandling.Ignore,
                 Formatting = Formatting.Indented
             };

        /// <summary>
        /// Преобразовать в json-форматированную строку
        /// </summary>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
        }
    }
}
