using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static Reus2Surveyor.Glossaries;

namespace Reus2Surveyor
{
    public class Planet
    {
        public readonly string name;
        public readonly int epochMinutes;
        public int number = -1;
        public readonly string path;
        public readonly string debugName;
        public readonly Dictionary<int, BioticumSlot> slotDictionary = [];
        public readonly Dictionary<int, Patch> patchDictionary = [];
        public readonly Dictionary<int, Biome> biomeDictionary = [];
        public readonly Dictionary<int, NatureBioticum> natureBioticumDictionary = []; // By end, active biotica only!
        public readonly Dictionary<int, City> cityDictionary = [];
        public readonly int totalSize;
        public readonly int wildSize;
        public readonly PatchMap<int?> patchIdMap;
        public readonly GameSession gameSession;

        public readonly HashSet<int> futureSlotIndices = [];
        public readonly HashSet<int> inactiveBioticumIndices = [];
        public readonly Dictionary<int, NatureBioticum> inactiveBioticumDictionary = [];

        // These are for debugging and checking counts for planets
        // Needs more information for the stats I want to track
        public readonly Dictionary<string, int> BioticaCounterDefs = [];
        public Dictionary<string, int> BioticaCounterNames { get; private set; } = [];
        public readonly Dictionary<string, int> LegacyBioticaCounterDefs = [];
        public Dictionary<string, int> LegacyBioticaCounterNames { get; private set; } = [];

        public HashSet<string> MasteredBioticaDefSet { get; private set; } = [];

        public List<string> GiantNames { get; private set; } = [];
        public Dictionary<string, double> BiomePercentages = [];
        public Dictionary<string, int> BiomePatchCounts = [];
        public Dictionary<int, (string biomeTypeName, double percentSize)> BiomeSizeMap = [];

        private Glossaries glossaries;

        //public List<int?> patchCollection;

        // TODO: Find a better way to find objects other than checking dictionary keys
        public readonly static List<string> slotCheckKeys = [
            "bioticum", "futureSlot", "patch", "locationOnPatch", "slotLevel", "parent",
                "slotbonusDefinitions", "archivedBiotica", "name"
            ];
        public readonly static List<string> patchCheckKeys = [
            "foregroundSlot", "backgroundSlot", "mountainSlot",
                "projectSlots", "_type", "biomeDefinition", "mountainPart"
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
            "aspectSlots", "_type", "definition", "receivedRiverBonus",
            "name", "parent"
            ];
        public readonly static List<string> cityCheckKeys = [
            "projectController", "resourceController", "luxuryController", "fancyName",
            "leftNeighbour", "rightNeighbour", // British Spelling
            "biomeOrigin", "initiatedTurningPoints", "nomadHeritage", "currentVisualStage", "projectSlots",
            ];
        public readonly static List<string> sessionCheckKeys = [
            "gameplayController", "startParameters", "sessionID", "isFinished", "freePlay", "name"
            ];
        public readonly static List<string> gameplayControllerCheckKeys = [
            "aspectController", "gameplayShop", "masteredBiotica", "name",
            ];

