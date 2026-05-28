using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class CityControllers
    {
        public class ProjectController
        {
            [JsonProperty(Required = Required.Always)] public ItemData<List<Id<int>>> projects { get; init; }
            [JsonProperty(Required = Required.Always)] public int projectsInspired { get; init; }
        }
    }
}
