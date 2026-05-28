using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class Biome
    {
        [JsonProperty(Required = Required.Always)] public Id<int> biomeBuffs { get; init; }
        [JsonProperty(Required = Required.Always)] public Id<int?> anchorPatch { get; init; }
        [JsonProperty(Required = Required.Always)] public string visualName { get; init; }
        public string namePrefix { get; init; }
        public string nameSuffix { get; init; }
        public bool isPolluted { get; init; }
        public bool nameOnlySuffixContainsTheme { get; init; }
        [JsonProperty(Required = Required.Always)] public Value<int> biomeType { get; init; }
        //public string name { get; init; }
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

        //
        public List<Patch> PatchesInBiome { get; private set; } = [];
        public List<Patch> WildPatchesInBiome { get; private set; } = [];
        public string? BiomeTypeDef { get; private set; } = null;
        public int TotalSize { get => this.PatchesInBiome.Count; }
        public int WildSize { get => this.WildPatchesInBiome.Count; }
        internal void BuildPatchInfo(PatchMap<int>? patchMap, Dictionary<int, Patch> patches)
        {
            if (this.anchorPatch.id is null)
            {
                return;
            }

            int anchorId = (int)this.anchorPatch.id;
            int anchorPosition = patchMap.IndexOf(anchorId);
            Patch anchorPatchObj = patches[anchorId];
            this.BiomeTypeDef = anchorPatchObj.biomeDefinition.value;

            List<int?> leftPatches = [];
            List<int?> rightPatches = [];

            for (int leftMapPosition = anchorPosition - 1; (patchMap.Count + (leftMapPosition % patchMap.Count)) % patchMap.Count != anchorPosition; leftMapPosition--)
            {
                int leftPatchIndex = (int)patchMap[(patchMap.Count + (leftMapPosition % patchMap.Count)) % patchMap.Count];
                if (!patches.ContainsKey(leftPatchIndex)) break;
                string leftPatchBiomeDef = patches[leftPatchIndex].biomeDefinition.value;
                if (leftPatchBiomeDef == this.BiomeTypeDef) leftPatches.Insert(0, leftPatchIndex);
                else break;
            }
            for (int rightMapPosition = anchorPosition + 1; rightMapPosition % patchMap.Count != anchorPosition; rightMapPosition++)
            {
                int rightPatchIndex = (int)patchMap[rightMapPosition % patchMap.Count];
                if (!patches.ContainsKey(rightPatchIndex)) break;
                string rightPatchBiomeDef = patches[rightPatchIndex].biomeDefinition.value;
                if (rightPatchBiomeDef == this.BiomeTypeDef) rightPatches.Add(rightPatchIndex);
                else break;
            }

            rightPatches.Insert(0, anchorId);
            leftPatches.AddRange(rightPatches);

            this.PatchesInBiome = [..leftPatches.Select(i => patches[(int)i])];
            this.WildPatchesInBiome = [.. this.PatchesInBiome.Where(p => p.IsWild)];
        }
    }
}
