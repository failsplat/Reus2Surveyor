﻿using ClosedXML.Excel;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Reus2Surveyor.Glossaries;

namespace Reus2Surveyor
{
    public class StatCollector
    {
        private Glossaries glossaryInstance;
        public Dictionary<string, BioticumStatEntry> BioticaStats { get; private set; } = [];
        public List<PlanetSummaryEntry> PlanetSummaries { get; private set; } = [];
        public List<CitySummaryEntry> CitySummaries { get; private set; } = [];

        public Dictionary<string, Dictionary<string, int>> BioticumVsSpiritCounter { get; private set; } = [];
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
            this.CountBioticaVsSpirit(planet, index);
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
                    bool b1match = bd.BiomesAllowed[gd.Biome1];
                    bool b2match = bd.BiomesAllowed[gd.Biome2];
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
            // Planet Summary
            PlanetSummaryEntry planetEntry = new(index, planet.name);
            planetEntry.Score = (int)planet.gameSession.turningPointPerformances.Last().scoreTotal;

            planetEntry.Giant1 = planet.GiantNames[0];
            planetEntry.Giant2 = planet.GiantNames[1];
            planetEntry.Giant3 = planet.GiantNames[2];

            planetEntry.Spirit = glossaryInstance.SpiritNameFromHash(planet.gameSession.selectedCharacterDef);

            List<int> cityProsList = [];
            List<int> cityPopList = [];
            List<int> cityTechList = [];
            List<int> cityWelList = [];

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
                cityWelList.Add((int)city.CivSummary.wealth);

                string founderName = glossaryInstance.SpiritNameFromHash(city.founderCharacterDef);
                typeof(PlanetSummaryEntry).GetField("Char" + cityIndex.ToString()).SetValue(planetEntry, founderName);
            }

            planetEntry.Pros = cityProsList.Sum();
            planetEntry.ProsMdn = Statistics.Median([.. cityProsList]);
            planetEntry.ProsAv = Statistics.Mean([.. cityProsList]);
            planetEntry.Gini = GiniCoeff(cityProsList);
            planetEntry.ProsHi = cityProsList.Max();

            planetEntry.Pop = cityPopList.Sum();
            planetEntry.Tech = cityTechList.Sum();
            planetEntry.Wel = cityWelList.Sum();

            // // % of total Prosperity (including bonus prosperity from luxuries, requests, etc.)
            //planetEntry.PopP = SafeDivide(planetEntry.Pop, planetEntry.Pros);
            //planetEntry.TechP = SafeDivide(planetEntry.Tech, planetEntry.Pros);
            //planetEntry.WelP = SafeDivide(planetEntry.Wel, planetEntry.Pros);

            planetEntry.PopP = SafePercent(planetEntry.Pop, planetEntry.Pop + planetEntry.Tech + planetEntry.Wel);
            planetEntry.TechP = SafePercent(planetEntry.Tech, planetEntry.Pop + planetEntry.Tech + planetEntry.Wel);
            planetEntry.WelP = SafePercent(planetEntry.Wel, planetEntry.Pop + planetEntry.Tech + planetEntry.Wel);

            planetEntry.PopHi = cityPopList.Max();
            planetEntry.TechHi = cityTechList.Max();
            planetEntry.WelHi = cityWelList.Max();

            planetEntry.PopMdn = Statistics.Median([.. cityPopList]);
            planetEntry.PopAv = Statistics.Mean([.. cityPopList]);
            planetEntry.TechMdn = Statistics.Median([.. cityTechList]);
            planetEntry.TechAv = Statistics.Mean([.. cityTechList]);
            planetEntry.WelMdn = Statistics.Median([.. cityWelList]);
            planetEntry.WelAv = Statistics.Mean([.. cityWelList]);

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
            planetEntry.UqPlants = uniqueBioticaDefs.Where(v => v.Type == "Plant").Count();
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

            // City Summary

