using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public static class CityObjects
    {
        public class Project
        {
            [JsonProperty(Required = Required.Always)] public Id<int> projectCrowd { get; init; }
            [JsonProperty(Required = Required.Always)] public Value<string> definition { get; init; }
            [JsonProperty(Required = Required.Always)] public string name { get; init; }
            [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

            public string Definition { get => this.definition.value; }
        }

        // This one is part of the city structure, not in separate reference token
        public class LuxurySlot
        {
            [JsonProperty(Required = Required.Always)] public Parent luxuryController { get; init; }
            public Id<int?> tradePartner { get; init; }
            public Id<int?> luxuryGood { get; init; }
            public bool? isActive { get; init; }
            public bool? isFree { get; init; }
            public bool? isStolen { get; init; }

            public int? LuxuryGoodId { get => this.luxuryGood.id; }
            public LuxuryGood? LuxuryGood { get; private set; } 
            public void AttachLuxuryGood(LuxuryGood luxuryGood)
            {
                this.LuxuryGood = luxuryGood;
            }
        }

        public class LuxuryGood
        {
            [JsonProperty(Required = Required.Always)] public Id<int?> originCity { get; init; }
            [JsonProperty(Required = Required.Always)] public Value<string> definition { get; init; }
            public Value<string>? originalBioticum { get; init; }
            [JsonProperty(Required = Required.Always)] public string name { get; init; }
            [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

            public int? OriginCityId { get => this.originCity.id; }
            public City? OriginCity { get; private set; }
            public void AttachOriginCity(City? city)
            {
                this.OriginCity = city;
            }
            public string Definition { get => this.definition.value; }
            public string? BioDefinition { get => this.originalBioticum?.value; }
        }
    }
}
