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
        [JsonProperty(Required = Required.Always)] public Value<string> biomeDefinition { get; init; }
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
    }
}
