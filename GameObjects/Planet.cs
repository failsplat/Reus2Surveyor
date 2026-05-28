using DocumentFormat.OpenXml.Office.Drawing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace Reus2Surveyor.GameObjects
{
    // This is not deserialized directly
    // Instead, it is assembled by reading SaveRoot.referenceTokens 
    public class Planet
    {
        // Indexed based on order in referenceTokens
        // Top-level objects (identified from _type or name)
        public Dictionary<int, BioticumSlot> BioticumSlots = [];
        public Dictionary<int, Patch> Patches = [];
        public Dictionary<int, Biome> Biomes = [];
        public Dictionary<int, NatureBioticum> ActiveBiotica = [];

        public Dictionary<int, City> Cities = [];
        public Dictionary<int, CityControllers.ProjectController> CityProjectControllers = [];

        // 

        // Collections by gameplay-relevant indices
        public List<City> CitiesInOrder { get => [.. this.Cities.Values.OrderBy(city => city.cityIndex)]; }

        public Planet(SaveRoot sr, string path)
        {
            int i = -1;

            // Casting JO to game objects
            // Top-level objects (identified from _type or name)
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
                if (dummy.name?.StartsWith("City #") ?? false)
                {
                    City city = jo.ToObject<City>();
                    this.Cities.Add(i, city);
                    continue;
                }
                if (dummy.name == "ProjectController")
                {
                    CityControllers.ProjectController pc = jo.ToObject<CityControllers.ProjectController>();
                    this.CityProjectControllers.Add(i, pc);
                    continue;
                }
            }

            // Second-pass items (located by token index number)

            // Cross-referencing and collation
        }
    }
}