            List<CitySummaryEntry> thisPlanetCitySummaries = [];
            List<City> citiesInOrder = [.. planet.cityDictionary.ToList().OrderBy(kv => kv.Key).Select(kv => kv.Value)];
            int cityN = 0;
            foreach (City city in citiesInOrder)
            {
                cityN++;
                CitySummaryEntry cityEntry = new(index, cityN, city.fancyName);

                cityEntry.Char = glossaryInstance.SpiritNameFromHash(city.founderCharacterDef);
                cityEntry.Level = city.currentVisualStage is not null ? city.currentVisualStage + 1 : null;

                cityEntry.Pros = (int)city.CivSummary.prosperity;
                cityEntry.Pop = (int)city.CivSummary.population;
                cityEntry.Tech = (int)city.CivSummary.innovation;
                cityEntry.Wel = (int)city.CivSummary.wealth;

                cityEntry.FoundBiome = glossaryInstance.BiomeNameFromHash(city.settledBiomeDef);
                cityEntry.CurrBiome = glossaryInstance.BiomeNameFromHash(city.currentBiomeDef);

                cityEntry.PopP = SafePercent(cityEntry.Pop, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);
                cityEntry.TechP = SafePercent(cityEntry.Tech, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);
                cityEntry.WelP = SafePercent(cityEntry.Wel, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);

                cityEntry.ProsVMdn = cityEntry.Pros / planetEntry.ProsMdn;
                cityEntry.PopVMdn = cityEntry.Pop / planetEntry.PopMdn;
                cityEntry.TechVMdn = cityEntry.Tech / planetEntry.TechMdn;
                cityEntry.WelVMdn = cityEntry.Wel / planetEntry.WelMdn;

                cityEntry.Inventions = city.CityLuxuryController.luxurySlots.Where(ls => ls.luxuryGoodId is not null).Count();
                cityEntry.TradeRoutes = city.CityLuxuryController.importAgreementIds.Count();
                cityEntry.TerrPatches = city.PatchesInTerritory.Where(p => p.IsWildPatch()).Count();

                cityEntry.TPLead = city.initiatedTurningPointsDefs.Count();
                foreach (string cityStartedEras in city.initiatedTurningPointsDefs)
                {
                    EraDefinition thisEra = glossaryInstance.TryEraDefinitionFromHash(cityStartedEras);
                    if (thisEra.Era == 0) continue;
                    string eraName = thisEra.Name;
                    switch (thisEra.Era)
                    {
                        case 1:
                            cityEntry.TP1 = eraName;
                            break;
                        case 2:
                            cityEntry.TP2 = eraName;
                            break;
                        case 3:
                            cityEntry.TP3 = eraName;
                            break;
                        default:
                            break;
                    }
                }

                cityEntry.Biotica = city.BioticaInTerritory.Count;
                List<int> bioticaLevels = [];
                foreach (NatureBioticum nb in city.BioticaInTerritory)
                {
                    if (glossaryInstance.BioticumDefinitionByHash.ContainsKey(nb.definition))
                    {
                        BioticumDefinition thisBio = glossaryInstance.BioticumDefinitionByHash[nb.definition];
                        bioticaLevels.Add(thisBio.Tier);
                        switch (thisBio.Type)
                        {
                            case "Plant":
                                cityEntry.Plants += 1;
                                break;
                            case "Animal":
                                cityEntry.Animals += 1;
                                break;
                            case "Mineral":
                                cityEntry.Minerals += 1;
                                break;
                        }
                    }
                }
                cityEntry.AvBioLv = bioticaLevels.Average();
                cityEntry.PlantP = SafePercent(cityEntry.Plants, cityEntry.Biotica);
                cityEntry.AnimalP = SafePercent(cityEntry.Animals, cityEntry.Biotica);
                cityEntry.MineralP = SafePercent(cityEntry.Minerals, cityEntry.Biotica);

                foreach (City.ProjectController.CityProject project in city.CityProjectController.projects)
                {
                    cityEntry.Buildings += 1;
                    if (glossaryInstance.ProjectDefinitionByHash.ContainsKey(project.definition))
                    {
                        CityProjectDefinition projectDef = glossaryInstance.TrProjectDefinitionFromHash(project.definition);
                        switch (projectDef.Slot)
                        {
                            case "Era1":
                                cityEntry.Era1B = projectDef.DisplayName;
                                break;
                            case "Era2":
                                cityEntry.Era2B = projectDef.DisplayName;
                                break;
                            case "Era3":
                                cityEntry.Era3B = projectDef.DisplayName;
                                break;
                            case "Lv1":
                                cityEntry.Lv1B = projectDef.DisplayName;
                                break;
                            case "Lv2":
                                cityEntry.Lv2B = projectDef.DisplayName;
                                break;
                            case "Lv3":
                                cityEntry.Lv3B = projectDef.DisplayName;
                                break;
                            case "Temple1":
                                cityEntry.Temple1 = projectDef.DisplayName;
                                break;
                            case "Temple2":
                                cityEntry.Temple2 = projectDef.DisplayName;
                                break;
                            case "Temple3":
                                cityEntry.Temple3 = projectDef.DisplayName;
                                break;
                        }
                    }
                }

                thisPlanetCitySummaries.Add(cityEntry);
            }
            thisPlanetCitySummaries.OrderBy(x => x.CityN);

