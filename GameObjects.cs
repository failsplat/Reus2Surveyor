using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.InteropServices.Swift;
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
        public readonly int totalSize;
        public readonly int wildSize;
        public readonly PatchMap<int?> patchIDMap;

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
                if (patchCollectionCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["name"] == "PatchCollection")
                {
                    this.patchIDMap = new PatchMap<int?>(
                        DictionaryHelper.TryGetIntList(refToken, ["models", "itemData"], "id"));
                    continue;
                }
                if (biomeCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.biomeDictionary.Add(i, new Biome(refToken));
                    continue;
                }                
            }

            // Secondary Data (calculated when all game objects parsed)
            this.totalSize = this.patchDictionary.Count;
            this.wildSize =  this.patchDictionary.Where(kvp => kvp.Value.IsWildPatch()).Count();
            foreach (Biome b in this.biomeDictionary.Values)
            {
                b.BuildPatchInfo(this.patchIDMap, this.patchDictionary);
            }
        }
    }

    public class BioticumSlot
    {
        public readonly int? bioticum, patch, locationOnPatch, slotLevel, parent;
        public readonly int? futureSlot;

        public readonly bool isInvasiveSlot;
        public readonly List<string> slotbonusDefinitions;
        public readonly List<Dictionary<string, object>> archivedBiotica;
        public readonly string name;

        public BioticumSlot(Dictionary<string, object> dict)
        {
            this.bioticum = DictionaryHelper.TryGetInt(dict, ["bioticum", "id"]);
            this.futureSlot = DictionaryHelper.TryGetInt(dict, ["futureSlot", "id"]);
            this.patch = DictionaryHelper.TryGetInt(dict, ["patch", "id"]);
            this.locationOnPatch = DictionaryHelper.TryGetInt(dict, ["locationOnPatch", "value"]);

            this.slotbonusDefinitions = DictionaryHelper.TryGetStringList(dict, ["slotbonusDefinitions", "itemData"], "itemData");

            this.slotLevel = (int)(long)dict["slotLevel"];

            this.archivedBiotica = DictionaryHelper.TryGetDictList(dict, ["archivedBiotica", "itemData"], "value");
            
            this.isInvasiveSlot = (bool)dict["isInvasiveSlot"];
            this.name = (string)dict["name"];
            this.parent = DictionaryHelper.TryGetInt(dict, ["parent", "id"]);
        }
    }

    public class Patch
    {
        public readonly int? foregroundSlot, backgroundSlot, mountainSlot, mountainPart;
        private readonly List<int?> projectSlots;
        public readonly string biomeDefinition, name;
        private readonly object ruinedCityMemory;

        public Patch(Dictionary<string, object> dict)
        {
            this.foregroundSlot = DictionaryHelper.TryGetInt(dict, ["foregroundSlot", "id"]);
            this.backgroundSlot = DictionaryHelper.TryGetInt(dict, ["backgroundSlot", "id"]);
            this.mountainSlot = DictionaryHelper.TryGetInt(dict, ["mountainSlot", "id"]);

            this.projectSlots = [];
            List<object> projectSlotDicts = (List<object>)DictionaryHelper.DigValueAtKeys(dict, ["projectSlots", "itemData"]);

            this.projectSlots = DictionaryHelper.TryGetIntList(dict, ["projectSlots", "itemData"], "id");
            this.biomeDefinition = DictionaryHelper.TryGetString(dict, ["biomeDefinition", "value"]);

            this.mountainPart = DictionaryHelper.TryGetInt(dict, ["mountainPart", "value"]);
            this.ruinedCityMemory = dict["ruinedCityMemory"];
            this.name = (string)dict["name"];
        }

        public bool IsWildPatch()
        {
            return (this.projectSlots.Count == 0);
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

        public List<T> PatchIndexSlice(int leftIndex, int rightIndex)
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

        public List<T> PatchIDSlice(T leftID, T rightID)
        {
            int leftIndex = this.IndexOf(leftID);
            int rightIndex = this.IndexOf(rightID);
            if (leftIndex == -1 || rightIndex == -1) return [];
            else return PatchIndexSlice(leftIndex, rightIndex);
        }

    }

    public class Biome
    {
        public readonly int? anchorPatch;
        public readonly string visualName;
        public readonly int? biomeTypeInt;
        public readonly string biomeTypeName;

        public string biomeTypeDef { get; private set; }
        public List<int?> patchList { get; private set; }
        public List<int?> wildPatchList { get; private set; }
        public int totalSize { get; private set; }
        public int wildSize { get; private set; }

        public Biome(Dictionary<string, object> dict)
        {
            this.anchorPatch = DictionaryHelper.TryGetInt(dict, ["anchorPatch", "id"]);
            this.visualName = DictionaryHelper.TryGetString(dict, ["visualName"]);
            this.biomeTypeInt = DictionaryHelper.TryGetInt(dict, ["biomeType", "value"]);
            if (this.biomeTypeInt is not null) this.biomeTypeName = Glossaries.BiomeNameByInt[(int)this.biomeTypeInt];
        }

        public void BuildPatchInfo(PatchMap<int?> patchMap, Dictionary<int, Patch> patchDict)
        {
            if (this.anchorPatch is null)
            {
                this.patchList = [];
                this.wildPatchList = [];
                this.totalSize = this.patchList.Count;
                this.wildSize = this.wildPatchList.Count;
                return;
            }

            int anchorMapPosition = patchMap.IndexOf(this.anchorPatch);
            Patch anchorPatchObj = patchDict[(int)this.anchorPatch];
            this.biomeTypeDef = anchorPatchObj.biomeDefinition;

            List<int?> leftPatches = [];
            List<int?> rightPatches = [];

            for (int leftMapPosition = anchorMapPosition - 1; (patchMap.Count + (leftMapPosition % patchMap.Count)) % patchMap.Count != anchorMapPosition; leftMapPosition--)
            {
                int leftPatchIndex = (int)patchMap[(patchMap.Count + (leftMapPosition % patchMap.Count)) % patchMap.Count];
                string leftPatchBiomeDef = patchDict[leftPatchIndex].biomeDefinition;
                if (leftPatchBiomeDef == this.biomeTypeDef) leftPatches.Insert(0, leftPatchIndex);
                else break;
            }
            for (int rightMapPosition = anchorMapPosition + 1; rightMapPosition % patchMap.Count != anchorMapPosition; rightMapPosition++)
            {
                int rightPatchIndex = (int)patchMap[rightMapPosition % patchMap.Count];
                string rightPatchBiomeDef = patchDict[rightPatchIndex].biomeDefinition;
                if (rightPatchBiomeDef == this.biomeTypeDef) rightPatches.Add(rightPatchIndex);
                else break;
            }

            rightPatches.Insert(0, this.anchorPatch);
            leftPatches.AddRange(rightPatches);
            this.patchList = leftPatches;
            this.wildPatchList = [..this.patchList.Where(x => patchDict[(int)x].IsWildPatch())];
            this.totalSize = this.patchList.Count;
            this.wildSize = this.wildPatchList.Count;
        }
    }

    public class Bioticum
    {

    }
}
