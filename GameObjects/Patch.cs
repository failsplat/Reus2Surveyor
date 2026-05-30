using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class Patch
    {
        [JsonProperty(Required = Required.Always)] public Id<int> foregroundSlot { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> backgroundSlot { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> mountainSlot { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Id<int>>> projectSlots { get; init; }
        //public Aspectloots aspectLoots { get; init; }
        [JsonProperty(Required = Required.Always)] public string _type { get; init; }
        public Id<int> planet { get; init; }
        public Value<string>? biomeDefinition { get; init; }
        //public bool isBeingClaimed { get; init; }
        //public Value<int> patchVariation { get; init; }
        //public Value<int> currentBackdropMode { get; init; }
        //public Colorsandparameters colorsAndParameters { get; init; }
        //public Elevation elevation { get; init; }
        //public Windandwater windAndWater { get; init; }
        public Value<int> specialNaturalFeature { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<int> mountainPart { get; init; }
        public bool isDoubleTagged { get; init; }
        public int randomizedNumber { get; init; }
        public bool isWetlandsSubmerged { get; init; }
        //public int currentRoadLayout { get; init; }
        //public int terraformElevationLocks { get; init; }
        //public double terraformElevationLockTimer { get; init; }
        //public bool looksPolluted { get; init; }
        //public bool hasDisasterDamage { get; init; }
        //public Value<int> disasterDamageType { get; init; }
        //public double disasterDamageHealTimer { get; init; }
        //public Value<int> otherFeatures { get; init; }
        //public Value<int> forcedOceanMode { get; init; }
        //public object ruinedCityMemory { get; init; }
        public string name { get; init; }
        public Parent parent { get; init; }

        //

        public bool IsWild { get => this.projectSlots.itemData.Count == 0; }

        
        public bool SlotsSet { get; private set; } = false;
        public int ForegroundSlotId { get => foregroundSlot.id; }
        public int BackgroundSlotId { get => backgroundSlot.id; }
        public int MountainSlotId { get => mountainSlot.id; }
        public int MountainPart { get => this.mountainPart.value; }
        public BioticumSlot? ForegroundSlot { get; private set; }
        public BioticumSlot? BackgroundSlot { get; private set; }
        public BioticumSlot? MountainSlot { get; private set; }
        public int SpecialNaturalFeatureValue { get => this.specialNaturalFeature.value; }
        public string? BiomeDefinition { get => this.biomeDefinition?.value; }

        public List<BioticumSlot> SlotsInPatch
        {
            get 
            { 
                if (!this.SlotsSet)
                {
                    throw new InvalidOperationException("SlotsInPatch called before slots were linked!");
                }

                List<BioticumSlot> slots = [];
                if (this.ForegroundSlot is not null) slots.Add(this.ForegroundSlot);
                if (this.BackgroundSlot is not null) slots.Add(this.BackgroundSlot);
                if (this.MountainSlot is not null) slots.Add(this.MountainSlot);
                return slots;
            }
        }

        // Link down to slot
        public void AttachSlots(Dictionary<int, BioticumSlot> slotDict)
        {
            //if (this.ForegroundSlotId is not null)
            //{
                BioticumSlot foreSlot = slotDict[(int)this.ForegroundSlotId];
                this.ForegroundSlot = foreSlot;
                foreSlot.LinkPatch(this);
            //}
            //if (this.BackgroundSlotId is not null)
            //{
                BioticumSlot backSlot = slotDict[(int)this.BackgroundSlotId];
                this.BackgroundSlot = backSlot;
                backSlot.LinkPatch(this);
            //}
            //if (this.MountainSlotId is not null) 
            //{
                BioticumSlot mountainSlot = slotDict[(int)this.MountainSlotId];
                this.MountainSlot = mountainSlot;
                mountainSlot.LinkPatch(this);
            //}
            this.SlotsSet = true;
        }

        public List<int> ActiveSlotIndices { 
            get 
            {
                List<int> output = [];
                output.Add(this.ForegroundSlotId);
                output.Add(this.BackgroundSlotId);
                if (this.MountainPart > 0) output.Add(this.MountainSlotId);
                return output;
            } 
        }
    }
}
