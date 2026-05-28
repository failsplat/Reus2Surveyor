using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

            public List<int> ProjectIds { get => [..this.projects.itemData.Select(i => i.id)]; }
            public List<CityObjects.Project> Projects { get; private set; } = [];
            public Dictionary<int, CityObjects.Project> FindProjects(List<JToken> tokens)
            {
                this.Projects = [];
                Dictionary<int, CityObjects.Project> output = [];
                foreach (int id in this.ProjectIds)
                {
                    CityObjects.Project p = tokens[id].ToObject<CityObjects.Project>();
                    this.Projects.Add(p);
                    output[id] = p;
                }
                return output;
            }
        }

        public class ResourceController
        {
            public Id<int> gatherPoint { get; init; }
            [JsonProperty(Required = Required.Always)] public int prosperity { get; init; }
            public int highestProsperityReached { get; init; }
            [JsonProperty(Required = Required.Always)] public string name { get; init; }
            [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }
        }
    }
}
