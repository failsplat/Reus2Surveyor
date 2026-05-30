using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reus2Surveyor
{
    class DictHelper
    {
        // Legacy old version of the deserialization
        // Used to clean up the structure for pretty printing
        public static object ObjectToDictionaryInner(object inputObject)
        {
            switch (inputObject)
            {
                case JObject jObject: // objects become Dictionary<string,object>
                    var dict = ((IEnumerable<KeyValuePair<string, JToken>>)jObject).ToDictionary(j => j.Key, j => ObjectToDictionaryInner(j.Value));

                    if (dict.ContainsKey("$type"))
                    {
                        dict.Remove("$type");
                    }
                    if (dict.ContainsKey("$values"))
                    {
                        return dict["$values"];
                    }
                    return dict;
                case JArray jArray: // arrays become List<object>
                    return jArray.Select(ObjectToDictionaryInner).ToList();
                case JValue jValue: // values just become the value
                    return jValue.Value;
                default: // don't know what to do here
                    throw new Exception($"Unsupported type: {inputObject.GetType()}");
            }
        }

        public static Dictionary<string, object> ObjectToDictionary(object inputObject)
        {
            return (Dictionary<string, object>)ObjectToDictionaryInner(inputObject);
        }

        public static string CleanupDeserialize(string decompressed)
        {
            object resAsObj = JsonConvert.DeserializeObject(decompressed);
            Dictionary<string, object> resAsDict = DictHelper.ObjectToDictionary(resAsObj);
            return JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
        }
    }
}
