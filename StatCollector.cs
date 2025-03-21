using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Features;
using DocumentFormat.OpenXml.Spreadsheet;
using static Reus2Surveyor.Glossaries;
using MathNet.Numerics.Statistics;

namespace Reus2Surveyor
{
    public class StatCollector
    {
        private Glossaries glossaryInstance;
        public Dictionary<string, BioticumStatEntry> BioticaStats { get; private set; } = [];
        public List<PlanetSummaryEntry> PlanetSummaries { get; private set; } = [];
        private int planetCount = 0;

        public StatCollector(Glossaries g)
        {
            this.glossaryInstance = g;
        }

        public void ConsumePlanet(Planet planet, int index)
        {
            if (planet is null) return;
            this.UpdateBioticaStats(planet, index);
            this.UpdateHumanityStats(planet, index);
            this.planetCount++;
        }

        public void UpdateBioticaStats(Planet planet, int index)
        {
            if (planet is null) return;

            HashSet<Glossaries.BioticumDefinition> biomeMatchingBiotica = [];
            // Count all biotica that are available in available biomes
            foreach (string giantHash in planet.gameSession.giantRosterDefs)
            {
                Glossaries.GiantDefinition gd = this.glossaryInstance.GiantDefinitionByHash[giantHash];

                foreach (Glossaries.BioticumDefinition bd in this.glossaryInstance.BioticumDefinitionList)
                {
                    bool b1match = (bool)(bd.GetType().GetProperty(gd.Biome1).GetValue(bd));
                    bool b2match = (bool)(bd.GetType().GetProperty(gd.Biome2).GetValue(bd));
                    if (b1match || b2match)
                    {
                        biomeMatchingBiotica.Add(bd);
                    }
                }
            }

            foreach (Glossaries.BioticumDefinition availDef in biomeMatchingBiotica)
            {
                if (!BioticaStats.ContainsKey(availDef.Hash))
                {
                    // This hash should never be null, but just in case and to keep things consistent:
                    if (glossaryInstance.BioticumDefFromHash(availDef.Hash) is null) BioticaStats[availDef.Hash] = new BioticumStatEntry(availDef.Hash, null);
                    else BioticaStats[availDef.Hash] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(availDef.Hash), null);

                }
                BioticaStats[availDef.Hash].Avail += 1;
            }

            Dictionary<string, int> activeBioCounter = [];
            Dictionary<string, int> legacyBioCounter = [];
            //Dictionary<(string,string), int> bioPropertyDict = [];

            foreach (KeyValuePair<string, int> legKv in planet.LegacyBioticaCounterDefs)
            {
                IncrementCounter(legacyBioCounter, legKv.Key, legKv.Value);
            }
            foreach (NatureBioticum nb in planet.natureBioticumDictionary.Values)
            {
                IncrementCounter(activeBioCounter, nb.definition, 1);
            }

            Dictionary<string, int> completeBioCounter = [];
            foreach (KeyValuePair<string, int> kv in activeBioCounter)
            {
                IncrementCounter(completeBioCounter, kv.Key, kv.Value);
            }
            foreach (KeyValuePair<string, int> kv in legacyBioCounter)
            {
                IncrementCounter(completeBioCounter, kv.Key, kv.Value);
            }

            // Make entries for drafted
            foreach (string draftDef in planet.MasteredBioticaDefSet)
            {
                if (!BioticaStats.ContainsKey(draftDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(draftDef) is null) BioticaStats[draftDef] = new BioticumStatEntry(draftDef, planet.name);
                    else BioticaStats[draftDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(draftDef), planet.name);

                }
                BioticaStats[draftDef].Draft += 1;
            }

