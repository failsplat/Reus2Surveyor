using static System.IO.Path;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using static Reus2Surveyor.GameObjects.CityControllers;

namespace Reus2Surveyor.GameObjects
{
    // This is not deserialized directly
    // Instead, it is assembled by reading SaveRoot.referenceTokens 
    public class Planet
    {
        public string Name { get; init; }
        public long EpochMinutes { get; init; }
        public int Number { get; init; }
        public string Path { get; init; }
        public string DebugPath { get; init; }

        //public readonly List<JToken> Tokens; // Memory hog?

        public GameSession GameSession { get; init; }
        public GameplayController GameplayController { get; init; }
        // Indexed based on order in referenceTokens
        // Top-level objects (identified from _type or name)
        public Dictionary<int, BioticumSlot> BioticumSlots { get; init; } = [];
        public Dictionary<int, Patch> Patches { get; init; } = [];
        public Dictionary<int, Biome> Biomes { get; init; } = [];
        public Dictionary<int, NatureBioticum> AllBiotica { get; init; } = [];
        public Dictionary<int, NatureBioticum> ActiveBiotica { get; init; } // Filled out on cross-referencing

        public Dictionary<int, City> Cities { get; init; } = [];
        public Dictionary<int, ProjectController> CityProjectControllers { get; init; } = [];
        public Dictionary<int, ResourceController> CityResourceControllers { get; init; } = [];
        public Dictionary<int, LuxuryController> CityLuxuryControllers { get; init; } = [];
        public Dictionary<int, BorderController> CityBorderControllers { get; init; } = [];
        public Dictionary<int, CityObjects.LuxuryGood> LuxuryGoods { get; init; } = [];
        public Dictionary<int, GenericBuff> GenericBuffs { get; init; } = [];

        public PatchMap<int> PlanetPatchMap { get; init; }

        // "Hidden" objects - can't identified by _type/name, located by reference from other object
        // Alternately this could be done by try-catching the deserialization or other way of matching the schema
        public Dictionary<int, CityObjects.Project> CityProjects { get; init; } = []; // Members in here are accessed from City

        // Collections by gameplay-relevant indices
        public List<City> CitiesInOrder { get; init; }
        public Dictionary<string, int> ActiveBioticaDefCounter { get; init; } = [];
        public Dictionary<string, int> ActiveBioticaNameCounter { get; init; } = [];
        public Dictionary<string, int> LegacyBioticaDefCounter { get; init; } = [];
        public int TotalSize { get; init; }
        public int WildSize { get; init; }
        public Dictionary<int, (string biomeTypeName, double percentSize)> BiomeSizeMap { get; init; }

        public Planet(SaveRoot sr, string path, int number)
        {
            this.Name = PlanetFileUtil.PlanetNameFromSaveFilePath(path);
            this.EpochMinutes = PlanetFileUtil.EpochMinutesFromSaveFilePath(path);
            this.Number = number;
            this.Path = path;
            List<string> pathParts = [.. this.Path.Split(System.IO.Path.DirectorySeparatorChar)];
            pathParts.Reverse();
            this.DebugPath = pathParts[1] + System.IO.Path.DirectorySeparatorChar + pathParts[0];

            int i = -1;
            //this.Tokens = sr.referenceTokens;

            // Casting JO to game objects
            // Top-level objects (identified from _type or name)
            foreach (JToken jo in sr.referenceTokens) 
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
                        city.TokenIndex = i;
                        this.Cities.Add(i, city);
                        continue;
                    case "BiomeModelData":
                        Biome biome = jo.ToObject<Biome>();
                        this.Biomes.Add(i, biome);
                        continue;
                    case "ProjectController":
                        ProjectController projectController = jo.ToObject<ProjectController>();
                        this.CityProjectControllers.Add(i, projectController);
                        continue;
                    case "PatchCollection":
                        PatchCollection patchCollection = jo.ToObject<PatchCollection>();
                        this.PlanetPatchMap = new(patchCollection.IdList);
                        continue;
                    case "CityResourceController":
                        ResourceController resCon = jo.ToObject<ResourceController>();
                        this.CityResourceControllers.Add(i, resCon);
                        continue;
                    case "LuxuryController":
                        LuxuryController luxCon = jo.ToObject<LuxuryController>();
                        this.CityLuxuryControllers.Add(i, luxCon);
                        continue;
                    case "LuxuryGood":
                        CityObjects.LuxuryGood lg = jo.ToObject<CityObjects.LuxuryGood>();
                        this.LuxuryGoods.Add(i, lg);
                        continue;
                    case "CityBorderController":
                        BorderController borderController = jo.ToObject<BorderController>();
                        this.CityBorderControllers.Add(i, borderController);
                        continue;
                    case "Session":
                        this.GameSession = jo.ToObject<GameSession>();
                        continue;
                    case "GameplayController":
                        this.GameplayController = jo.ToObject<GameplayController>();
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
                    case "GenericBuff":
                        GenericBuff genBuff = jo.ToObject<GenericBuff>();
                        this.GenericBuffs.Add(i, genBuff);
                        continue;
                    default:
                        break;
                }
            }