        public Planet(List<object> referenceTokensList, string planetPath)
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
                if (bioticumCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["name"] == "NatureBioticum")
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
                        DictHelper.TryGetIntList(refToken, ["models", "itemData"], "id"));
                    continue;
                }
                if (gameplayControllerCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["name"] == "GameplayController")
                {
                    this.MasteredBioticaDefSet.UnionWith(DictHelper.TryGetStringList(refToken, ["masteredBiotica", "itemData"], "value"));
                }
            }

            this.path = planetPath;
            this.name = PlanetFileUtil.PlanetNameFromSaveFilePath(planetPath);
            this.epochMinutes = PlanetFileUtil.EpochMinutesFromSaveFilePath(planetPath);
            List<string> pathParts = [.. this.path.Split(Path.DirectorySeparatorChar)];
            pathParts.Reverse();
            this.debugName = pathParts[1] + Path.DirectorySeparatorChar + pathParts[0];

            // Trace out warnings if any of the core dictionaries are empty
            if (slotDictionary.Count == 0) Trace.TraceWarning(String.Format("Empty slotDictionary in {0}", this.debugName));
            if (patchDictionary.Count == 0) Trace.TraceWarning(String.Format("Empty patchDictionary in {0}", this.debugName));
            if (natureBioticumDictionary.Count == 0) Trace.TraceWarning(String.Format("Empty natureBioticumDictionary in {0}", this.debugName));
            if (cityDictionary.Count == 0) Trace.TraceWarning(String.Format("Empty cityDictionary in {0}", this.debugName));
            if (biomeDictionary.Count == 0) Trace.TraceWarning(String.Format("Empty biomeDictionary in {0}", this.debugName));


            // Secondary Data (calculated when all game objects parsed)
            this.totalSize = this.patchDictionary.Count;
            this.wildSize = this.patchDictionary.Where(kvp => kvp.Value.IsWildPatch()).Count();
            foreach (Biome b in this.biomeDictionary.Values)
            {
                b.BuildPatchInfo(this.patchIdMap, this.patchDictionary);
            }

            // Remove biotica with ID 0
            /*List<int> inactiveBiotica = [];
            foreach (KeyValuePair<int,NatureBioticum> kv in this.natureBioticumDictionary)
            {
                if (!kv.Value.IsActive())
                {
                    inactiveBiotica.Add(kv.Key);
                    //if (this.glossaries is not null) kv.Value.CheckName(this.glossaries);
                }
            }
            foreach(int removeIndex in inactiveBiotica)
            {
                this.natureBioticumDictionary.Remove(removeIndex);
            }*/


            // Remove any biotica on futureSlots
            foreach (BioticumSlot bs in this.slotDictionary.Values)
            {
                if (bs.futureSlotId is not null) this.futureSlotIndices.Add((int)bs.futureSlotId);
            }
            foreach (KeyValuePair<int, NatureBioticum> kv in this.natureBioticumDictionary)
            {
                if (kv.Value.slotId is not null)
                {
                    if (this.futureSlotIndices.Contains((int)kv.Value.slotId))
                    {
                        this.inactiveBioticumIndices.Add(kv.Key);
                    }
                }
            }
            foreach (int ib in this.inactiveBioticumIndices)
            {
                this.inactiveBioticumDictionary.Add(ib, this.natureBioticumDictionary[ib]);
                this.natureBioticumDictionary.Remove(ib);
            }

            // Count active biotica
            foreach (NatureBioticum nb in this.natureBioticumDictionary.Values)
            {
                if (nb.definition is null) continue;
                if (this.BioticaCounterDefs.ContainsKey(nb.definition))
                {
                    this.BioticaCounterDefs[nb.definition] += 1;
                }
                else
                {
                    this.BioticaCounterDefs[nb.definition] = 1;
                }
            }

            // Count legacy biotica
            foreach (BioticumSlot slot in this.slotDictionary.Values)
            {
                foreach (string archivedBioticumDef in slot.archivedBioticaDefs)
                {
                    if (archivedBioticumDef is null) continue;
                    if (this.LegacyBioticaCounterDefs.ContainsKey(archivedBioticumDef))
                    {
                        this.LegacyBioticaCounterDefs[archivedBioticumDef] += 1;
                    }
                    else
                    {
                        this.LegacyBioticaCounterDefs[archivedBioticumDef] = 1;
                    }
                }
            }

            // City information
            foreach (City city in this.cityDictionary.Values)
            {
                city.BuildTerritoryInfo(this.patchIdMap, this.patchDictionary);
                city.CountTerritoryBiotica(this.slotDictionary, this.natureBioticumDictionary);
            }

            List<ValueTuple<City, GameSession.CivSummary>> CitySummaryPairs = [.. this.cityDictionary.Values.Zip(this.gameSession.civSummaries).ToList()];
            foreach (ValueTuple<City, GameSession.CivSummary> cc in CitySummaryPairs)
            {
                cc.Item1.AttachCivSummary(cc.Item2);
            }

            // Percentages of each biome
            Dictionary<string, int> patchesPerBiomeType = [];
            foreach (Biome biome in this.biomeDictionary.Values)
            {
                if (biome.anchorPatchId is null) continue;

                string biomeType = biome.biomeTypeName;

                if (!patchesPerBiomeType.ContainsKey(biomeType)) patchesPerBiomeType[biomeType] = 0;
                patchesPerBiomeType[biomeType] += biome.totalSize;
            }
            this.BiomePatchCounts = patchesPerBiomeType;
            this.BiomePercentages = patchesPerBiomeType
                .Select(kv => new KeyValuePair<string, double>(kv.Key, (double)kv.Value / (double)this.totalSize))
                .ToDictionary();
            this.BiomeSizeMap =
                this.biomeDictionary.Values
                .Where(b => b.anchorPatchId is not null)
                .Select(b => new KeyValuePair<int, (string, double)>(
                    (int)b.anchorPatchId, (b.biomeTypeName, (double)b.totalSize / (double)this.totalSize))
                )
                .ToDictionary()
                ;
        }

        public void SetGlossaryThenLookup(Glossaries g)
        {
            this.glossaries = g;
            foreach (NatureBioticum b in this.natureBioticumDictionary.Values)
            {
                b.CheckName(this.glossaries);
            }

            foreach (KeyValuePair<string, int> kv in this.LegacyBioticaCounterDefs)
            {
                this.LegacyBioticaCounterNames[this.glossaries.BioticumNameFromHash(kv.Key)] = kv.Value;
            }
            foreach (KeyValuePair<string, int> kv in this.BioticaCounterDefs)
            {
                this.BioticaCounterNames[this.glossaries.BioticumNameFromHash(kv.Key)] = kv.Value;
            }

            List<GiantDefinition> giantDefs = [.. this.gameSession.giantRosterDefs.Select(v => glossaries.TryGiantDefinitionFromHash(v))];
            giantDefs.Sort((x, y) => x.Position - y.Position);
            this.GiantNames = [.. giantDefs.Select(v => v.Name)];
        }
    }

    public class BioticumSlot
    {
        // Primary Data
        public readonly int? bioticumId, patchId, locationOnPatch, slotLevel;
        public readonly int? futureSlotId;

        public readonly bool? isInvasiveSlot;
        public readonly List<string> slotbonusDefinitions;
        public readonly List<Dictionary<string, object>> archivedBiotica;
        public readonly List<string> archivedBioticaDefs;
        public readonly string name;

        public BioticumSlot(Dictionary<string, object> refDict)
        {
            this.bioticumId = DictHelper.TryGetInt(refDict, ["bioticum", "id"]);
            this.futureSlotId = DictHelper.TryGetInt(refDict, ["futureSlot", "id"]);
            this.patchId = DictHelper.TryGetInt(refDict, ["patch", "id"]);

            // 0 = Foreground
            // 1 = Background
            // 2 = Mountain 
            this.locationOnPatch = DictHelper.TryGetInt(refDict, ["locationOnPatch", "value"]);

            this.slotbonusDefinitions = DictHelper.TryGetStringList(refDict, ["slotbonusDefinitions", "itemData"], "itemData");

            this.slotLevel = DictHelper.TryGetInt(refDict, "slotLevel");

            this.archivedBiotica = DictHelper.TryGetDictList(refDict, ["archivedBiotica", "itemData"], "value");
            this.archivedBioticaDefs = DictHelper.TryGetStringList(refDict, ["archivedBiotica", "itemData"], ["value", "bioticum", "value"]);

            this.isInvasiveSlot = DictHelper.TryGetBool(refDict, "isInvasiveSlot");

            // "BioticumSlot (<patch> - <position>)
            this.name = DictHelper.TryGetString(refDict, "name");
        }
    }

    public class Patch
    {
        // Primary Data
        public readonly int? foregroundSlotId, backgroundSlotId, mountainSlotId, mountainPart;
        private readonly List<int?> projectSlotsIds;
        public readonly string biomeDefinition, name;
        private readonly object ruinedCityMemory;

        // Patch-specific features
        // 1 = Creek, 2 = Sanctuary, 3 = Anomaly
        public readonly int? specialNaturalFeature;

        public Patch(Dictionary<string, object> refDict)
        {
            this.foregroundSlotId = DictHelper.TryGetInt(refDict, ["foregroundSlot", "id"]);
            this.backgroundSlotId = DictHelper.TryGetInt(refDict, ["backgroundSlot", "id"]);
            this.mountainSlotId = DictHelper.TryGetInt(refDict, ["mountainSlot", "id"]);

            this.projectSlotsIds = [];
            List<object> projectSlotDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["projectSlots", "itemData"]);

            this.projectSlotsIds = DictHelper.TryGetIntList(refDict, ["projectSlots", "itemData"], "id");
            this.biomeDefinition = DictHelper.TryGetString(refDict, ["biomeDefinition", "value"]);

            this.mountainPart = DictHelper.TryGetInt(refDict, ["mountainPart", "value"]);
            if (refDict.ContainsKey("ruinedCityMemory")) this.ruinedCityMemory = refDict["ruinedCityMemory"];
            this.name = (string)refDict["name"];

            this.specialNaturalFeature = DictHelper.TryGetInt(refDict, ["specialNaturalFeature", "value"]);
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
                return this.Slice(leftIndex, rightIndex - leftIndex + 1);
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
            this.anchorPatchId = DictHelper.TryGetInt(refDict, ["anchorPatch", "id"]);
            this.visualName = DictHelper.TryGetString(refDict, ["visualName"]);
            this.biomeTypeInt = DictHelper.TryGetInt(refDict, ["biomeType", "value"]);
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
            this.wildPatchList = [.. this.patchList.Where(x => patchDict[(int)x].IsWildPatch())];
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
        public readonly bool? receivedRiverBonus, anomalyBonusActive;
        public readonly int? slotId;

        // Secondary Data
        public bool IsOnMountain { get; private set; }
        public bool HasMicro { get; private set; }
        public string BioticumName { get; private set; }

        public NatureBioticum(Dictionary<string, object> refDict)
        {
            this.aspectSlotsIds = DictHelper.TryGetIntList(refDict, ["aspectSlots", "itemData"], "id");

            // Older saves don't have this, so it can't be used to tell if a bioticum is active
            if (refDict.ContainsKey("bioticumId"))
            {
                this.bioticumId = DictHelper.TryGetInt(refDict, "bioticumID");
            }
            else
            {

            }

            this.definition = DictHelper.TryGetString(refDict, ["definition", "value"]);

            this.receivedRiverBonus = DictHelper.TryGetBool(refDict, "receivedRiverBonus");
            this.anomalyBonusActive = DictHelper.TryGetBool(refDict, "anomalyBonusActve"); // [sic]

            this.slotId = DictHelper.TryGetInt(refDict, ["parent", "id"]);
        }

        public void CheckSlotProperties(Dictionary<int, BioticumSlot> slotDict)
        {
            if (this.slotId is null || this.bioticumId is null) return;
            BioticumSlot thisSlot = slotDict[(int)this.slotId];
            this.IsOnMountain = thisSlot.locationOnPatch == 2;
        }
        // TODO: (Maybe) check that the aspect slot(s) have placed micros

        public void CheckName(Glossaries g)
        {
            this.BioticumName = g.BioticumNameFromHash(this.definition);
        }

        //public bool IsActive() { return this.bioticumId is null ? false : this.bioticumId > 0; }
    }

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
            public LuxuryController(Dictionary<string, object> refDict, List<object> referenceTokensList)
            {
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

    public class GameSession
    {
        // Primary Data
        // sessionSummary
        public readonly List<TurningPointPerformance> turningPointPerformances = [];
        public readonly List<string?> giantRosterDefs = [];
        public readonly string? scenarioDefinition;
        public readonly string? selectedCharacterDef;
        public readonly int? finalEra;
        public readonly bool? isTimeBasedChallenge, giantsRandomized, startingSpiritRandomized;
        public readonly int? draftMode, rerollsPerEra, eventIntensity;
        public readonly int? challengeIndex, timedChallengeType, sessionDifficulty;
        // sessionSummary:planetSummary2
        // Biome sectors for planet preview bar
        public readonly List<BiomeSector> biomeSectors = [];
        public int? terribleFate;
        // sessionSummary:humanitySummary2
        public readonly List<CivSummary> civSummaries = [];
        public readonly int? coolBiomes;

        public readonly bool? pacifismMode, planetIsLost;

        public GameSession(Dictionary<string, object> refDict)
        {
            // sessionSummary
            this.giantRosterDefs = DictHelper.TryGetStringList(refDict,
                ["sessionSummary", "startParameters", "giantRoster", "itemData"],
                ["value", "Item2", "value"]);
            this.scenarioDefinition = DictHelper.TryGetString(refDict, ["sessionSummary", "startParameters", "scenarioDefinition", "value"]);
            this.selectedCharacterDef = DictHelper.TryGetString(refDict, ["sessionSummary", "startParameters", "selectedCharacter", "value"]);
            this.finalEra = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "finalEra", "value"]);

            this.isTimeBasedChallenge = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "isTimeBasedChallenge"]);
            this.giantsRandomized = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "giantsRandomized"]);
            this.startingSpiritRandomized = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "startingSpiritRandomized"]);
            this.draftMode = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.rerollsPerEra = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.eventIntensity = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.challengeIndex = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "challengeIndex"]);
            this.timedChallengeType = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "timedChallengeType", "value"]);
            this.pacifismMode = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "pacifismMode"]);
            this.sessionDifficulty = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "sessionDifficulty", "value"]);
            this.planetIsLost = DictHelper.TryGetBool(refDict, "planetIsLost");
            this.coolBiomes = DictHelper.TryGetInt(refDict, ["sessionSummary", "coolBiomes"]);

            List<object> tpDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["sessionSummary", "scoreCard", "turningPointPerformances", "itemData"]);
            if (tpDicts is not null)
            {
                foreach (Dictionary<string, object> tpd in tpDicts)
                {
                    this.turningPointPerformances.Add(new TurningPointPerformance(tpd));
                }
            }

            // planetSummary
            List<object> sectorDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["sessionSummary", "planetSummary2", "biomeSectors", "itemData"]);
            foreach (Dictionary<string, object> sd in sectorDicts)
            {
                this.biomeSectors.Add(new BiomeSector(sd));
            }
            this.terribleFate = DictHelper.TryGetInt(refDict, ["sessionSummary", "planetSummary2", "terribleFate", "value"]);

            List<object> civDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["sessionSummary", "humanitySummary2", "civs", "itemData"]);
            foreach (Dictionary<string, object> cd in civDicts)
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
                this.turningPointDef = DictHelper.TryGetString(subDict, ["value", "turningPoint", "value"]);
                this.requestingCharacterDef = DictHelper.TryGetString(subDict, ["value", "requestingCharacter", "value"]);
                this.starRating = DictHelper.TryGetInt(subDict, ["value", "starRating"]);
                this.scoreTotal = DictHelper.TryGetIntList(subDict, ["value", "scoreElements", "itemData"], ["value", "score"]).Sum();
            }
        }

        public class BiomeSector
        {
            public readonly int? typeDef;
            public readonly float? len;
            public readonly bool? hasCity;

            public BiomeSector(Dictionary<string, object> subDict)
            {
                this.typeDef = DictHelper.TryGetInt(subDict, ["value", "biomeType", "value"]);
                this.len = DictHelper.TryGetFloat(subDict, ["value", "sectorLength"]);
                this.hasCity = DictHelper.TryGetBool(subDict, ["value", "hasCity"]);
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
                this.name = DictHelper.TryGetString(subDict, ["value", "name"]);

                this.prosperity = DictHelper.TryGetInt(subDict, ["value", "prosperity"]);
                this.population = DictHelper.TryGetInt(subDict, ["value", "population"]);
                this.wealth = DictHelper.TryGetInt(subDict, ["value", "wealth"]);
                this.innovation = DictHelper.TryGetInt(subDict, ["value", "innovation"]);

                this.characterDef = DictHelper.TryGetString(subDict, ["value", "character", "value"]);
                this.homeBiomeDef = DictHelper.TryGetString(subDict, ["value", "homeBiome", "value"]);

                this.projectDefs = DictHelper.TryGetStringList(subDict,
                    ["value", "projects", "itemData"],
                    ["value", "projectDefinition", "value"]);
            }
        }
    }
}
