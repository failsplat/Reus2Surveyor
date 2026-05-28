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
        public Dictionary<int, NatureBioticum> AllBiotica = [];
        public Dictionary<int, NatureBioticum> ActiveBiotica; // Filled out on cross-referencing

        public Dictionary<int, City> Cities = [];
        public Dictionary<int, CityControllers.ProjectController> CityProjectControllers = [];

        public PatchMap<int> PlanetPatchMap;

        // "Hidden" objects - can't identified by _type/name, located by reference from other object
        // Alternately this could be done by try-catching the deserialization or other way of matching the schema
        public Dictionary<int, CityObjects.Project> CityProjects = [];

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
                    this.AllBiotica.Add(i, bio);
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
                if (dummy.name == "PatchCollection")
                {
                    PatchCollection pc = jo.ToObject<PatchCollection>();
                    this.PlanetPatchMap = new(pc.IdList);
                }
            }

            // Second-pass items (located by token index number)

            

            // Filtering out irrelevant objects
            // CitySlots
            HashSet<int> citySlots = [.. this.BioticumSlots.Where(kv => kv.Value.locationOnPatch.value == 3).Select(kv => kv.Key)];
            foreach (int cs in citySlots) this.BioticumSlots.Remove(cs);
            // Biotica on future slots
            HashSet<int> futureSlots = [.. this.BioticumSlots.Values.Select(s => s.futureSlot.id)];
            this.ActiveBiotica = this.AllBiotica.Where(kv => !futureSlots.Contains((int)kv.Value.parent.id)).ToDictionary();

            // Cross-referencing and collation
            // Call method on parent object to put child objects in its properties
            // Parent object calls method on child object to put itself in its properties
            foreach (Patch patch in this.Patches.Values)
            {
                patch.FindSlots(this.BioticumSlots);
            }
            foreach (BioticumSlot slot in this.BioticumSlots.Values)
            {
                slot.FindBiotica(this.AllBiotica);
            }
            // Put patches in biome from PatchMap and dictionary of patches
            foreach (Biome b in this.Biomes.Values)
            {
                b.BuildPatchInfo(this.PlanetPatchMap, this.Patches);
            }

            List<BioticumSlot> orphanSlots = [.. this.BioticumSlots.Values.Where(slot => slot.Patch is null && slot.locationOnPatch.value != 3)];
            List<NatureBioticum> orphanBio = [.. this.ActiveBiotica.Values.Where(bio => bio.Slot is null)];
        }
    }
}
