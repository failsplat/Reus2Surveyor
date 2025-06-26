using System.Collections.Generic;
using System.Linq;

namespace Reus2Surveyor
{
    public class City
    {
        // Internally Constructed
        // These are generated with the city, instead of as an external object
        public readonly int? projectControllerId, resourceControllerId, luxuryControllerId, borderControllerId;
        public ProjectController CityProjectController { get; private set; }
        public ResourceController CityResourceController { get; private set; }
        public LuxuryController CityLuxuryController { get; private set; }
        public BorderController CityBorderController { get; private set; }

        // Primary Data
        public readonly string fancyName;
        public readonly int? cityIndex;
        public readonly int? leftNeighbourId, rightNeighbourId;
        public readonly string settledBiomeDef;
        public readonly string founderCharacterDef;
        public readonly int? patchId;
        public readonly int? currentVisualStage;
        public readonly List<string?> initiatedTurningPointsDefs = [];

        public int? tokenIndex = null;

        // Secondary Data
        public List<int> PatchIdsInTerritory { get; private set; } = [];
        public List<Patch> PatchesInTerritory { get; private set; } = [];
        public List<NatureBioticum> BioticaInTerritory { get; private set; } = [];
        public string currentBiomeDef { get; private set; }
        public GameSession.CivSummary CivSummary { get; private set; }

        public City(Dictionary<string, object> refDict, List<object> referenceTokensList)
        {
            this.projectControllerId = DictHelper.TryGetInt(refDict, ["projectController", "id"]);
            this.resourceControllerId = DictHelper.TryGetInt(refDict, ["resourceController", "id"]);
            this.luxuryControllerId = DictHelper.TryGetInt(refDict, ["luxuryController", "id"]);
            this.borderControllerId = DictHelper.TryGetInt(refDict, ["borderController", "id"]);

            if (this.projectControllerId is not null)
            {
                this.CityProjectController =
                    new ProjectController((Dictionary<string, object>)(referenceTokensList[(int)this.projectControllerId]), referenceTokensList);
            }
            if (this.resourceControllerId is not null)
            {
                this.CityResourceController =
                    new ResourceController((Dictionary<string, object>)(referenceTokensList[(int)this.resourceControllerId]));
            }
            if (this.luxuryControllerId is not null)
            {
                this.CityLuxuryController =
                    new LuxuryController((Dictionary<string, object>)(referenceTokensList[(int)this.luxuryControllerId]), referenceTokensList);
            }
            if (this.borderControllerId is not null)
            {
                this.CityBorderController =
                    new BorderController((Dictionary<string, object>)(referenceTokensList[(int)this.borderControllerId]));
            }

            this.fancyName = DictHelper.TryGetString(refDict, "fancyName");
            this.cityIndex = DictHelper.TryGetInt(refDict, "cityIndex");
            this.leftNeighbourId = DictHelper.TryGetInt(refDict, ["leftNeighbour", "id"]);
            this.rightNeighbourId = DictHelper.TryGetInt(refDict, ["rightNeighbour", "id"]);
            this.initiatedTurningPointsDefs = DictHelper.TryGetStringList(refDict, ["initiatedTurningPoints", "itemData"], "value");
            this.settledBiomeDef = DictHelper.TryGetString(refDict, ["nomadHeritage", "settledBiome", "value"]);
            this.founderCharacterDef = DictHelper.TryGetString(refDict, ["nomadHeritage", "character", "value"]);
            this.currentVisualStage = DictHelper.TryGetInt(refDict, "currentVisualStage");
            this.patchId = DictHelper.TryGetInt(refDict, ["position", "patch", "id"]);
        }

        public void BuildTerritoryInfo(PatchMap<int?> patchIDMap, Dictionary<int, Patch> patchDictionary)
        {
            this.PatchIdsInTerritory = [.. patchIDMap.PatchIndexSlice(this.CityBorderController.leftBorderPatchId, this.CityBorderController.rightBorderPatchId).Select(v => (int)v)];
            this.PatchesInTerritory = [.. patchDictionary.Where(kv => this.PatchIdsInTerritory.Contains(kv.Key)).Select(kv => kv.Value)];
            this.currentBiomeDef = patchDictionary[(int)this.patchId].biomeDefinition;
        }

        public List<int> ListSlotIndicesInTerritory()
        {
            List<int> output = [];
            foreach (Patch patch in this.PatchesInTerritory)
            {
                output.AddRange([.. patch.GetActiveSlotIndices()]);
            }
            return output;
        }

        public void CountTerritoryBiotica(Dictionary<int, BioticumSlot> slotDictionary, Dictionary<int, NatureBioticum> bioticaDictionary)
        {
            List<int> slotIndices = this.ListSlotIndicesInTerritory();
            if (slotDictionary.Count == 0)
            {
                return;
            }
            List<BioticumSlot> slots = [.. slotIndices.Select(s => slotDictionary[s])];
            List<int> biotIndices = [.. slots.Where(s => s.bioticumId is not null && s.bioticumId > 0).Select(s => (int)s.bioticumId)];
            this.BioticaInTerritory = [.. biotIndices.Select(s => bioticaDictionary.ContainsKey(s) ? bioticaDictionary[s] : null)];
        }

