using System.Collections.Generic;
using System.Linq;

namespace Reus2Surveyor
{
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
        public string biomeTypeName;

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
}
