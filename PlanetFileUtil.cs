using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Reus2Surveyor
{
    public class PlanetFileUtil
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
            return (Dictionary<string, object>)ObjectToDictionaryInner(inputObject);
        }

        public static Dictionary<string, object> ReadDictFromFile(string path)
        {
            string res = PlanetFileUtil.DecompressEncodedFile(path);
            var resAsObj = JsonConvert.DeserializeObject(res);
            Dictionary<string, object> resAsDict = PlanetFileUtil.ObjectToDictionary(resAsObj);
            return resAsDict;
        }

        public static Planet InterpretDictAsPlanet(Dictionary<string, object> resAsDict, string planetPath)
        {
            List<object> refTokens = (List<object>)resAsDict["referenceTokens"];
            Planet newPlanet = new(refTokens, planetPath);
            return newPlanet;
        }

        public static Planet ReadPlanetFromFile(string path)
        {
            Dictionary<string, object> resAsDict = PlanetFileUtil.ReadDictFromFile(path);
            string planetName = PlanetNameFromSaveFilePath(path);
            Planet newPlanet = InterpretDictAsPlanet(resAsDict, path);
            return newPlanet;
        }

        public static string PlanetNameFromSaveFilePath(string path)
        {
            List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
            pathParts.Reverse();
            string planetFolder = pathParts[1];
            List<string> planetNameParts = [.. planetFolder.Split('_')];
            return String.Join(' ', planetNameParts[1..]);
        }

        public static int EpochMinutesFromSaveFilePath(string path)
        {
            List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
            pathParts.Reverse();
            string planetFolder = pathParts[1];
            List<string> planetNameParts = [.. planetFolder.Split('_')];
            try
            {
                return System.Convert.ToInt32(planetNameParts[0]);
            }
            catch
            {
                return -1;
            }
        }

        public static string PlanetNameFromPlanetFolderPath(string path)
        {
            List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
            pathParts.Reverse();
            string planetFolder = pathParts[0];
            List<string> planetNameParts = [.. planetFolder.Split('_')];
            return String.Join(' ', planetNameParts[1..]);
        }

        public class SaveSlotManager
        {
            // This class is mostly so I don't have to make multiple lists or a nested dictionary
            // for tracking which planets have which save files
            public readonly string parentPath;
            public (string path, bool valid) Complete { get; private set; }

            public SaveSlotManager(string parentPath)
            {
                this.parentPath = parentPath;

                this.Complete = (
                    Path.Combine(parentPath, "auto_complete.deux"),
                    Path.Exists(Path.Combine(parentPath, "auto_complete.deux"))
                    );
            }
        }
    }
}
