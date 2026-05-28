using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class Biome
    {
        [JsonProperty(Required = Required.Always)] public Id<int> biomeBuffs { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int?> anchorPatch { get; init; }
        [JsonProperty(Required = Required.Always)] public string visualName { get; init; }
        public string namePrefix { get; init; }
        public string nameSuffix { get; init; }
        public bool isPolluted { get; init; }
        public bool nameOnlySuffixContainsTheme { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<int> biomeType { get; init; }
        //public string name { get; init; }
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }
    }
}