        public void AttachCivSummary(GameSession.CivSummary value)
        {
            this.CivSummary = value;
        }

        // Classes for internal use
        public class ProjectController
        {
            public readonly List<int?> projectsIds;
            public readonly int? projectsInspiredCount;
            public readonly List<CityProject> projects = [];

            public ProjectController(Dictionary<string, object> refDict, List<object> referenceTokensList)
            {
                this.projectsIds = DictHelper.TryGetIntList(refDict, ["projects", "itemData"], "id");
                this.projectsInspiredCount = DictHelper.TryGetInt(refDict, "projectsInspired");
                foreach (int pid in this.projectsIds)
                {
                    projects.Add(new CityProject((Dictionary<string, object>)(referenceTokensList[pid])));
                }
            }

            public class CityProject
            {
                public readonly string definition;
                public readonly string name;

                public CityProject(Dictionary<string, object> refDict)
                {
                    this.definition = DictHelper.TryGetString(refDict, ["definition", "value"]);
                    this.name = DictHelper.TryGetString(refDict, "name");
                }
            }
        }
        public class ResourceController
        {
            // Primary Data
            public float? highestProsperityReached;

            public ResourceController(Dictionary<string, object> refDict)
            {
                this.highestProsperityReached = DictHelper.TryGetFloat(refDict, "highestProsperityReached");
            }
        }
        public class LuxuryController
        {
            // Internally Constructed
            public readonly List<LuxurySlot> luxurySlots = [];
            public readonly List<LuxurySlot> tradeSlots = [];
            public readonly List<int?> importAgreementIds = [];
            public readonly int? luxuryBuffControllerId;
            public LuxuryController(Dictionary<string, object> refDict, List<object> referenceTokensList)
            {
                this.luxuryBuffControllerId = DictHelper.TryGetInt(refDict, ["luxuryBuffs", "id"]);

                List<object> luxurySlotSubdictList = (List<object>)DictHelper.DigValueAtKeys(refDict, ["luxurySlots", "itemData"]);
                foreach (Dictionary<string, object> subdict in luxurySlotSubdictList)
                {
                    this.luxurySlots.Add(new LuxurySlot((Dictionary<string, object>)subdict["value"], referenceTokensList));
                }

                List<object> tradeSlotSubdictList = (List<object>)DictHelper.DigValueAtKeys(refDict, ["tradeSlots", "itemData"]);
                foreach (Dictionary<string, object> subdict in tradeSlotSubdictList)
                {
                    this.tradeSlots.Add(new LuxurySlot((Dictionary<string, object>)subdict["value"], referenceTokensList));
                }

                this.importAgreementIds = DictHelper.TryGetIntList(refDict, ["importAgreements", "itemData"], "id");
            }

            // Classes for internal use
            public class LuxurySlot
            {
                public readonly int? tradePartnerId;
                public readonly int? luxuryGoodId;
                public readonly bool? isActive, isFree, isStolen;

                public readonly LuxuryGood luxuryGood;
                public LuxurySlot(Dictionary<string, object> subDict, List<object> referenceTokensList)
                {
                    this.tradePartnerId = DictHelper.TryGetInt(subDict, ["tradePartner", "id"]);
                    this.luxuryGoodId = DictHelper.TryGetInt(subDict, ["luxuryGood", "id"]);
                    if (this.luxuryGoodId is not null)
                    {
                        this.luxuryGood = new LuxuryGood((Dictionary<string, object>)(referenceTokensList[(int)this.luxuryGoodId]));
                    }
                    this.isActive = DictHelper.TryGetBool(subDict, "isActive");
                    this.isFree = DictHelper.TryGetBool(subDict, "isFree");
                    this.isStolen = DictHelper.TryGetBool(subDict, "isStolen");
                }
            }
            public class LuxuryGood
            {
                public readonly int? originCityId;
                public readonly string definition;
                public readonly string originalBioticumDef;

                public LuxuryGood(Dictionary<string, object> refDict)
                {
                    this.originCityId = DictHelper.TryGetInt(refDict, ["originCity", "id"]);
                    this.definition = DictHelper.TryGetString(refDict, ["definition", "value"]);
                    this.originalBioticumDef = DictHelper.TryGetString(refDict, ["originalBioticum", "value"]);
                }
            }
        }
        public class BorderController
        {
            public readonly int? leftBorderPatchId, rightBorderPatchId;
            public readonly int? leftBorderBiomeType, rightBorderBiomeType;

            public BorderController(Dictionary<string, object> refDict)
            {
                this.leftBorderPatchId = DictHelper.TryGetInt(refDict, ["leftBorder", "id"]);
                this.rightBorderPatchId = DictHelper.TryGetInt(refDict, ["rightBorder", "id"]);
                this.leftBorderBiomeType = DictHelper.TryGetInt(refDict, ["leftBorderBiomeType", "value"]);
                this.rightBorderBiomeType = DictHelper.TryGetInt(refDict, ["rightBorderBiomeType", "value"]);
            }
        }
    }
}
