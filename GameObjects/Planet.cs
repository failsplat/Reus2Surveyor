using DocumentFormat.OpenXml.Office.Drawing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime;

namespace Reus2Surveyor.GameObjects
{
    // This is not deserialized directly
    // Instead, it is assembled by reading SaveRoot.referenceTokens 
    public class Planet
    {
        public Dictionary<int, BioticumSlot> BioticumSlots = [];
        public Dictionary<int, Patch> Patches = [];
        public Dictionary<int, Biome> Biomes = [];
        public Dictionary<int, NatureBioticum> ActiveBiotica = [];

        public Planet(SaveRoot sr, string path)
        {
            int i = -1;

            // Casting JO to game objects
            foreach (JToken jo in sr.referenceTokens) 
            {
                i++;
                Dummy dummy = jo.ToObject<Dummy>();
                if (dummy.name?.StartsWith("BioticumSlot") ?? false)
                {
                    BioticumSlot slot = jo.ToObject<BioticumSlot>();
                    this.BioticumSlots.Add(i, slot);
                    continue;
                }
                if (dummy._type == "Patch")
                {
                    Patch patch = jo.ToObject<Patch>();
                    this.Patches.Add(i, patch);
                    continue;
                }
                if (dummy.name == "BiomeModelData")
                {
                    Biome biome = jo.ToObject<Biome>();
                    this.Biomes.Add(i, biome);
                    continue;
                }
                if (dummy._type == "NatureBioticum")
                {
                    NatureBioticum bio = jo.ToObject<NatureBioticum>();
                    this.ActiveBiotica.Add(i, bio);
                    continue;
                }
            }

            // Cross-referencing and collation
        }
    }
}