            List<int> cityRanks = [..thisPlanetCitySummaries
                .Select(cs => cs.Pros)
                .Select((x, i) => new KeyValuePair<int, int>(x, i))
                .OrderBy(xi => -xi.Key)
                .Select(xi => xi.Value)];

            for (int i = 0; i < cityRanks.Count; i++)
            {
                int c = cityRanks[i];
                thisPlanetCitySummaries[c].Rank = i + 1;
                thisPlanetCitySummaries[c].Upset = thisPlanetCitySummaries[c].CityN - thisPlanetCitySummaries[c].Rank;
            }

            this.CitySummaries.AddRange(thisPlanetCitySummaries);
        }

        public void CountBioticaVsSpirit(Planet planet, int index)
        {
            foreach (City city in planet.cityDictionary.Values)
            {
                string spirit = glossaryInstance.SpiritNameFromHash(city.founderCharacterDef);
                foreach (NatureBioticum nb in city.BioticaInTerritory)
                {
                    if (glossaryInstance.BioticumDefinitionByHash.ContainsKey(nb.definition))
                    {
                        string activeBioName = glossaryInstance.BioticumNameFromHash(nb.definition);
                        if (!BioticumVsSpiritCounter.ContainsKey(activeBioName)) this.BioticumVsSpiritCounter[activeBioName] = new();
                        if (!BioticumVsSpiritCounter[activeBioName].ContainsKey(spirit)) this.BioticumVsSpiritCounter[activeBioName][spirit] = 0;
                        this.BioticumVsSpiritCounter[activeBioName][spirit] += 1;
                    }
                }
                foreach (int slotIndex in city.ListSlotIndicesInTerritory())
                {
                    BioticumSlot slot = planet.slotDictionary[slotIndex];
                    foreach (string legacyDef in slot.archivedBioticaDefs)
                    {
                        if (glossaryInstance.BioticumDefinitionByHash.ContainsKey(legacyDef))
                        {
                            string legacyBioName = glossaryInstance.BioticumNameFromHash(legacyDef);
                            if (!BioticumVsSpiritCounter.ContainsKey(legacyBioName)) this.BioticumVsSpiritCounter[legacyBioName] = new();
                            if (!BioticumVsSpiritCounter[legacyBioName].ContainsKey(spirit)) this.BioticumVsSpiritCounter[legacyBioName][spirit] = 0;
                            this.BioticumVsSpiritCounter[legacyBioName][spirit] += 1;
                        }
                    }
                }
            }
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

        public static DataTable NestDictToDataTable<T, T2>(Dictionary<T, Dictionary<T, T2>> input, string indexName)
        {
            // Similar to pandas.DataFrame.from_dict with the "row" orientation
            DataTable output = new();
            output.Columns.Add(indexName);

            List<string> columnHeaders = [.. input.SelectMany(kv => kv.Value.Select(kv => kv.Key.ToString())).Distinct()];
            columnHeaders.Sort();
            foreach (string colHead in columnHeaders)
            {
                output.Columns.Add(colHead);
                output.Columns[colHead].DataType = typeof(T2);
            }

            foreach (KeyValuePair<T, Dictionary<T, T2>> rowKV in input)
            {
                DataRow newRow = output.NewRow();
                newRow[indexName] = rowKV.Key.ToString();
                foreach (KeyValuePair<T, T2> dataKV in rowKV.Value)
                {
                    newRow[dataKV.Key.ToString()] = dataKV.Value;
                }
                output.Rows.Add(newRow);
            }

            return output;
        }

        public void WriteToExcel(string dstPath)
        {
            using (XLWorkbook wb = new())
            {
                var planetSummWs = wb.AddWorksheet("Planets");
                var planetTable = planetSummWs.Cell("A1").InsertTable(this.PlanetSummaries, "Planets");
                planetTable.Theme = XLTableTheme.TableStyleMedium4;
                foreach (KeyValuePair<string, List<string>> kv in PlanetSummaryEntry.GetColumnFormats())
                {
                    string format = kv.Key;
                    List<string> columns = kv.Value;

                    foreach (string colName in columns)
                    {
                        try
                        {
                            var col = planetTable.FindColumn(c => c.FirstCell().Value.ToString() == colName);
                            col.Style.NumberFormat.Format = format;
                        }
                        catch { }
                    }
                }

                var cityWs = wb.AddWorksheet("Cities");
                var cityTable = cityWs.Cell("A1").InsertTable(this.CitySummaries, "Cities");
                cityTable.Theme = XLTableTheme.TableStyleLight1;
                foreach (KeyValuePair<string, List<string>> kv in CitySummaryEntry.GetColumnFormats())
                {
                    string format = kv.Key;
                    List<string> columns = kv.Value;

                    foreach (string colName in columns)
                    {
                        try
                        {
                            var col = cityTable.FindColumn(c => c.FirstCell().Value.ToString() == colName);
                            col.Style.NumberFormat.Format = format;
                        }
                        catch { }
                    }
                }

                var bioWs = wb.AddWorksheet("Biotica");
                var bioticaTable = bioWs.Cell("A1").InsertTable(this.BioticaStats.Values, "Biotica");
                bioticaTable.Theme = XLTableTheme.TableStyleMedium3;
                foreach (KeyValuePair<string, List<string>> kv in BioticumStatEntry.GetColumnFormats())
                {
                    string format = kv.Key;
                    List<string> columns = kv.Value;

                    foreach (string colName in columns)
                    {
                        try
                        {
                            var col = bioticaTable.FindColumn(c => c.FirstCell().Value.ToString() == colName);
                            col.Style.NumberFormat.Format = format;
                        }
                        catch { }
                    }
                }

                DataTable bioticaVsSpiritTable = NestDictToDataTable(this.BioticumVsSpiritCounter, "Bioticum");
                var bioVsCharWs = wb.AddWorksheet("BioticaVsChar");
                var bioVsCharTable = bioVsCharWs.Cell("A1").InsertTable(bioticaVsSpiritTable);

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

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "AvailP", "DraftP", "UsageP", "LegacyP", "FinalP", "MultiP", } },
                {"0.00", new List<string> { "MultiAv", } },
                };

            public BioticumStatEntry(Glossaries.BioticumDefinition bioDef, string p1name)
            {
                this.Definition = bioDef;
                this.Name = bioDef.Name;
                this.Type = bioDef.Type;
                this.Tier = bioDef.Tier;
                this.Apex = bioDef.Apex ? "☆" : null;

                this.Desert = bioDef.BiomesAllowed["Desert"] ? "Y" : null;
                this.Forest = bioDef.BiomesAllowed["Forest"] ? "Y" : null;
                this.IceAge = bioDef.BiomesAllowed["IceAge"] ? "Y" : null;
                this.Ocean = bioDef.BiomesAllowed["Ocean"] ? "Y" : null;
                this.Rainforest = bioDef.BiomesAllowed["Rainforest"] ? "Y" : null;
                this.Savanna = bioDef.BiomesAllowed["Savanna"] ? "Y" : null;
                this.Taiga = bioDef.BiomesAllowed["Taiga"] ? "Y" : null;

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
                this.Rainforest = null;
                this.Savanna = null;
                this.Taiga = null;

                this.P1 = p1name;
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

            public static Dictionary<string, List<string>> GetColumnFormats()
            {
                return columnFormats;
            }

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
            public double? ProsMdn, ProsAv, Gini;
            public int Pop, Tech, Wel;
            public double? PopP, TechP, WelP;
            public int PopHi, TechHi, WelHi;
            public double? PopMdn, TechMdn, WelMdn, PopAv, TechAv, WelAv;

            public int? Biomes, CBiomes;
            public int Biotica, Plants, Animals, Minerals = 0;
            public int UqBiotica, UqPlants, UqAnimals, UqMinerals;
            public double? PlantP, AnimalP, MineralP;
            public int Apex;
            private int OccupiedSlotTotalLevel = 0;
            public double? ApexP, SlotLvAv;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "PopP", "TechP", "WelP", "PlantP", "AnimalP", "MineralP", "ApexP" } },
                {"0.00", new List<string> { "ProsAv", "Gini", "PopAv", "TechAv", "WelAv", "SlotLvAv" } },
                };

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
                this.AnimalP = SafePercent(this.Animals, this.Biotica);
                this.PlantP = SafePercent(this.Plants, this.Biotica);
                this.MineralP = SafePercent(this.Minerals, this.Biotica);
                this.ApexP = SafePercent(this.Apex, this.Biotica);
            }

            public static Dictionary<string, List<string>> GetColumnFormats()
            {
                return columnFormats;
            }

        }

        public class CitySummaryEntry
        {
            public readonly int PlanetN, CityN;
            public readonly string Name;
            public string Char;
            public int? Level;
            public int Pros, Pop, Tech, Wel;
            public string FoundBiome, CurrBiome;
            public int? Rank, Upset = null;
            public double? PopP, TechP, WelP = null;
            public double? ProsVMdn, PopVMdn, TechVMdn, WelVMdn = null;

            public int Inventions, TradeRoutes = 0;
            public int TerrPatches = 0;

            public int TPLead = 0;
            public string TP1, TP2, TP3 = null;

            public int Biotica = 0;
            public double? AvBioLv = null;
            public int Plants, Animals, Minerals = 0;
            public double? PlantP, AnimalP, MineralP = null;

            public int Buildings = 0;
            public string Lv1B, Lv2B, Lv3B, Era1B, Era2B, Era3B, Temple1, Temple2, Temple3 = null;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "PopP", "TechP", "WelP", "PlantP", "AnimalP", "MineralP"} },
                {"0.00", new List<string> { "ProsVMdn", "PopVMdn", "TechVMdn", "WelVMdn", "AvBioLv" } },
                };

            public static Dictionary<string, List<string>> GetColumnFormats() { return columnFormats; }

            public CitySummaryEntry(int planetN, int cityN, string name)
            {
                this.PlanetN = planetN;
                this.CityN = cityN;
                this.Name = name;
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
        public static void FormatColumn(IXLTable table, string columnName, string numFormat = "0.00%")
        {
            var column = table.FindColumn(c => c.FirstCell().Value.ToString() == columnName);
            column.Style.NumberFormat.Format = numFormat;
        }

        public static double? GiniCoeff(List<double> values)
        {
            int n = values.Count;
            if (n == 0) return null;

            List<double> v2 = [.. values.Select(v => v / values.Sum())];
            v2.Sort();

            double a = 0;
            for (int i = 0; i < v2.Count; i++)
            {
                double vi = v2[i];
                a += i * vi;
            }

            double g = a;
            g *= 2;
            g /= v2.Sum();
            g /= n;
            g -= (n + 1) / n;
            return g;
        }
        public static double? GiniCoeff(List<int> values)
        {
            int n = values.Count;
            if (n == 0) return null;

            List<double> v2 = [.. values.Select(v => ((double)v) / values.Sum())];
            v2.Sort();

            double a = 0;
            for (int i = 0; i < v2.Count; i++)
            {
                double vi = v2[i];
                a += (i + 1) * vi;
            }

            double g = a;
            g *= 2;
            g /= v2.Sum();
            g /= n;
            g -= (n + 1) / n;
            return g;
        }

    }
}
