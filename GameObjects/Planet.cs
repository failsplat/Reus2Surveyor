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
        public List<JToken> Tokens;

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
        public Dictionary<int, CityObjects.Project> CityProjects = []; // Members in here are accessed from City

        // Collections by gameplay-relevant indices
        public List<City> CitiesInOrder { get => [.. this.Cities.Values.OrderBy(city => city.cityIndex)]; }

        public Planet(SaveRoot sr, string path)
        {
            int i = -1;
            this.Tokens = sr.referenceTokens;

            // Casting JO to game objects
            // Top-level objects (identified from _type or name)
            foreach (JToken jo in this.Tokens) 
            {
                i++;

                Dummy dummy = jo.ToObject<Dummy>();

                if (dummy is null) continue;
                
                switch (dummy.name)
                {
                    case (null):
                        break;
                    case { } when dummy.name.StartsWith("BioticumSlot"):
                        BioticumSlot slot = jo.ToObject<BioticumSlot>();
                        this.BioticumSlots.Add(i, slot);
                        continue;
                    case { } when dummy.name.StartsWith("City #"):
                        City city = jo.ToObject<City>();
                        this.Cities.Add(i, city);
                        continue;
                    case "BiomeModelData":
                        Biome biome = jo.ToObject<Biome>();
                        this.Biomes.Add(i, biome);
                        continue;
                    case "ProjectController":
                        CityControllers.ProjectController projectController = jo.ToObject<CityControllers.ProjectController>();
                        this.CityProjectControllers.Add(i, projectController);
                        continue;
                    case "PatchCollection":
                        PatchCollection patchCollection = jo.ToObject<PatchCollection>();
                        this.PlanetPatchMap = new(patchCollection.IdList);
                        continue;
                    default:
                        break;
                }

                switch (dummy._type)
                {
                    case (null):
                        break;
                    case "Patch":
                        Patch patch = jo.ToObject<Patch>();
                        this.Patches.Add(i, patch);
                        continue;
                    case "NatureBioticum":
                        NatureBioticum bio = jo.ToObject<NatureBioticum>();
                        this.AllBiotica.Add(i, bio);
                        continue;
                    default:
                        break;
                }
            }

            // Second-pass items (located by token index number)

            foreach (CityControllers.ProjectController proc in this.CityProjectControllers.Values)
            {
                Dictionary<int, CityObjects.Project> foundProjects = proc.FindProjects(this.Tokens);
                foreach ((int projectId, CityObjects.Project project) in foundProjects) 
                {
                    this.CityProjects[projectId] = project;
                }
            }

            // Filtering out irrelevant objects
            // CitySlots
            HashSet<int> citySlots = [.. this.BioticumSlots.Where(kv => kv.Value.locationOnPatch.value == 3).Select(kv => kv.Key)];
            foreach (int cs in citySlots) this.BioticumSlots.Remove(cs);
            // Biotica on future slots
            HashSet<int> futureSlots = [.. this.BioticumSlots.Values.Select(s => s.futureSlot.id)];
            this.ActiveBiotica = this.AllBiotica.Where(kv => !futureSlots.Contains((int)kv.Value.parent.id)).ToDictionary();

            // Cross-referencing and collation

            // Geography
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

            // City
            // There are accessed per-city, so linking only is useful in one direction
            foreach (CityControllers.ProjectController proc in this.CityProjectControllers.Values)
            {
                this.Cities[(int)proc.parent.id].AttachProjectController(proc);
            }

            List<BioticumSlot> orphanSlots = [.. this.BioticumSlots.Values.Where(slot => slot.Patch is null && slot.locationOnPatch.value != 3)];
            List<NatureBioticum> orphanBio = [.. this.ActiveBiotica.Values.Where(bio => bio.Slot is null)];
        }
    }
}
