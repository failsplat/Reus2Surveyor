using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reus2Surveyor
{
    public class Planet
    {
        public readonly string name;
        private Dictionary<int, BioticumSlot> slotDictionary;
        public Dictionary<int, BioticumSlot> SlotDictionary { get => slotDictionary; private set => slotDictionary = value; }
        private Dictionary<int, Patch> patchDictionary;
        public Dictionary<int, Patch> PatchDictionary { get => patchDictionary; private set => patchDictionary = value; }
        private readonly int totalSize;
        public int PatchCount { get => totalSize; }
        private readonly int wildSize;
        public int WildPatchCount { get => wildSize; }
        private PatchMap<int?> patchIDMap;
        public PatchMap<int?> PatchIDMap { get => patchIDMap; private set => patchIDMap = value; }

        //public List<int?> patchCollection;

        public static List<string> slotCheckKeys = [
            "bioticum", "futureSlot", "patch", "locationOnPatch", "slotLevel", "parent",
                "isInvasiveSlot", "slotbonusDefinitions", "archivedBiotica", "name"
            ];
        public static List<string> patchCheckKeys = [
            "foregroundSlot", "backgroundSlot", "mountainSlot",
                "projectSlots", "_type", "planet", "biomeDefinition", "patchVariation",
                "currentBackdropMode", "colorsAndParameters", "elevation", "windAndWater", "specialNaturalFeature", "mountainPart"
            ];
        /*public static List<string> patchCollectionCheckKeys = [
            "models", "name", "parent",
            ];*/
        public static List<string> planetCloneCheckKeys = [
            "patches", "rivers", "biomeSaveData", "puzzle", "animalController", "allPropVisuals", "name"
            ];

        public Planet(List<object> referenceTokensList)
        {
            this.slotDictionary = [];
            this.patchDictionary = [];

            int i = 0;
            foreach (Dictionary<string, object> refToken in referenceTokensList)
            {
                List<string> rtKeys = [.. refToken.Keys];
                if (slotCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.slotDictionary.Add(i, new BioticumSlot(refToken));
                    i++;
                    continue;
                }
                if (patchCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["_type"] == "Patch")
                {
                    this.patchDictionary.Add(i, new Patch(refToken));
                    i++;
                    continue;
                }
                if (planetCloneCheckKeys.All(k => rtKeys.Contains(k)))
                {
                    this.patchIDMap = new PatchMap<int?>(DictionaryHelper.TryGetIntList(refToken, ["representedBackdrops", "itemData"], ["value", "Item1", "id"]));
                }

                /*if (patchCollectionCheckKeys.All(k => rtKeys.Contains(k)) && (string)refToken["name"] == "PatchCollection")
                {
                    this.patchCollection = DictionaryHelper.TryGetIntList(refToken, ["models", "itemData"], "id");
                    i++;
                    continue;
                }*/

                // Yay incrementer
                i++;
            }
            this.totalSize = this.patchDictionary.Count;
            this.wildSize =  this.patchDictionary.Where(kvp => kvp.Value.IsWildPatch()).Count();
        }
    }

    public class BioticumSlot
    {
        private int? bioticum, patch, locationOnPatch, slotLevel, parent;
        public readonly int? futureSlot;
        public int? BioticumRef { get => bioticum; private set => bioticum = value; }
        public int? PatchRef { get => patch; private set => patch = value; }
        public int? LocationOnPatch { get => locationOnPatch; private set => locationOnPatch = value; }
        public int? SlotLevel { get => slotLevel; private set => slotLevel = value; }
        public int? ParentRef { get => parent; private set => parent = value; }

        private bool isInvasiveSlot;
        public bool IsInvasiveSlot { get => isInvasiveSlot; private set => isInvasiveSlot = value; }
        private List<string> slotbonusDefinitions;
        public List<string> SlotBonusDefinitions { get => slotbonusDefinitions; private set => slotbonusDefinitions = value; }
        private List<Dictionary<string, object>> archivedBiotica;
        public List<Dictionary<string, object>> LegacyBiotica { get => archivedBiotica; private set => archivedBiotica = value; }
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
        private int? foregroundSlot, backgroundSlot, mountainSlot, mountainPart;
        public int? ForegroundSlotRef { get => foregroundSlot; private set => foregroundSlot = value; }
        public int? BackgroundSlotRef { get => backgroundSlot; private set => backgroundSlot = value; }
        public int? MountainSlot { get => mountainSlot; private set => mountainSlot = value; }
        private List<int?> projectSlots;
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

}
