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

        public readonly List<GenericBuff> BuffList = [];

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
        public readonly static List<string> genericBuffCheckKeys = [
            "_type", "definition", "owner", "isActive", "name", "parent" 
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
                if (refToken.TryGetValue("_type", out object patchTypeCheck) && (string)patchTypeCheck == "Patch" && patchCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.patchDictionary.Add(i, new Patch(refToken));
                    continue;
                }
                if (biomeCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.biomeDictionary.Add(i, new Biome(refToken));
                    continue;
                }
                if (refToken.TryGetValue("name", out object bioNameCheck) && (string)bioNameCheck == "NatureBioticum" && bioticumCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.natureBioticumDictionary.Add(i, new NatureBioticum(refToken));
                    continue;
                }
                if (cityCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.cityDictionary.Add(i, new City(refToken, referenceTokensList));
                    this.cityDictionary[i].tokenIndex = i;
                    continue;
                }
                if (refToken.TryGetValue("name", out object sessionNameCheck) && (string)sessionNameCheck == "Session" && sessionCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.gameSession = new GameSession(refToken);
                    continue;
                }
                if (refToken.TryGetValue("name", out object patchCollNameCheck) && (string)patchCollNameCheck == "PatchCollection" && patchCollectionCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.patchIdMap = new PatchMap<int?>(
                        DictHelper.TryGetIntList(refToken, ["models", "itemData"], "id"));
                    continue;
                }
                if (refToken.TryGetValue("name", out object gcNameCheck) && (string)gcNameCheck == "GameplayController" && gameplayControllerCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.MasteredBioticaDefSet.UnionWith(DictHelper.TryGetStringList(refToken, ["masteredBiotica", "itemData"], "value"));
                    continue;
                }
                if (refToken.TryGetValue("_type", out object buffTypeCheck) && (string)buffTypeCheck == "GenericBuff" && genericBuffCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.BuffList.Add(new(refToken));
                    continue;
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

            // Percentages of each biome
            Dictionary<string, int> patchesPerBiomeType = [];
            foreach (Biome biome in this.biomeDictionary.Values)
            {
                if (biome.anchorPatchId is null) continue;
                if (biome.biomeTypeInt is not null)
                {
                    biome.biomeTypeName = this.glossaries.GetBiomeNameFromInt((int)biome.biomeTypeInt);
                }
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
    }
}
