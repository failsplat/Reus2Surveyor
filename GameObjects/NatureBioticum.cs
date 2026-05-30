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
        public ItemData<List<Id<int>>> aspectSlots { get; init; }
        public ItemData<List<Id<int>>> powerBonuses { get; init; }
        public string _type { get; init; }
        public int bioticumID { get; init; }
        //public object upgradeTimer { get; init; }
        public bool canReproduce { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<string> definition { get; init; }
        //public Id<int?> bioticumVisual { get; init; }
        public bool receivedRiverBonus { get; init; }
        public bool anomalyBonusActve { get; init; }
        public bool aurasActive { get; init; }
        public ItemData<List<Value<string>>> evolvedBiotica { get; init; }
        public Id<int?> riverBonus { get; init; }
        public string name { get; init; }
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

        //
        public string? Definition { get => this.definition.value; }
        public List<string> EvolvedDefinitions { get => [..this.evolvedBiotica.itemData
                .Where(i => i.value is not null)
                .Select(i => (string)i.value)]; }

        // Link upwards to Slot
        public BioticumSlot? Slot { get; private set; }
        public void LinkSlot(BioticumSlot slot)
        {
            this.Slot = slot;
        }
    }
}