            // Make entries for active then archived then complete
            foreach (string activeDef in activeBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(activeDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(activeDef) is null) BioticaStats[activeDef] = new BioticumStatEntry(activeDef, planet.name);
                    else BioticaStats[activeDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(activeDef), planet.name);

                }
                BioticaStats[activeDef].Final += activeBioCounter[activeDef];
            }
            foreach (string legacyDef in legacyBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(legacyDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(legacyDef) is null) BioticaStats[legacyDef] = new BioticumStatEntry(legacyDef, planet.name);
                    else BioticaStats[legacyDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(legacyDef), planet.name);

                }
                BioticaStats[legacyDef].Legacy += legacyBioCounter[legacyDef];
            }

            foreach (string cDef in completeBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(cDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(cDef) is null) BioticaStats[cDef] = new BioticumStatEntry(cDef, planet.name);
                    else BioticaStats[cDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(cDef), planet.name);

                }
                BioticaStats[cDef].Planets += 1;
                if (BioticaStats[cDef].P1 is null) BioticaStats[cDef].P1 = planet.name;
                BioticaStats[cDef].Total += completeBioCounter[cDef];
                if (completeBioCounter[cDef] > 1) BioticaStats[cDef].AddMultiValue(completeBioCounter[cDef]);
            }


        }

        public void UpdateHumanityStats(Planet planet, int index)
        {
            PlanetSummaryEntry planetEntry = new(index, planet.name);
            planetEntry.Score = (int)planet.gameSession.turningPointPerformances.Last().scoreTotal;

            List<GiantDefinition> giantDefs = [.. planet.gameSession.giantRosterDefs.Select(v => glossaryInstance.TryGiantDefinitionFromhash(v))];
            giantDefs.Sort((x, y) => x.Position - y.Position);
            List<string> giantNames = [.. giantDefs.Select(v => v.Name)];

            planetEntry.Giant1 = giantNames[0];
            planetEntry.Giant2 = giantNames[1];
            planetEntry.Giant3 = giantNames[2];

            planetEntry.Spirit = glossaryInstance.SpiritNameFromHash(planet.gameSession.selectedCharacterDef);

            List<int> cityProsList = [];
            List<int> cityPopList = [];
            List<int> cityTechList = [];
            List<int> cityWealList = [];

            planetEntry.Cities = planet.cityDictionary.Count;
            planetEntry.Prjs = 0;

            int cityIndex = 0; // Starts at 1, increments at beginning of loop
            foreach (City city in planet.cityDictionary.Values)
            {
                cityIndex += 1;

                planetEntry.Prjs += city.CityProjectController.projects.Count;


                cityProsList.Add((int)city.CivSummary.prosperity);
                cityPopList.Add((int)city.CivSummary.population);
                cityTechList.Add((int)city.CivSummary.innovation);
                cityWealList.Add((int)city.CivSummary.wealth);

                string founderName = glossaryInstance.SpiritNameFromHash(city.founderCharacterDef);
                typeof(PlanetSummaryEntry).GetField("Char" + cityIndex.ToString()).SetValue(planetEntry, founderName);
            }

            planetEntry.Pros = cityProsList.Sum();
            planetEntry.ProsMdn = Statistics.Median([..cityProsList]);
            planetEntry.ProsAv = Statistics.Mean([..cityProsList]);

            planetEntry.Pop = cityPopList.Sum();
            planetEntry.Tech = cityTechList.Sum();
            planetEntry.Weal = cityWealList.Sum();

            planetEntry.PopP = SafeDivide(planetEntry.Pop, planetEntry.Pros);
            planetEntry.TechP = SafeDivide(planetEntry.Tech, planetEntry.Pros);
            planetEntry.WealP = SafeDivide(planetEntry.Weal, planetEntry.Pros);

            planetEntry.PopHi = cityPopList.Max();
            planetEntry.TechHi = cityTechList.Max();
            planetEntry.WealHi = cityWealList.Max();

            planetEntry.PopMdn = Statistics.Median([.. cityPopList]);
            planetEntry.PopAv = Statistics.Mean([.. cityPopList]);
            planetEntry.TechMdn = Statistics.Median([.. cityTechList]);
            planetEntry.TechAv = Statistics.Mean([.. cityTechList]);
            planetEntry.WealMdn = Statistics.Median([.. cityWealList]);
            planetEntry.WealAv = Statistics.Mean([.. cityWealList]);

            if (planet.gameSession.turningPointPerformances.Count >= 1)
            {
                planetEntry.Era1Name = glossaryInstance.EraNameFromHash(planet.gameSession.turningPointPerformances[0].turningPointDef);
                planetEntry.Era1Score = planet.gameSession.turningPointPerformances[0].scoreTotal;
                planetEntry.Era1Star = planet.gameSession.turningPointPerformances[0].starRating;
            }

            if (planet.gameSession.turningPointPerformances.Count >= 2)
            {
                planetEntry.Era2Name = glossaryInstance.EraNameFromHash(planet.gameSession.turningPointPerformances[1].turningPointDef);
                planetEntry.Era2Score = planet.gameSession.turningPointPerformances[1].scoreTotal;
                planetEntry.Era2Star = planet.gameSession.turningPointPerformances[1].starRating;
            }

            if (planet.gameSession.turningPointPerformances.Count >= 3)
            {
                planetEntry.Era3Name = glossaryInstance.EraNameFromHash(planet.gameSession.turningPointPerformances[2].turningPointDef);
                planetEntry.Era3Score = planet.gameSession.turningPointPerformances[2].scoreTotal;
                planetEntry.Era3Star = planet.gameSession.turningPointPerformances[2].starRating;
            }

            planetEntry.SzT = planet.totalSize;
            planetEntry.SzWld = planet.wildSize;

            List<Biome> activeBiomes = [.. planet.biomeDictionary.Values.ToList().Where(b => b.anchorPatchId is not null)];
            planetEntry.Biomes = activeBiomes.Count;
            planetEntry.CBiomes = planet.gameSession.coolBiomes;

            List<string> bioticaHashList = [.. planet.natureBioticumDictionary.Values.ToList().Select(v => v.definition)];
            List<BioticumDefinition> bioticaDefList = [..bioticaHashList
                .Select(v => glossaryInstance.BioticumDefFromHash(v))
                .Where(v => v is not null)];

            HashSet<BioticumDefinition> uniqueBioticaDefs = bioticaDefList.ToHashSet();

            planetEntry.Biotica = bioticaHashList.Count;
            planetEntry.UqBiotica = uniqueBioticaDefs.Count;
            planetEntry.Plants = bioticaDefList.Where(v => v.Type == "Plant").Count();
            planetEntry.UqPlants = uniqueBioticaDefs.Where(v => v.Type == "Planet").Count();
            planetEntry.Animals = bioticaDefList.Where(v => v.Type == "Animal").Count();
            planetEntry.UqAnimals = uniqueBioticaDefs.Where(v => v.Type == "Animal").Count();
            planetEntry.Minerals = bioticaDefList.Where(v => v.Type == "Mineral").Count();
            planetEntry.UqMinerals = uniqueBioticaDefs.Where(v => v.Type == "Mineral").Count();

            planetEntry.Apex = bioticaDefList.Where(v => v.Apex).Count();
            foreach (BioticumSlot slot in planet.slotDictionary.Values)
            {
                if (slot.bioticumId is null) continue;
                if (planet.natureBioticumDictionary.ContainsKey((int)slot.bioticumId))
                {
                    planetEntry.FilledSlots += 1;
                    if (slot.slotLevel is not null) planetEntry.IncrementSlotTotalLevel((int)slot.slotLevel);
                }
            }

            this.PlanetSummaries.Add(planetEntry);
        }

        public void FinalizeStats()
        {
            foreach (BioticumStatEntry bse in this.BioticaStats.Values)
            {
                bse.CalculateStats(this.planetCount);
            }
            foreach (PlanetSummaryEntry pse in this.PlanetSummaries)
            {
                pse.CalculateStats();
            }
        }

        public static void IncrementCounter<T>(Dictionary<T, int> dict, T key, int value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] += value;
            }
            else
            {
                dict[key] = value;
            }
        }

        public void WriteToExcel(string dstPath)
        {
            using (XLWorkbook wb = new())
            {
                var planetSummWs = wb.AddWorksheet("Planets");
                planetSummWs.Cell("A1").InsertTable(this.PlanetSummaries, "Planets");

                var bioWs = wb.AddWorksheet("Biotica");
                bioWs.Cell("A1").InsertTable(this.BioticaStats.Values, "Biotica");

                wb.SaveAs(dstPath);
            }


        }

        public class BioticumStatEntry
        {
            private readonly Glossaries.BioticumDefinition Definition;
            public readonly string Name;
            public readonly string Type;
            public readonly int? Tier;
            public readonly string Apex;

            public readonly string Desert, Forest, IceAge, Ocean, Rainforest, Savanna, Taiga;

            public int Avail { get; set; } = 0;
            public double? AvailP { get; set; } = null;
            public int Draft { get; set; } = 0;
            public double? DraftP { get; set; } = null;
            public int Planets { get; set; } = 0;
            public double? UsageP { get; private set; } = null;

            public int Total { get; set; } = 0;
            public int Legacy { get; set; } = 0;
            public double? LegacyP { get; private set; } = null;
            public int Final { get; set; } = 0;
            public double? FinalP { get; private set; } = null;

            private List<int> MultiNumberList = [];
            public int? Multi { get; set; } = null;
            public double? MultiP { get; private set; } = null;
            public int? MultiMx { get; private set; } = null;
            public double? MultiAv { get; private set; } = null;

            /*public int Creek { get; set; } = 0;
            public double? CreekPercent { get; private set; } = null;
            public int Invasive { get; set; } = 0;
            public double? InvasivePercent { get; private set; } = null;
            public int Anomaly { get; set; } = 0;
            public double? AnomalyPercent { get; private set; } = null;
            public int Sanctuary { get; set; } = 0;
            public double? SanctuaryPercent { get; private set; } = null;
            public int Mountain { get; set; } = 0;
            public double? MountainPercent { get; private set; } = null;
            public int Micro { get; set; } = 0;
            public double? MicroPercent { get; private set; } = null;*/
            public string P1 { get; set; }
            public readonly string Hash;

            public BioticumStatEntry(Glossaries.BioticumDefinition bioDef, string p1name)
            {
                this.Definition = bioDef;
                this.Name = bioDef.Name;
                this.Type = bioDef.Type;
                this.Tier = bioDef.Tier;
                this.Apex = bioDef.Apex ? "☆" : null;

                this.Desert = bioDef.Desert ? "Y" : null;
                this.Forest = bioDef.Forest ? "Y" : null;
                this.IceAge = bioDef.IceAge ? "Y" : null;
                this.Ocean = bioDef.Ocean ? "Y" : null;
                this.Savanna = bioDef.Savanna ? "Y" : null;
                this.Taiga = bioDef.Taiga ? "Y" : null;

                this.Hash = bioDef.Hash;
                this.P1 = p1name;
            }

            public BioticumStatEntry(string hash, string p1name)
            {
                this.Definition = null;
                this.Name = "?";
                this.Type = "?";
                this.Tier = null;
                this.Apex = "?";
                this.Hash = hash;

                this.Desert = null;
                this.Forest = null;
                this.IceAge = null;
                this.Ocean = null;
                this.Savanna = null;
                this.Taiga = null;

                this.P1 = p1name;
            }

            public void IncrementField(string fieldName)
            {
                Type thisType = typeof(BioticumStatEntry);

                FieldInfo field = thisType.GetField(fieldName);
                if (field is not null)
                {
                    if (field.GetType() == typeof(int))
                    {
                        field.SetValue(this, (int)(field.GetValue(this)) + 1);
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("{0} is not an int field of {1}", fieldName, thisType.Name), thisType.Name);
                    }
                }
                else
                {
                    throw new ArgumentException(String.Format("{0} is not a valid field of {1}", fieldName, thisType.Name), thisType.Name);
                }
            }

            public void AddMultiValue(int value)
            {
                this.MultiNumberList.Add(value);
            }

            public void CalculateStats(int planetCount)
            {
                this.UsageP = SafePercent(this.Planets, this.Avail);
                this.LegacyP = SafePercent(this.Legacy, this.Total);
                this.FinalP = SafePercent(this.Final, this.Total);
                this.DraftP = SafePercent(this.Draft, this.Avail);
                this.AvailP = SafePercent(this.Avail, planetCount);

                if (this.MultiNumberList.Count > 0)
                {
                    this.Multi = this.MultiNumberList.Count;
                    this.MultiAv = this.MultiNumberList.Average();
                    this.MultiMx = this.MultiNumberList.Max();
                    this.MultiP = SafePercent((int)this.Multi, this.Planets);
                }
                else
                {
                    this.Multi = this.MultiNumberList.Count;
                }


                /*this.CreekPercent = SafeDivide(this.Creek, this.Total);
                this.InvasivePercent = SafeDivide(this.Invasive, this.Total);
                this.AnomalyPercent = SafeDivide(this.Anomaly, this.Total);
                this.SanctuaryPercent = SafeDivide(this.Sanctuary, this.Total);
                this.MountainPercent = SafeDivide(this.Mountain, this.Total);
                this.MicroPercent = SafeDivide(this.Micro, this.Total);*/
            }
        }
        private static double? SafePercent(int a0, int b0)
        {
            double? c = SafeDivide(a0, b0);
            if (c is null) return null;
            return Math.Max(Math.Min((double)c, 1.0), 0.0);
        }
        public static double? SafeDivide(int a0, int b0)
        {
            if (b0 == 0) return null;
            double a = (double)a0;
            double b = (double)b0;
            return a / b;
        }

        public class PlanetSummaryEntry
        {
            public readonly int N;
            public readonly string Name;

            public int Score;
            public int Pros;
            public string Giant1, Giant2, Giant3;
            public string Spirit;

            public int Cities, Prjs;

            public string Era1Name;
            public int? Era1Star, Era1Score;
            public string Era2Name;
            public int? Era2Star, Era2Score;
            public string Era3Name;
            public int? Era3Star, Era3Score;

            public string Char1, Char2, Char3, Char4, Char5, Char6;

            public int SzT, SzWld;
            public int FilledSlots = 0;

            public int ProsHi;
            public double? ProsMdn, ProsAv;
            public int Pop, Tech, Weal;
            public double? PopP, TechP, WealP;
            public int PopHi, TechHi, WealHi;
            public double? PopMdn, TechMdn, WealMdn, PopAv, TechAv, WealAv;

            public int? Biomes, CBiomes;
            public int Biotica, Plants, Animals, Minerals = 0;
            public int UqBiotica, UqPlants, UqAnimals, UqMinerals;
            public double? PlantP, AnimalP, MineralP;
            public int Apex;
            private int OccupiedSlotTotalLevel = 0;
            public double? ApexP, SlotLvAv; 

            public PlanetSummaryEntry(int N, string Name)
            {
                this.N = N;
                this.Name = Name;
            }

            public void IncrementSlotTotalLevel(int value)
            {
                this.OccupiedSlotTotalLevel += value;
            }

            public void CalculateStats()
            {
                this.SlotLvAv = SafeDivide(this.OccupiedSlotTotalLevel, this.FilledSlots);
                this.AnimalP = SafeDivide(this.Animals, this.Biotica);
                this.PlantP = SafeDivide(this.Plants, this.Biotica);
                this.MineralP = SafeDivide(this.Minerals, this.Biotica);
            }

        }
    }
}
