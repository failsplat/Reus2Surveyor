using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public int? cityIndex { get; private set; } // Not present in earlier versions. If not present, use TokenIndex
        public Id<int?> leftNeighbour { get; init; }
        public Id<int?> rightNeighbour { get; init; }
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
        public CityPosition position { get; init; }
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
        public int TokenIndex;
        public int CityOrder
        {
            // Uses cityIndex if available, falls back on TokenIndex
            get
            {
                if (this.cityIndex is not null) return (int)this.cityIndex;
                else return this.TokenIndex;
            }
        }

        private ProjectController? CityProjectController { get; set; }
        public void AttachProjectController(ProjectController? projectController)
        {
            this.CityProjectController = projectController;
        }
        private ResourceController? CityResourceController { get; set; }
        public void AttachResourceController(ResourceController? resourceController)
        {
            this.CityResourceController = resourceController;
        }
        private LuxuryController? CityLuxuryController { get; set; }
        public void AttachLuxuryController(LuxuryController? luxuryController)
        {
            this.CityLuxuryController = luxuryController;
        }
        public void AttachLuxuryGoods(Dictionary<int, CityObjects.LuxuryGood> goodDict)
        {
            this.CityLuxuryController.AttachLuxuryGoods(goodDict);
        }
        private BorderController? CityBorderController { get; set; }
        public void AttachBorderController(BorderController? borderController)
        {
            this.CityBorderController = borderController;
        }
        public CivSummary? CivSummary { get; set; }
        public void AttachCivSummary(CivSummary? civSummary)
        {
            this.CivSummary = civSummary;
        }

        public List<CityObjects.Project> Projects { get => this.CityProjectController.Projects; }
        public List<CityObjects.LuxurySlot> LuxurySlots { get => [.. this.CityLuxuryController.luxurySlots.itemData.Select(i => i.value)]; }
        public List<CityObjects.LuxurySlot> TradeSlots { get => [.. this.CityLuxuryController.tradeSlots.itemData.Select(i => i.value)]; }
        public List<CityObjects.LuxuryGood> LuxuryGoods { get => this.CityLuxuryController.LuxuryGoodsLocal; }
        public List<CityObjects.LuxuryGood> TradeGoods { get => this.CityLuxuryController.LuxuryGoodsTrade; }
        public string FoundingCharacterDef { get => this.nomadHeritage.character.value; }
        public string SettledBiome { get => this.nomadHeritage.settledBiome.value; }
        public int LuxuryBuffControllerId { get => this.CityLuxuryController.luxuryBuffs.id; }
        public List<string> InitiatedTurningPoints { get => [..this.initiatedTurningPoints.itemData.Select(i => i.value)]; }

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

        public List<int> PatchIdsInTerritory { get; private set; } = [];
        public List<Patch> PatchesInTerritory { get; private set; } = [];
        public string CurrentBiomeDefinition { get; private set; } = "NOT SET";
        public List<NatureBioticum> BioticaInTerritory { get; private set; } = [];
        public int CityPatch { get => this.position.patch.id; }
        public void BuildTerritoryInformation(PatchMap<int> patchMap, Dictionary<int, Patch> patches, Dictionary<int, NatureBioticum> bioticaActive)
        {
            this.PatchIdsInTerritory = patchMap.PatchIndexSlice(this.CityBorderController.LeftBorderId, this.CityBorderController.RightBorderId);
            this.PatchesInTerritory = [.. this.PatchIdsInTerritory.Select(i => patches[i])];
            this.CurrentBiomeDefinition = patches[this.CityPatch].BiomeDefinition;

            foreach (Patch patch in this.PatchesInTerritory)
            {
                foreach (BioticumSlot slot in patch.SlotsInPatch)
                {
                    if (slot.BioticumIndex is not null)
                    {
                        this.BioticaInTerritory.Add(bioticaActive[(int)slot.BioticumIndex]);
                    }
                }
            }
        }

        public class CityPosition
        {
            public Id<int> patch { get; init; }
            public double positionX { get; init; }
            public double positionY { get; init; }
            public Value<int> attachment { get; init; }
            public bool detectWaterLimit { get; init; }
            public bool dynamicWaterMode { get; init; }
            public int visualPatchIndex { get; init; }
        }
    }
}
