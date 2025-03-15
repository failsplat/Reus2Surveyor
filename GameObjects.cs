using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.InteropServices.Swift;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reus2Surveyor
{
    public class Planet
    {
        public string name;
        public readonly Dictionary<int, BioticumSlot> slotDictionary = [];
        public readonly Dictionary<int, Patch> patchDictionary = [];
        public readonly Dictionary<int, Biome> biomeDictionary = [];
        public readonly Dictionary<int, NatureBioticum> natureBioticumDictionary = [];
        public readonly Dictionary<int, City> cityDictionary = [];
        public readonly int totalSize;
        public readonly int wildSize;
        public readonly PatchMap<int?> patchIdMap;
        public readonly GameSession gameSession;

        //public List<int?> patchCollection;

        public readonly static List<string> slotCheckKeys = [
            "bioticum", "futureSlot", "patch", "locationOnPatch", "slotLevel", "parent",
                "isInvasiveSlot", "slotbonusDefinitions", "archivedBiotica", "name"
            ];
        public readonly static List<string> patchCheckKeys = [
            "foregroundSlot", "backgroundSlot", "mountainSlot",
                "projectSlots", "_type", "planet", "biomeDefinition", "patchVariation",
                "currentBackdropMode", "colorsAndParameters", "elevation", "windAndWater", "specialNaturalFeature", "mountainPart"
            ];
        /*public readonly static List<string> planetCloneCheckKeys = [
            "patches", "rivers", "biomeSaveData", "puzzle", "animalController", "allPropVisuals", "name"
            ];*/
        public readonly static List<string> patchCollectionCheckKeys = [
            "models", "name", "parent"
            ];
        public readonly static List<string> biomeCheckKeys = [
            "biomeBuffs", "anchorPatch", "visualName", "biomeType", "name",
            ];
        public readonly static List<string> bioticumCheckKeys = [
            "aspectSlots", "_type", "bioticumID", "definition", "receivedRiverBonus", 
            "anomalyBonusActve", // [sic]
            "name", "parent"
            ];
        public readonly static List<string> cityCheckKeys = [
            "projectController", "resourceController", "luxuryController", "fancyName", "cityIndex", 
            "leftNeighbour", "rightNeighbour", // British Spelling
            "biomeOrigin", "initiatedTurningPoints", "nomadHeritage", "currentVisualStage", "citySlot", "projectSlots",
            ];
        public readonly static List<string> sessionCheckKeys = [
            "gameplayController", "startParameters", "sessionID", "isFinished", "freePlay", "planetIsLost", "name"
            ];

        public Planet(List<object> referenceTokensList)
        {
            int i = -1;

            // Primary Data (first pass getting data directly from the dictionary)
            foreach (Dictionary<string, object> refToken in referenceTokensList)
            {
                i++;
                List<string> rtKeys = [.. refToken.Keys];
                if (slotCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.slotDictionary.Add(i, new BioticumSlot(refToken));
                    continue;
                }
                if (patchCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["_type"] == "Patch")
                {
                    this.patchDictionary.Add(i, new Patch(refToken));
                    continue;
                }
                if (biomeCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.biomeDictionary.Add(i, new Biome(refToken));
                    continue;
                }
                if (bioticumCheckKeys.All(k=> rtKeys.Contains(k)) && (string)refToken["name"] == "NatureBioticum")
                {
                    this.natureBioticumDictionary.Add(i, new NatureBioticum(refToken));
                    continue;
                }
                if (cityCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.cityDictionary.Add(i, new City(refToken, referenceTokensList));
                    continue;
                }
                if (sessionCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["name"] == "Session")
                {
                    this.gameSession = new GameSession(refToken);
                    continue;
                } 
                if (patchCollectionCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["name"] == "PatchCollection")
                {
                    this.patchIdMap = new PatchMap<int?>(
                        DictionaryHelper.TryGetIntList(refToken, ["models", "itemData"], "id"));
                    continue;
                }
            }

            // Secondary Data (calculated when all game objects parsed)
            this.totalSize = this.patchDictionary.Count;
            this.wildSize =  this.patchDictionary.Where(kvp => kvp.Value.IsWildPatch()).Count();
            foreach (Biome b in this.biomeDictionary.Values)
            {
                b.BuildPatchInfo(this.patchIdMap, this.patchDictionary);
            }
            // Remove biotica with ID 0
            List<int> inactiveBiotica = [];
            foreach (KeyValuePair<int,NatureBioticum> kv in this.natureBioticumDictionary)
            {
                if (!kv.Value.IsActive()) inactiveBiotica.Add(kv.Key);
            }
            foreach(int removeIndex in inactiveBiotica)
            {
                this.natureBioticumDictionary.Remove(removeIndex);
            }

            foreach(City city in this.cityDictionary.Values)
            {
                city.BuildTerritoryInfo(this.patchIdMap, this.patchDictionary);
                city.CountTerritoryBiotica(this.slotDictionary, this.natureBioticumDictionary);
            }

            List<ValueTuple<City,GameSession.CivSummary>> CitySummaryPairs = [..this.cityDictionary.Values.Zip(this.gameSession.civSummaries).ToList()];
            foreach (ValueTuple<City,GameSession.CivSummary> cc in CitySummaryPairs)
            {
                cc.Item1.AttachCivSummary(cc.Item2);
            }
        }
    }

    public class BioticumSlot
    {
        // Primary Data
        public readonly int? bioticumId, patchId, locationOnPatch, slotLevel;
        public readonly int? futureSlotId;

        public readonly bool isInvasiveSlot;
        public readonly List<string> slotbonusDefinitions;
        public readonly List<Dictionary<string, object>> archivedBiotica;
        public readonly string name;

        public BioticumSlot(Dictionary<string, object> refDict)
        {
            this.bioticumId = DictionaryHelper.TryGetInt(refDict, ["bioticum", "id"]);
            this.futureSlotId = DictionaryHelper.TryGetInt(refDict, ["futureSlot", "id"]);
            this.patchId = DictionaryHelper.TryGetInt(refDict, ["patch", "id"]);

            // 0 = Foreground
            // 1 = Background
            // 2 = Mountain 
            this.locationOnPatch = DictionaryHelper.TryGetInt(refDict, ["locationOnPatch", "value"]);

            this.slotbonusDefinitions = DictionaryHelper.TryGetStringList(refDict, ["slotbonusDefinitions", "itemData"], "itemData");

            this.slotLevel = DictionaryHelper.TryGetInt(refDict,"slotLevel");

            this.archivedBiotica = DictionaryHelper.TryGetDictList(refDict, ["archivedBiotica", "itemData"], "value");
            
            this.isInvasiveSlot = (bool)refDict["isInvasiveSlot"];

            // "BioticumSlot (<patch> - <position>)
            this.name = DictionaryHelper.TryGetString(refDict, "name");
        }
    }

    public class Patch
    {
        // Primary Data
        public readonly int? foregroundSlotId, backgroundSlotId, mountainSlotId, mountainPart;
        private readonly List<int?> projectSlotsIds;
        public readonly string biomeDefinition, name;
        private readonly object ruinedCityMemory;

        public Patch(Dictionary<string, object> refDict)
        {
            this.foregroundSlotId = DictionaryHelper.TryGetInt(refDict, ["foregroundSlot", "id"]);
            this.backgroundSlotId = DictionaryHelper.TryGetInt(refDict, ["backgroundSlot", "id"]);
            this.mountainSlotId = DictionaryHelper.TryGetInt(refDict, ["mountainSlot", "id"]);

            this.projectSlotsIds = [];
            List<object> projectSlotDicts = (List<object>)DictionaryHelper.DigValueAtKeys(refDict, ["projectSlots", "itemData"]);

            this.projectSlotsIds = DictionaryHelper.TryGetIntList(refDict, ["projectSlots", "itemData"], "id");
            this.biomeDefinition = DictionaryHelper.TryGetString(refDict, ["biomeDefinition", "value"]);

            this.mountainPart = DictionaryHelper.TryGetInt(refDict, ["mountainPart", "value"]);
            this.ruinedCityMemory = refDict["ruinedCityMemory"];
            this.name = (string)refDict["name"];
        }

        public bool IsWildPatch()
        {
            return (this.projectSlotsIds.Count == 0);
        }

        public List<int> GetActiveSlotIndices()
        {
            List<int> output = [];
            if (this.foregroundSlotId is not null) output.Add((int)this.foregroundSlotId);
            if (this.backgroundSlotId is not null) output.Add((int)this.backgroundSlotId);
            if (this.mountainSlotId is not null && this.mountainPart > 0) output.Add((int)this.mountainSlotId);
            return output;
        }
    }

    public class PatchMap<T> : List<T>
    {
        // Index 0 -> leftmost
        // Increasing index goes to right on planet (clockwise) 
        // TODO: Check if the assumption above is actually true
        // This class makes the list superficially behave like a circular linked list for specific methods

        public PatchMap(IEnumerable<T> values) : base(values)
        { }

        public List<T> IndexSlice(int leftIndex, int rightIndex)
        {
            if (leftIndex <= rightIndex)
            {
                return this.Slice(leftIndex, rightIndex-leftIndex+1);
            }
            else
            {
                List<T> tailslice = this[leftIndex..this.Count()];
                List<T> headSlice = this[..(rightIndex + 1)];
                return [.. tailslice, .. headSlice];
            }
        }

        public List<T> PatchIndexSlice(T leftID, T rightID)
        {
            int leftIndex = this.IndexOf(leftID);
            int rightIndex = this.IndexOf(rightID);
            if (leftIndex == -1 || rightIndex == -1) return [];
            else return IndexSlice(leftIndex, rightIndex);
        }

    }

    public class Biome
    {
        // Primary Data
        public readonly int? anchorPatchId;
        public readonly string visualName;
        public readonly int? biomeTypeInt;
        public readonly string biomeTypeName;

        // Secondary Data
        public string biomeTypeDef { get; private set; }
        public List<int?> patchList { get; private set; }
        public List<int?> wildPatchList { get; private set; }
        public int totalSize { get; private set; }
        public int wildSize { get; private set; }

        public Biome(Dictionary<string, object> refDict)
        {
            this.anchorPatchId = DictionaryHelper.TryGetInt(refDict, ["anchorPatch", "id"]);
            this.visualName = DictionaryHelper.TryGetString(refDict, ["visualName"]);
            this.biomeTypeInt = DictionaryHelper.TryGetInt(refDict, ["biomeType", "value"]);
            if (this.biomeTypeInt is not null) this.biomeTypeName = Glossaries.BiomeNameByInt[(int)this.biomeTypeInt];
        }

        public void BuildPatchInfo(PatchMap<int?> patchMap, Dictionary<int, Patch> patchDict)
        {
            if (this.anchorPatchId is null)
            {
                this.patchList = [];
                this.wildPatchList = [];
                this.totalSize = this.patchList.Count;
                this.wildSize = this.wildPatchList.Count;
                return;
            }

            int anchorMapPosition = patchMap.IndexOf(this.anchorPatchId);
            Patch anchorPatchObj = patchDict[(int)this.anchorPatchId];
            this.biomeTypeDef = anchorPatchObj.biomeDefinition;

            List<int?> leftPatches = [];
            List<int?> rightPatches = [];

            for (int leftMapPosition = anchorMapPosition - 1; (patchMap.Count + (leftMapPosition % patchMap.Count)) % patchMap.Count != anchorMapPosition; leftMapPosition--)
            {
                int leftPatchIndex = (int)patchMap[(patchMap.Count + (leftMapPosition % patchMap.Count)) % patchMap.Count];
                if (!patchDict.ContainsKey(leftPatchIndex)) break;
                string leftPatchBiomeDef = patchDict[leftPatchIndex].biomeDefinition;
                if (leftPatchBiomeDef == this.biomeTypeDef) leftPatches.Insert(0, leftPatchIndex);
                else break;
            }
            for (int rightMapPosition = anchorMapPosition + 1; rightMapPosition % patchMap.Count != anchorMapPosition; rightMapPosition++)
            {
                int rightPatchIndex = (int)patchMap[rightMapPosition % patchMap.Count];
                if (!patchDict.ContainsKey(rightPatchIndex)) break;
                string rightPatchBiomeDef = patchDict[rightPatchIndex].biomeDefinition;
                if (rightPatchBiomeDef == this.biomeTypeDef) rightPatches.Add(rightPatchIndex);
                else break;
            }

            rightPatches.Insert(0, this.anchorPatchId);
            leftPatches.AddRange(rightPatches);
            this.patchList = leftPatches;
            this.wildPatchList = [..this.patchList.Where(x => patchDict[(int)x].IsWildPatch())];
            this.totalSize = this.patchList.Count;
            this.wildSize = this.wildPatchList.Count;
        }
    }

    public class NatureBioticum
    {
        // Active bioticum only!
        // Legacy biotica are archived as part of the slots.

        // Primary Data
        public readonly List<int?> aspectSlotsIds;
        public readonly int? bioticumId;
        public readonly string? definition;
        public readonly bool receivedRiverBonus, anomalyBonusActive;
        public readonly int? slotId;

        // Secondary Data
        public bool IsOnMountain { get; private set; }
        public bool HasMicro { get; private set; }

        public NatureBioticum(Dictionary<string,object> refDict)
        {
            this.aspectSlotsIds = DictionaryHelper.TryGetIntList(refDict, ["aspectSlots", "itemData"], "id");
            this.bioticumId = DictionaryHelper.TryGetInt(refDict, "bioticumID");
            this.definition = DictionaryHelper.TryGetString(refDict, ["definition", "value"]);

            this.receivedRiverBonus = (bool)refDict["receivedRiverBonus"];
            this.anomalyBonusActive = (bool)refDict["anomalyBonusActve"]; // [sic]

            this.slotId = DictionaryHelper.TryGetInt(refDict, ["parent", "id"]);
        }
        
        public void CheckSlotProperties(Dictionary<int, BioticumSlot> slotDict)
        {
            if (this.slotId is null || this.bioticumId is null) return;
            BioticumSlot thisSlot = slotDict[(int)this.slotId];
            this.IsOnMountain = thisSlot.locationOnPatch == 2;
        }
        // TODO: (Maybe) check that the aspect slot(s) have placed micros

        public bool IsActive() { return this.bioticumId is null ? false : this.bioticumId > 0; }
    }

    public class City
    {
        // Internally Constructed
        // These are generated with the city, instead of as an external object
        public readonly int? projectControllerId, resourceControllerId, luxuryControllerId, borderControllerId;
        public ProjectController CityProjectController {get; private set;}
        public ResourceController CityResourceController {get; private set;}
        public LuxuryController CityLuxuryController {get; private set;}
        public BorderController CityBorderController {get; private set;}

        // Primary Data
        public readonly string fancyName;
        public readonly int? cityIndex;
        public readonly int? leftNeighbourId, rightNeighbourId;
        public readonly string settledBiomeDef;
        public readonly string founderCharacterDef;
        public readonly int? patchId;
        public readonly int? currentVisualStage;
        public readonly List<string?> initiatedTurningPointsDefs = [];

        // Secondary Data
        public List<int> PatchIdsInTerritory { get; private set; } = [];
        public List<Patch> PatchesInTerritory { get; private set; } = [];
        public List<NatureBioticum> BioticaInTerritory { get; private set; } = [];
        public string currentBiomeDef { get; private set; }
        public GameSession.CivSummary CivSummary { get; private set; }

        public City(Dictionary<string, object> refDict, List<object> referenceTokensList)
        {
            this.projectControllerId = DictionaryHelper.TryGetInt(refDict, ["projectController", "id"]);
            this.resourceControllerId = DictionaryHelper.TryGetInt(refDict, ["resourceController", "id"]);
            this.luxuryControllerId = DictionaryHelper.TryGetInt(refDict, ["luxuryController", "id"]);
            this.borderControllerId = DictionaryHelper.TryGetInt(refDict, ["borderController", "id"]);

            if (this.projectControllerId is not null) 
            {
                this.CityProjectController = 
                    new ProjectController((Dictionary<string, object>)(referenceTokensList[(int)this.projectControllerId]));
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

            this.fancyName = DictionaryHelper.TryGetString(refDict, "fancyName");
            this.cityIndex = DictionaryHelper.TryGetInt(refDict, "cityIndex");
            this.leftNeighbourId = DictionaryHelper.TryGetInt(refDict, ["leftNeighbour", "id"]);
            this.rightNeighbourId = DictionaryHelper.TryGetInt(refDict, ["rightNeighbour", "id"]);
            this.initiatedTurningPointsDefs = DictionaryHelper.TryGetStringList(refDict, ["initiatedTurningPoints", "itemData"], "value");
            this.settledBiomeDef = DictionaryHelper.TryGetString(refDict, ["nomadHeritage", "settledBiome", "value"]);
            this.founderCharacterDef = DictionaryHelper.TryGetString(refDict, ["nomadHeritage", "character", "value"]);
            this.currentVisualStage = DictionaryHelper.TryGetInt(refDict, "currentVisualStage");
            this.patchId = DictionaryHelper.TryGetInt(refDict, ["position", "patch", "id"]);
        }

        public void BuildTerritoryInfo(PatchMap<int?> patchIDMap, Dictionary<int, Patch> patchDictionary)
        {
            this.PatchIdsInTerritory = [..patchIDMap.PatchIndexSlice(this.CityBorderController.leftBorderPatchId, this.CityBorderController.rightBorderPatchId).Select(v => (int)v)];
            this.PatchesInTerritory = [..patchDictionary.Where(kv => this.PatchIdsInTerritory.Contains(kv.Key)).Select(kv => kv.Value)];
            this.currentBiomeDef = patchDictionary[(int)this.patchId].biomeDefinition;
        }

        public List<int> ListSlotIndicesInTerritory()
        {
            List<int> output = [];
            foreach (Patch patch in this.PatchesInTerritory)
            {
                output.AddRange([..patch.GetActiveSlotIndices()]);
            }
            return output;
        }

        public void CountTerritoryBiotica(Dictionary<int, BioticumSlot> slotDictionary, Dictionary<int, NatureBioticum> bioticaDictionary)
        {
            List<int> slotIndices = this.ListSlotIndicesInTerritory();
            List<BioticumSlot> slots = [..slotIndices.Select(s => slotDictionary[s])];
            List<int> biotIndices = [.. slots.Where(s => s.bioticumId is not null && s.bioticumId > 0).Select(s => (int)s.bioticumId)];
            this.BioticaInTerritory = [.. biotIndices.Select(s => bioticaDictionary[s])];
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

            public ProjectController(Dictionary<string, object> refDict)
            {
                this.projectsIds = DictionaryHelper.TryGetIntList(refDict, ["projects", "itemData"], "id");
                this.projectsInspiredCount = DictionaryHelper.TryGetInt(refDict, "projectsInspired");
            }

        }
        public class ResourceController
        {
            // Primary Data
            public float? highestProsperityReached;

            public ResourceController(Dictionary<string, object> refDict)
            {
                this.highestProsperityReached = DictionaryHelper.TryGetFloat(refDict, "highestProsperityReached");
            }
        }
        public class LuxuryController
        {
            // Internally Constructed
            public readonly List<LuxurySlot> luxurySlots = [];
            
            public LuxuryController(Dictionary<string, object> refDict, List<object> referenceTokensList)
            {
                List<object> luxurySlotSubdictList = (List<object>)DictionaryHelper.DigValueAtKeys(refDict, ["luxurySlots", "itemData"]);
                foreach (Dictionary<string,object> subdict in luxurySlotSubdictList)
                {
                    this.luxurySlots.Add(new LuxurySlot((Dictionary<string, object>)subdict["value"], referenceTokensList));
                }
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
                    this.tradePartnerId = DictionaryHelper.TryGetInt(subDict, ["tradePartner","id"]);
                    this.luxuryGoodId = DictionaryHelper.TryGetInt(subDict, ["luxuryGood","id"]);
                    if (this.luxuryGoodId is not null) 
                    {
                        this.luxuryGood = new LuxuryGood((Dictionary<string, object>)(referenceTokensList[(int)this.luxuryGoodId]));
                    }
                    this.isActive = DictionaryHelper.TryGetBool(subDict, "isActive");
                    this.isFree = DictionaryHelper.TryGetBool(subDict, "isFree");
                    this.isStolen = DictionaryHelper.TryGetBool(subDict, "isStolen");
                }
            }
            public class LuxuryGood
            {
                public readonly int? originCityId;
                public readonly string definition;
                public readonly string originalBioticumDef;

                public LuxuryGood(Dictionary<string, object> refDict)
                {
                    this.originCityId = DictionaryHelper.TryGetInt(refDict, ["originCity", "id"]);
                    this.definition = DictionaryHelper.TryGetString(refDict, ["definition", "value"]);
                    this.originalBioticumDef = DictionaryHelper.TryGetString(refDict, ["originalBioticum", "value"]);
                }
            }
        }
        public class BorderController
        {
            public readonly int? leftBorderPatchId, rightBorderPatchId;
            public readonly int? leftBorderBiomeType, rightBorderBiomeType;

            public BorderController(Dictionary<string, object> refDict)
            {
                this.leftBorderPatchId = DictionaryHelper.TryGetInt(refDict, ["leftBorder", "id"]);
                this.rightBorderPatchId = DictionaryHelper.TryGetInt(refDict, ["rightBorder", "id"]);
                this.leftBorderBiomeType = DictionaryHelper.TryGetInt(refDict, ["leftBorderBiomeType", "value"]);
                this.rightBorderBiomeType = DictionaryHelper.TryGetInt(refDict, ["rightBorderBiomeType", "value"]);
            }
        }
    }

    public class GameSession
    {
        // Primary Data
        // sessionSummary
        public readonly List<TurningPointPerformance> turningPointPerformances = [];
        public readonly List<string?> giantRosterDefs = [];
        public readonly string? scenarioDefinition;
        public readonly string? selectedCharacterDef;
        public readonly int? finalEra;
        public readonly bool? isTimeBasedChallenge, giantsRandomized, startingSpiritsRandomized;
        public readonly int? draftMode, rerollsPerEra, eventIntensity;
        public readonly int? challengeIndex, timedChallengeType, sessionDifficulty;
        // sessionSummary:planetSummary2
        // Biome sectors for planet preview bar
        public readonly List<BiomeSector> biomeSectors = [];
        public int? terribleFate;
        // sessionSummary:humanitySummary2
        public readonly List<CivSummary> civSummaries = [];

        public readonly bool? pacifismMode, planetIsLost;
        
        public GameSession(Dictionary<string, object> refDict)
        {
            // sessionSummary
            this.giantRosterDefs = DictionaryHelper.TryGetStringList(refDict, 
                ["sessionSummary", "startParameters", "giantRoster", "itemData"], 
                ["value", "Item2", "value"]);
            this.scenarioDefinition = DictionaryHelper.TryGetString(refDict, ["sessionSummary", "startParameters", "scenarioDefinition", "value"]);
            this.selectedCharacterDef = DictionaryHelper.TryGetString(refDict, ["sessionSummary", "startParameters", "selectedCharacter", "value"]);
            this.finalEra = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "finalEra", "value"]);

            this.isTimeBasedChallenge = DictionaryHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "isTimeBasedChallenge"]);
            this.giantsRandomized = DictionaryHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "giantsRandomized"]);
            this.startingSpiritsRandomized = DictionaryHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "startingSpiritsRandomized"]);
            this.draftMode = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.rerollsPerEra = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.eventIntensity = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.challengeIndex = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "challengeIndex"]);
            this.timedChallengeType = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "timedChallengeType", "value"]);
            this.pacifismMode = DictionaryHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "pacifismMode"]);
            this.sessionDifficulty = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "sessionDifficulty", "value"]);
            this.planetIsLost = DictionaryHelper.TryGetBool(refDict, "planetIsLost");

            List<object> tpDicts = (List<object>)DictionaryHelper.DigValueAtKeys(refDict, ["sessionSummary", "scoreCard", "turningPointPerformances", "itemData"]);
            foreach(Dictionary<string, object> tpd in tpDicts)
            {
                this.turningPointPerformances.Add(new TurningPointPerformance(tpd));
            }

            // planetSummary
            List<object> sectorDicts = (List<object>) DictionaryHelper.DigValueAtKeys(refDict, ["sessionSummary", "planetSummary2", "biomeSectors", "itemData"]);
            foreach (Dictionary<string,object> sd in sectorDicts)
            {
                this.biomeSectors.Add(new BiomeSector(sd));
            }
            this.terribleFate = DictionaryHelper.TryGetInt(refDict, ["sessionSummary", "planetSummary2", "terribleFate", "value"]);

            List<object> civDicts = (List<object>) DictionaryHelper.DigValueAtKeys(refDict, ["sessionSummary", "humanitySummary2", "civs", "itemData"]);
            foreach(Dictionary<string, object> cd in civDicts)
            {
                this.civSummaries.Add(new CivSummary(cd));
            }

        }

        public class TurningPointPerformance
        {
            public readonly string? turningPointDef, requestingCharacterDef;
            public readonly int? starRating, scoreTotal;

            public TurningPointPerformance(Dictionary<string, object> subDict)
            {
                this.turningPointDef = DictionaryHelper.TryGetString(subDict, ["value", "turningPoint", "value"]);
                this.requestingCharacterDef = DictionaryHelper.TryGetString(subDict, ["value", "requestingCharacter", "value"]);
                this.starRating = DictionaryHelper.TryGetInt(subDict, ["value","starRating"]);
                this.scoreTotal = DictionaryHelper.TryGetIntList(subDict, ["value", "scoreElements", "itemData"], ["value", "score"]).Sum();
            }
        }

        public class BiomeSector
        {
            public readonly int? typeDef;
            public readonly float? len;
            public readonly bool? hasCity;

            public BiomeSector(Dictionary<string, object> subDict)
            {
                this.typeDef = DictionaryHelper.TryGetInt(subDict, ["value", "biomeType", "value"]);
                this.len = DictionaryHelper.TryGetFloat(subDict, ["value", "sectorLength"]);
                this.hasCity = DictionaryHelper.TryGetBool(subDict, ["value", "hasCity"]);
            }
        }

        public class CivSummary
        {
            public readonly string? name;
            public readonly int? prosperity, population, wealth, innovation;
            public readonly string? characterDef, homeBiomeDef;
            public readonly List<string?> projectDefs;

            public CivSummary(Dictionary<string, object> subDict)
            {
                this.name = DictionaryHelper.TryGetString(subDict, ["value", "name"]);

                this.prosperity = DictionaryHelper.TryGetInt(subDict, ["value", "prosperity"]);
                this.population = DictionaryHelper.TryGetInt(subDict, ["value", "population"]);
                this.wealth = DictionaryHelper.TryGetInt(subDict, ["value", "wealth"]);
                this.innovation = DictionaryHelper.TryGetInt(subDict, ["value", "innovation"]);

                this.characterDef = DictionaryHelper.TryGetString(subDict, ["value", "character", "value"]);
                this.homeBiomeDef = DictionaryHelper.TryGetString(subDict, ["value", "homeBiome", "value"]);

                this.projectDefs = DictionaryHelper.TryGetStringList(subDict, 
                    ["value", "projects", "itemData"], 
                    ["value", "projectDefinition", "value"]);
            }
        }
    }
}
