using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reus2Surveyor.GameObjects
{
    public class BioticumSlot
    {
        [JsonProperty(Required = Required.Always)] public Id<int?> bioticum { get; init; }
        public Id<int> futureSlot { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> patch { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<int> locationOnPatch { get; init; } // 0 = Foreground, 1 = Background, 2 = Mountain 
        public bool areBonusesDiscovered { get; init; }
        public bool isAvailable { get; init; }
        public bool hasPermanentBooster { get; init; }
        public bool hasMountainDiscount { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Value<string>>> slotbonusDefinitions { get; init; }
        public double lastPayedEonPrice { get; init; }
        public bool pendingAspect { get; init; }
        public Value<int> citySlotCategory { get; init; }
        [JsonProperty(Required = Required.Always)] public int slotLevel { get; init; }
        public int fireLevel { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Value<ArchivedBioticum>>> archivedBiotica { get; init; }
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

        // 
        public int? BioticumIndex { get => this.bioticum.id; }

        // Link upwards to Patch
        public Patch? Patch { get; private set; }
        public void LinkPatch(Patch patch)
        {
            this.Patch = patch;
        }

        // Link downwards to Bioticum
        public NatureBioticum? ActiveBioticum { get; private set; } = null;
        public void FindBiotica(Dictionary<int, NatureBioticum> bioDict)
        {
            if (this.BioticumIndex is not null)
            {
                if (this.locationOnPatch.value > 2) 
                {
                    // This is a CityCustom patch, no actual bioticum
                    return;
                }

                if (this.Patch is null)
                {
                    throw new InvalidOperationException("Slot must be linked to Patch before linking to Biotica");
                }
                
                if (this.Patch.IsWild)
                {
                    NatureBioticum bio = bioDict[(int)this.BioticumIndex];
                    this.ActiveBioticum = bio;
                    bio.LinkSlot(this);
                }
            }
        }

        public List<ArchivedBioticum> ArchivedBiotica { get => [..this.archivedBiotica.itemData.Select(i => i.value)]; }
        public List<string> ArchivedBioticaDefs { get => [..this.ArchivedBiotica.Select(a => a.bioticum.value)]; }

        public int LocationOnPatch { get => this.locationOnPatch.value; }
    }
}
