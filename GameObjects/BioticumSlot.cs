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
        [JsonProperty(Required = Required.Always)] public Id<int?> bioticum { get; set; }
        public Id<int> futureSlot { get; set; }
        [JsonProperty(Required = Required.Always)] public Id<int> patch { get; set; }
        [JsonProperty(Required = Required.Always)] public Value<int> locationOnPatch { get; set; }
        public bool areBonusesDiscovered { get; set; }
        public bool isAvailable { get; set; }
        public bool hasPermanentBooster { get; set; }
        public bool hasMountainDiscount { get; set; }
        [JsonProperty(Required = Required.Always)] public ValueList<string> slotbonusDefinitions { get; set; }
        public double lastPayedEonPrice { get; set; }
        public bool pendingAspect { get; set; }
        public Value<int> citySlotCategory { get; set; }
        [JsonProperty(Required = Required.Always)] public int slotLevel { get; set; }
        public int fireLevel { get; set; }
        [JsonProperty(Required = Required.Always)] public ValueList<ArchivedBioticum> archivedBiotica { get; set; }
        public bool hasBioDiscount { get; set; }
        public bool isInvasiveSlot { get; set; }
        public int? fireSize { get; set; }
        [JsonProperty(Required = Required.Always)] public string name { get; set; }
        [JsonProperty(Required = Required.Always)] public Id<int?> parent { get; set; }

        public class ArchivedBioticum
        {
            public Value<string> bioticum { get; init; }
            // don't care about the other parts
        }
    }
}
