using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reus2Surveyor.GameObjects
{
    public class BioticumSlot
    {
        [JsonProperty(Required = Required.Always)] public Id<int?> bioticum { get; init; }
        public Id<int> futureSlot { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> patch { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<int> locationOnPatch { get; init; }
        public bool areBonusesDiscovered { get; init; }
        public bool isAvailable { get; init; }
        public bool hasPermanentBooster { get; init; }
        public bool hasMountainDiscount { get; init; }
        [JsonProperty(Required = Required.Always)] public ValueItemDataList<string> slotbonusDefinitions { get; init; }
        public double lastPayedEonPrice { get; init; }
        public bool pendingAspect { get; init; }
        public Value<int> citySlotCategory { get; init; }
        [JsonProperty(Required = Required.Always)] public int slotLevel { get; init; }
        public int fireLevel { get; init; }
        [JsonProperty(Required = Required.Always)] public ValueItemDataList<ArchivedBioticum> archivedBiotica { get; init; }
        public bool hasBioDiscount { get; init; }
        public bool isInvasiveSlot { get; init; }
        public int? fireSize { get; init; }
        [JsonProperty(Required = Required.Always)] public string name { get; init; }
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

        public class ArchivedBioticum
        {
            public Value<string> bioticum { get; init; }
            // don't care about the other parts
        }
    }
}
