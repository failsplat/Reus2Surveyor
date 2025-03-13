using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Compression;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Reus2Surveyor
{
    class PlanetFileUtil
    {
        public static string DecompressEncodedFile(string src)
        {
            FileStream rawFileStream = File.Open(src, FileMode.Open);
            rawFileStream.Seek(3, SeekOrigin.Begin); // Skip past v1
            GZipStream decompressor = new(rawFileStream, CompressionMode.Decompress);

            string s;
            using (var sr = new StreamReader(decompressor))
            {
                s = sr.ReadToEnd();
            }
            rawFileStream.Dispose();
            decompressor.Dispose();
            return s;
        }

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
            return (Dictionary<string, object>) ObjectToDictionaryInner(inputObject);
        }

        public static (Planet, Dictionary<string, object>) ReadPlanetFromFile(string path)
        {
            string res = PlanetFileUtil.DecompressEncodedFile(path);
            var resAsObj = JsonConvert.DeserializeObject(res);
            Dictionary<string, object> resAsDict = PlanetFileUtil.ObjectToDictionary(resAsObj);
            List<object> refTokens = (List<object>)resAsDict["referenceTokens"];
            Planet newPlanet = new(refTokens);
            return (newPlanet, resAsDict);
        }
    }
}
