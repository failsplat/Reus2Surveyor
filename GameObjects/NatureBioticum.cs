using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class NatureBioticum
    {
        public IdItemDataList<int> aspectSlots { get; init; }
        public IdItemDataList<int> powerBonuses { get; init; }
        public string _type { get; init; }
        public int bioticumID { get; init; }
        //public object upgradeTimer { get; init; }
        public bool canReproduce { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<string> definition { get; init; }
        //public Id<int?> bioticumVisual { get; init; }
        public bool receivedRiverBonus { get; init; }
        public bool anomalyBonusActve { get; init; }
        public bool aurasActive { get; init; }
        public ValueItemDataList<string> evolvedBiotica { get; init; }
        public Id<int?> riverBonus { get; init; }
        public string name { get; init; }
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }
    }
}
