using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class CityObjects
    {
        public class Project
        {
            [JsonProperty(Required = Required.Always)] public Id<int> projectCrowd { get; init; }
            [JsonProperty(Required = Required.Always)] public Value<string> definition { get; init; }
            [JsonProperty(Required = Required.Always)] public string name { get; init; }
            [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }
        }
    }
}
