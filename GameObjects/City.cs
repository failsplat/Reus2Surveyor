using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Reus2Surveyor.GameObjects.CityControllers;

namespace Reus2Surveyor.GameObjects
{

    public partial class City
    {
        //public Giantresponsecrowds giantResponseCrowds { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> projectController { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> resourceController { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> luxuryController { get; init; }
        //public Id<int> armyController { get; init; }
        //public Id<int> hunterController { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> borderController { get; init; }
        //public Id<int> personalityController { get; init; }
        //public Id<int> requestController { get; init; }
        //public Id<int> thoughtController { get; init; }
        //public Id<int> cityBuffController { get; init; }
        //public Id<int> foreman { get; init; }
        //public Id<int> skirmisher { get; init; }
        //public Id<int> cityVillagerPool { get; init; }
        //public Id<int> errandPool { get; init; }
        //public Id<int> cityBehaviorController { get; init; }
        //public Id<int> cityDramaController { get; init; }
        //public IdItemDataList<int> people { get; init; }
        public Id<int> visualCompositor { get; init; }
        public Value<string> definition { get; init; }
        public double foundedOn { get; init; }
        [JsonProperty(Required = Required.Always)] public string fancyName { get; init; }
        [JsonProperty(Required = Required.Always)] public int cityIndex { get; init; }
        public Id<int> leftNeighbour { get; init; }
        public Id<int> rightNeighbour { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<string> biomeOrigin { get; init; }
        //public Value<string> currentCityEra { get; init; }
        //public Value<string> activatedMiniPower { get; init; }
        //public object cityEvolutionDelayTimer { get; init; }
        //public object cityEvolutionTimer { get; init; }
        //public double projectSpawnCooldown { get; init; }
        //public bool isEvolving { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Value<string>>> initiatedTurningPoints { get; init; }
        public Items<ItemData<List<Value<string>>>> bioticumPool { get; init; }
        public NomadHeritage nomadHeritage { get; init; }
        //public Position position { get; init; }
        public CityRandom cityRandom { get; init; }
        public int currentVisualStage { get; init; }
        public int cityIconIndex { get; init; }
        //public string cityLog { get; init; }
        //public Color color { get; init; }
        //public int colorIndex { get; init; }
        //public Envoyspawners envoySpawners { get; init; }
        //public Completedambitions completedAmbitions { get; init; }
        //public Value<int> healthType { get; init; }
        public double hitByDisasterCooldown { get; init; }
        //public Lastattackinggiant lastAttackingGiant { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int> citySlot { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Id<int>>> projectSlots { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Id<int>>> expansionsLeft { get; init; }
        [JsonProperty(Required = Required.Always)] public ItemData<List<Id<int>>> expansionsRight { get; init; }
        public bool isReptilian { get; init; }
        [JsonProperty(Required = Required.Always)] public string name { get; init; } // e.g. "City #1"
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

        //
        public ProjectController? CityProjectController { get; private set; }
        public void AttachProjectController(ProjectController? projectController)
        {
            this.CityProjectController = projectController;
        }
        public ResourceController? CityResourceController { get; private set; }
        public void AttachResourceController(ResourceController? resourceController)
        {
            this.CityResourceController = resourceController;
        }
        public LuxuryController? CityLuxuryController { get; private set; }
        public void AttachLuxuryController(LuxuryController? luxuryController)
        {
            this.CityLuxuryController = luxuryController;
        }
        public BorderController? CityBorderController { get; private set; }
        public void AttachBorderController(BorderController? borderController)
        {
            this.CityBorderController = borderController;
        }

    }

    public class NomadHeritage
    {
        public Value<string> settledBiome { get; init; }
        public Value<string> character { get; init; }
        public Value<string> pendingRequest { get; init; }
        public ItemData<List<Value<string>>> selectedTraits { get; init; }
        public Id<int> harmonyController { get; init; }
        public int emblemIndex { get; init; }
    }

    public class CityRandom
    {
        public int seedState { get; init; }
        public float pulls { get; init; }
        public int baseSeedState { get; init; }
    }
}