            // Second-pass items (located by token index number)

            foreach (ProjectController proc in this.CityProjectControllers.Values)
            {
                Dictionary<int, CityObjects.Project> foundProjects = proc.FindProjects(sr.referenceTokens);
                foreach ((int projectId, CityObjects.Project project) in foundProjects) 
                {
                    this.CityProjects[projectId] = project;
                }
            }

            // Filtering out irrelevant objects
            // CitySlots
            HashSet<int> citySlots = [.. this.BioticumSlots.Where(kv => kv.Value.LocationOnPatch == 3).Select(kv => kv.Key)];
            foreach (int cs in citySlots) this.BioticumSlots.Remove(cs);
            // Biotica on future slots
            HashSet<int> futureSlots = [.. this.BioticumSlots.Values.Select(s => s.futureSlot.id)];
            this.ActiveBiotica = this.AllBiotica.Where(kv => !futureSlots.Contains((int)kv.Value.parent.id)).ToDictionary();


            // Count active biotica
            this.CitiesInOrder = [.. this.Cities.Values.OrderBy(city => city.cityIndex)];
            foreach (string? bioDef in this.ActiveBiotica.Values.Select(nb => nb.Definition))
            {
                if (bioDef is null) continue;
                string bioName = Glossaries.BioticumNameFromHash(bioDef);
                if (this.ActiveBioticaDefCounter.ContainsKey(bioDef)) 
                {
                    this.ActiveBioticaDefCounter[bioDef] += 1;
                    this.ActiveBioticaNameCounter[bioName] += 1;
                }
                else
                {
                    this.ActiveBioticaDefCounter[bioDef] = 1;
                    this.ActiveBioticaNameCounter[bioName] = 1;
                }
            }

            // Geography
            // Call method on parent object to put child objects in its properties
            // Parent object calls method on child object to put itself in its properties
            foreach (Patch patch in this.Patches.Values)
            {
                patch.AttachSlots(this.BioticumSlots);
            }
            foreach (BioticumSlot slot in this.BioticumSlots.Values)
            {
                slot.FindBiotica(this.AllBiotica);
                foreach (string archivedDef in slot.ArchivedBioticaDefs)
                {
                    if (this.LegacyBioticaDefCounter.ContainsKey(archivedDef))
                    {
                        this.LegacyBioticaDefCounter[archivedDef] += 1;
                    }
                    else
                    {
                        this.LegacyBioticaDefCounter[archivedDef] = 1;
                    }
                }
            }
            // Put patches in biome from PatchMap and dictionary of patches
            foreach (Biome b in this.Biomes.Values)
            {
                b.BuildPatchInfo(this.PlanetPatchMap, this.Patches);
            }
            // Order-dependant
            this.TotalSize = this.Patches.Count;
            this.WildSize = this.Patches.Values.Where(p => p.IsWild).Count();
            this.BiomeSizeMap = this.Biomes.Values.Where(b => b.AnchorPatch is not null && b.BiomeTypeDef is not null)
                .ToDictionary(b => (int)b.AnchorPatch, b => (Glossaries.BiomeNameFromHash(b.BiomeTypeDef), (double)b.TotalSize / (double)this.TotalSize));

            // City
            // There are accessed per-city, so linking only is useful in one direction

            int ci = 0;
            foreach (City city in this.CitiesInOrder)
            {
                // Makes sure that the controllers exist for each city
                city.AttachProjectController(this.CityProjectControllers[city.projectController.id]);
                city.AttachResourceController(this.CityResourceControllers[city.resourceController.id]);
                city.AttachLuxuryController(this.CityLuxuryControllers[city.luxuryController.id]);
                city.AttachBorderController(this.CityBorderControllers[city.borderController.id]);
                city.AttachLuxuryGoods(this.LuxuryGoods);
                city.AttachCivSummary(this.GameSession.CivSummaries[ci]);
                city.BuildTerritoryInformation(this.PlanetPatchMap, this.Patches, this.ActiveBiotica);
                ci++;
            }
            foreach (CityObjects.LuxuryGood lg in this.LuxuryGoods.Values)
            {
                if (lg.OriginCityId is not null)
                {
                    lg.AttachOriginCity(this.Cities[(int)lg.OriginCityId]);
                }
            }



            List<BioticumSlot> orphanSlots = [.. this.BioticumSlots.Values.Where(slot => slot.Patch is null && slot.LocationOnPatch != 3)];
            List<NatureBioticum> orphanBio = [.. this.ActiveBiotica.Values.Where(bio => bio.Slot is null)];
        }
    }
}
