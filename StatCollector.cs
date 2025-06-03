using ClosedXML;
using ClosedXML.Attributes;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using MathNet.Numerics.Statistics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using static Reus2Surveyor.Glossaries;
using static Reus2Surveyor.StatCollector;

namespace Reus2Surveyor
{
    public partial class StatCollector
    {
        private Glossaries glossaryInstance;
        public OrderedDictionary<string, BioticumStatEntry> BioticaStats { get; private set; } = [];
        public List<PlanetSummaryEntry> PlanetSummaries { get; private set; } = [];
        public List<CitySummaryEntry> CitySummaries { get; private set; } = [];
        public OrderedDictionary<string, SpiritStatEntry> SpiritStats { get; private set; } = [];

        public OrderedDictionary<string, Dictionary<string, int>> BioticumVsSpiritCounter { get; private set; } = [];
        public OrderedDictionary<string, Dictionary<string, double>> BioticumVsSpiritRatios { get; private set; } = [];
        // First key is bioticum
        // Second key is spirit or character

        private int planetCount = 0;
        private HashSet<string> draftedOrPlacedInProfile { get; set; } = [];

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

            HashSet<string> draftedOrPlacedInSession = [];
            // Count if drafted
            foreach (string draftDef in planet.MasteredBioticaDefSet)
            {
                draftedOrPlacedInProfile.Add(draftDef);
                draftedOrPlacedInSession.Add(draftDef);
            }

            // Make entries for active then archived then complete
            foreach (string activeDef in activeBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(activeDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(activeDef) is null) BioticaStats[activeDef] = new BioticumStatEntry(activeDef, planet.number);
                    else BioticaStats[activeDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(activeDef), planet.number);

                }
                BioticaStats[activeDef].Final += activeBioCounter[activeDef];
                draftedOrPlacedInProfile.Add(activeDef);
                draftedOrPlacedInSession.Add(activeDef);
            }
            foreach (string legacyDef in legacyBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(legacyDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(legacyDef) is null) BioticaStats[legacyDef] = new BioticumStatEntry(legacyDef, planet.number);
                    else BioticaStats[legacyDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(legacyDef), planet.number);

                }
                BioticaStats[legacyDef].Legacy += legacyBioCounter[legacyDef];
                draftedOrPlacedInProfile.Add(legacyDef);
                draftedOrPlacedInSession.Add(legacyDef);
            }

            // Count all biotica that are available in available biomes
            // Only increment if it has been drafted or placed in this planet or previous planets
            // (Could be not unavailable by level or DLC)
            HashSet<string> biomeMatchingBiotica = [];
            foreach (string giantHash in planet.gameSession.giantRosterDefs)
            {
                Glossaries.GiantDefinition gd = this.glossaryInstance.GiantDefinitionByHash[giantHash];
                foreach (Glossaries.BioticumDefinition bd in this.glossaryInstance.BioticumDefinitionList)
                {
                    bool b1match = bd.BiomesAllowed[gd.Biome1];
                    bool b2match = bd.BiomesAllowed[gd.Biome2];
                    if (b1match || b2match)
                    {
                        biomeMatchingBiotica.Add(bd.Hash);
                        if (bd.Starter)
                        {
                            draftedOrPlacedInProfile.Add(bd.Hash);
                            draftedOrPlacedInSession.Add(bd.Hash);
                        }
                    }
                }
            }

            foreach (string draftDef in draftedOrPlacedInSession)
            {
                if (!BioticaStats.ContainsKey(draftDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(draftDef) is null) BioticaStats[draftDef] = new BioticumStatEntry(draftDef, planet.number);
                    else BioticaStats[draftDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(draftDef), planet.number);

                }
                BioticaStats[draftDef].Draft += 1;
            }

            foreach (string cDef in completeBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(cDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(cDef) is null) BioticaStats[cDef] = new BioticumStatEntry(cDef, planet.number);
                    else BioticaStats[cDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(cDef), planet.number);

                }
                BioticaStats[cDef].Planets += 1;
                BioticaStats[cDef].Total += completeBioCounter[cDef];
                BioticaStats[cDef].PLast = planet.number;
                if (completeBioCounter[cDef] > 1) BioticaStats[cDef].AddMultiValue(completeBioCounter[cDef]);
            }

            HashSet<string> missedDraft = [.. draftedOrPlacedInSession.Except(biomeMatchingBiotica)];
            HashSet<string> availBiotica = [..biomeMatchingBiotica.Intersect(draftedOrPlacedInProfile)];
            foreach (string availDef in availBiotica)
            {
                if (draftedOrPlacedInProfile.Contains(availDef))
                {
                    BioticaStats[availDef].Avail += 1;
                }
            }
        }

        public void UpdateHumanityStats(Planet planet, int index)
        {
            // Planet Summary
            PlanetSummaryEntry planetEntry = new(planet);
            if (planet.gameSession.turningPointPerformances.Count > 0)
            {
                planetEntry.Score = (int)planet.gameSession.turningPointPerformances.Last().scoreTotal;
            }

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
            planetEntry.AvPros = Statistics.Mean([.. cityProsList]);
            planetEntry.Gini = GiniCoeff(cityProsList);
            planetEntry.HiPros = cityProsList.Count > 0 ? cityProsList.Max() : 0;

            planetEntry.Pop = cityPopList.Sum();
            planetEntry.Tech = cityTechList.Sum();
            planetEntry.Wel = cityWelList.Sum();

            // // % of total Prosperity (including bonus prosperity from luxuries, requests, etc.)
            //planetEntry.PPop = SafeDivide(planetEntry.Pop, planetEntry.Pros);
            //planetEntry.PTech = SafeDivide(planetEntry.Tech, planetEntry.Pros);
            //planetEntry.PWel = SafeDivide(planetEntry.Wel, planetEntry.Pros);

            planetEntry.PPop = SafePercent(planetEntry.Pop, planetEntry.Pop + planetEntry.Tech + planetEntry.Wel);
            planetEntry.PTech = SafePercent(planetEntry.Tech, planetEntry.Pop + planetEntry.Tech + planetEntry.Wel);
            planetEntry.PWel = SafePercent(planetEntry.Wel, planetEntry.Pop + planetEntry.Tech + planetEntry.Wel);

            planetEntry.HiPop = cityPopList.Count > 0 ? cityPopList.Max() : 0;
            planetEntry.HiTech = cityTechList.Count > 0 ? cityTechList.Max() : 0;
            planetEntry.HiWel = cityWelList.Count > 0 ? cityWelList.Max() : 0;

            planetEntry.MdnPop = Statistics.Median([.. cityPopList]);
            planetEntry.AvPop = Statistics.Mean([.. cityPopList]);
            planetEntry.MdnTech = Statistics.Median([.. cityTechList]);
            planetEntry.AvTech = Statistics.Mean([.. cityTechList]);
            planetEntry.MdnWel = Statistics.Median([.. cityWelList]);
            planetEntry.AvWel = Statistics.Mean([.. cityWelList]);

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
                if (slot.isInvasiveSlot ?? false) planetEntry.InvasiveSpots += 1; 
                if (planet.natureBioticumDictionary.ContainsKey((int)slot.bioticumId))
                {
                    planetEntry.FilledSlots += 1;
                    if (slot.slotLevel is not null) planetEntry.IncrementSlotTotalLevel((int)slot.slotLevel);
                }
            }

            Dictionary<int, Patch> wildPatches = planet.patchDictionary.Where(kv => kv.Value.IsWildPatch()).ToDictionary();
            int wildSlots = wildPatches.SelectMany(kv => kv.Value.GetActiveSlotIndices()).Count();
            planetEntry.FillP = SafePercent(planetEntry.FilledSlots, wildSlots);

            planetEntry.Creeks = wildPatches.Where(kv => kv.Value.specialNaturalFeature == (int)Glossaries.SpecialNaturalFeatures.Creek).Count();
            planetEntry.Anomalies = wildPatches.Where(kv => kv.Value.specialNaturalFeature == (int)Glossaries.SpecialNaturalFeatures.Anomaly).Count();
            planetEntry.Sanctuaries = wildPatches.Where(kv => kv.Value.specialNaturalFeature == (int)Glossaries.SpecialNaturalFeatures.Sanctuary).Count();
            planetEntry.MountainSlots = wildPatches.Where(kv => kv.Value.mountainPart > 0).Count();

            foreach ((string biomeName, double percent) in planet.BiomePercentages)
            {
                planetEntry.biomePercents[biomeName] = percent;
            }

            this.PlanetSummaries.Add(planetEntry);

            // City Summary and Spirit Stats

            List<CitySummaryEntry> thisPlanetCitySummaries = [];
            List<City> citiesInOrder = [.. planet.cityDictionary.ToList().OrderBy(kv => kv.Key).Select(kv => kv.Value)];
            int cityN = 0;
            foreach (City city in citiesInOrder)
            {
                cityN++;
                CitySummaryEntry cityEntry = new(index, cityN, city.fancyName);

                string founderName = glossaryInstance.SpiritNameFromHash(city.founderCharacterDef);

                cityEntry.Char = founderName;
                cityEntry.Level = city.currentVisualStage is not null ? city.currentVisualStage + 1 : null;

                cityEntry.Pros = (int)city.CivSummary.prosperity;
                cityEntry.Pop = (int)city.CivSummary.population;
                cityEntry.Tech = (int)city.CivSummary.innovation;
                cityEntry.Wel = (int)city.CivSummary.wealth;

                cityEntry.FoundBiome = glossaryInstance.BiomeNameFromHash(city.settledBiomeDef);
                cityEntry.CurrBiome = glossaryInstance.BiomeNameFromHash(city.currentBiomeDef);

                cityEntry.PPop = SafePercent(cityEntry.Pop, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);
                cityEntry.PTech = SafePercent(cityEntry.Tech, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);
                cityEntry.PWel = SafePercent(cityEntry.Wel, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);

                cityEntry.RelPros = cityEntry.Pros / planetEntry.ProsMdn;
                cityEntry.RelPop = cityEntry.Pop / planetEntry.MdnPop;
                cityEntry.RelTech = cityEntry.Tech / planetEntry.MdnTech;
                cityEntry.RelWel = cityEntry.Wel / planetEntry.MdnWel;

                cityEntry.Invent = city.CityLuxuryController.luxurySlots.Where(ls => ls.luxuryGoodId is not null).Count();
                cityEntry.Trades = city.CityLuxuryController.importAgreementIds.Count();
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

                int slotCount = 0;
                foreach (Patch wildPatch in city.PatchesInTerritory.Where(p => p.IsWildPatch()))
                {
                    foreach(int slotIndex in wildPatch.GetActiveSlotIndices())
                    {
                        BioticumSlot slot = planet.slotDictionary[slotIndex];
                        slotCount += 1;
                        if (slot.bioticumId is not null) cityEntry.FilledSlots += 1;
                    }
                }
                cityEntry.FillP = SafePercent(cityEntry.FilledSlots, slotCount);

                cityEntry.Biotica = city.BioticaInTerritory.Count;
                List<int> bioticaLevels = [];
                // Active biotica only!
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
                        if (thisBio.Apex) cityEntry.Apex += 1;
                    }
                }

                foreach (Patch patch in city.PatchesInTerritory)
                {
                    foreach (int slotIndex in patch.GetActiveSlotIndices())
                    {
                        BioticumSlot slot = planet.slotDictionary[slotIndex];
                        foreach (string abd in slot.archivedBioticaDefs)
                        {
                            BioticumDefinition thisLegBio = glossaryInstance.BioticumDefFromHash(abd);
                            if (thisLegBio is null) continue;
                            switch (thisLegBio.Type)
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
                            if (thisLegBio.Apex) cityEntry.Apex += 1;
                            cityEntry.Biotica += 1;
                        }
                    }
                }

                cityEntry.AvFBioLv = bioticaLevels.Count > 0 ? bioticaLevels.Average() : 0;
                cityEntry.PPlant = SafePercent(cityEntry.Plants, cityEntry.Biotica);
                cityEntry.PAnimal = SafePercent(cityEntry.Animals, cityEntry.Biotica);
                cityEntry.PMineral = SafePercent(cityEntry.Minerals, cityEntry.Biotica);
                cityEntry.ApexP = SafePercent(cityEntry.Apex, cityEntry.Biotica);

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

            foreach (CitySummaryEntry ce in thisPlanetCitySummaries)
            {
                string founderName = ce.Char;
                if (!this.SpiritStats.ContainsKey(founderName)) 
                { 
                    this.SpiritStats[founderName] = new(founderName);
                    this.SpiritStats[founderName].InitializeBiomeCounters(this.glossaryInstance);
                }

                SpiritStatEntry se = this.SpiritStats[founderName];
                se.Count += 1;
                if (ce.CityN == 1)
                {
                    se.Prime += 1;
                    if (planet.gameSession.turningPointPerformances.Count > 0)
                    {
                        se.IncrementPlanetScoreTotalAsPrimary((int)planet.gameSession.turningPointPerformances.Last().scoreTotal);
                        se.PrScoreHi = Math.Max(se.PrScoreHi, (int)planet.gameSession.turningPointPerformances.Last().scoreTotal);
                    }
                }
                if (planet.gameSession.turningPointPerformances.Count > 0) se.IncrementPlanetScoreTotal((int)planet.gameSession.turningPointPerformances.Last().scoreTotal);

                se.IncrementProsperityTotals(ce.Pros, ce.Pop, ce.Tech, ce.Wel);
                se.IncrementProsperityPercentTotals((double)ce.PPop, (double)ce.PTech, (double)ce.PWel);
                se.IncrementProsperityRelTotals((double)ce.RelPros, (double)ce.RelPop, (double)ce.RelTech, (double)ce.RelWel);

                se.HiPros = Math.Max(se.HiPros, ce.Pros);
                se.HiPop = Math.Max(se.HiPop, ce.Pop);
                se.HiTech = Math.Max(se.HiTech, ce.Tech);
                se.HiWel = Math.Max(se.HiWel, ce.Wel);

                se.HiPPop = Math.Max(se.HiPPop, (double)ce.PPop);
                se.HiPTech = Math.Max(se.HiPTech, (double)ce.PTech);
                se.HiPWel = Math.Max(se.HiPWel, (double)ce.PWel);

                se.HiRelPros = Math.Max(se.HiRelPros, (double)ce.RelPros);
                se.HiRelPop = Math.Max(se.HiRelPop, (double)ce.RelPop);
                se.HiRelTech = Math.Max(se.HiRelTech, (double)ce.RelTech);
                se.HiRelWel = Math.Max(se.HiRelWel, (double)ce.RelWel);

                se.Invent += ce.Invent;
                se.Trades += ce.Trades;

                int upset = (int)ce.Upset;
                se.IncrementUpsetTotal(upset);
                if (upset > 0) se.PosUpset += 1;
                if (upset < 0) se.NegUpset += 1;

                se.Plants += ce.Plants;
                se.Animals += ce.Animals;
                se.Minerals += ce.Minerals;

                //se.IncrementBioticaPercentTotals((double)ce.PPlant, (double)ce.PAnimal, (double)ce.PMineral, (double)ce.ApexP);
                //se.IncrementBioticaPercentTotals((double)ce.ApexP);
                se.Apex += ce.Apex;
            }

            foreach (City city in planet.cityDictionary.Values)
            {
                Dictionary<string, int> biomePatchesInCity = [];
                string founderName = glossaryInstance.SpiritNameFromHash(city.founderCharacterDef);
                foreach (Patch patch in city.PatchesInTerritory)
                {
                    if (!patch.IsWildPatch()) continue;
                    string patchBiome = glossaryInstance.BiomeNameFromHash(patch.biomeDefinition);
                    if (!biomePatchesInCity.ContainsKey(patchBiome)) biomePatchesInCity[patchBiome] = 0;
                    biomePatchesInCity[patchBiome] += 1;
                }
                this.SpiritStats[founderName].IncrementBiomeUsage(biomePatchesInCity);

                List<int> bioticaLevels = [];
                // Active biotica only!
                foreach (NatureBioticum nb in city.BioticaInTerritory)
                {
                    if (glossaryInstance.BioticumDefinitionByHash.ContainsKey(nb.definition))
                    {
                        BioticumDefinition thisBio = glossaryInstance.BioticumDefinitionByHash[nb.definition];
                        bioticaLevels.Add(thisBio.Tier);
                    }
                }
                this.SpiritStats[founderName].IncrementBioticaLevelTotal(bioticaLevels);
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
            foreach(SpiritStatEntry sse in this.SpiritStats.Values)
            {
                sse.CalculateStats(this.PlanetSummaries.Count);
            }
            this.BioticumVsSpiritRatios = NestedCounterToNestedRatioDictionary(this.BioticumVsSpiritCounter);
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

        public static DataTable NestDictToDataTable<T, T2>(IDictionary<T, Dictionary<T, T2>> input, string indexName)
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
        public static OrderedDictionary<TKey, Dictionary<TKey, double>> NestedCounterToNestedRatioDictionary<TKey>(IDictionary<TKey, Dictionary<TKey, int>> input)
        {
            OrderedDictionary<TKey, Dictionary<TKey, double>> output = [];

            List<KeyValuePair<TKey, int>> flattened = [..input.SelectMany(kv1 => kv1.Value.ToList())];
            int total = flattened.Select(kv => kv.Value).Sum();
            Dictionary<TKey, int> columnTotals = [];
            List<TKey> columns = [..flattened.Select(kv => kv.Key).Distinct()];
            foreach (TKey c in columns) columnTotals[c] = 0;
            foreach (KeyValuePair<TKey, int> f in flattened)
            {
                columnTotals[f.Key] += f.Value;
            }
            foreach ((TKey rowKey, Dictionary<TKey, int> row) in input)
            {
                output[rowKey] = [];
                int rowTotal = row.Values.Sum();
                foreach ((TKey colKey, int count) in row)
                {
                    output[rowKey][colKey] = ((double)row[colKey] / (double)columnTotals[colKey]) / ((double)rowTotal / (double)total);
                }
                List<TKey> missingCols = [..columns.Except(row.Keys)];
                foreach (TKey missingCol in missingCols)
                {
                    output[rowKey][missingCol] = 0;
                }
            }
            return output;
        }

        public static DataTable ExpandToColumns<T>(IEnumerable<T> input, Glossaries glossaryInstance)
        {
            DataTable output = new();

            Type thisType = typeof(T);
            List<MemberInfo> columnMembers = [];

            columnMembers.AddRange(thisType.GetFields());
            columnMembers.AddRange(thisType.GetProperties());

            columnMembers.Where(mi => mi.GetCustomAttribute<XLColumnAttribute>() is not null ? !mi.GetCustomAttribute<XLColumnAttribute>().Ignore : true);
            columnMembers.OrderBy(mi =>
                mi.GetCustomAttribute<XLColumnAttribute>().Order
                );

            // Build table headers
            foreach (MemberInfo mi in columnMembers)
            {
                UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                string headerName;
                XLColumnAttribute xlColAttr = mi.GetCustomAttribute<XLColumnAttribute>();
                if (xlColAttr is null) headerName = mi.Name;
                else if (xlColAttr.Header is null) headerName = mi.Name;
                else headerName = xlColAttr.Header;
                if (biomeAttr is not null)
                {
                    foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                    {
                        output.Columns.Add(biomeAttr.Prefix + biomeName + biomeAttr.Suffix);
                        if (biomeAttr.DefaultValue is not null)
                        {
                            output.Columns[biomeAttr.Prefix + biomeName + biomeAttr.Suffix].DataType = biomeAttr.DefaultValue.GetType();
                        }
                        else
                        {
                            output.Columns[biomeAttr.Prefix + biomeName + biomeAttr.Suffix].DataType = typeof(string);
                        }
                    }
                }
                else
                {
                    output.Columns.Add(mi.Name);
                    Type memberType;
                    switch (mi.MemberType)
                    {
                        case (MemberTypes.Property):
                            memberType = ((PropertyInfo)mi).PropertyType;
                            break;
                        case (MemberTypes.Field):
                            memberType = ((FieldInfo)mi).FieldType;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    Type underlyingType = Nullable.GetUnderlyingType(memberType);
                    if (underlyingType is null) output.Columns[mi.Name].DataType = memberType;
                    else output.Columns[mi.Name].DataType = underlyingType;
                }
            }

            // Build table rows
            foreach (T entry in input)
            {
                DataRow dr = output.NewRow();

                foreach (MemberInfo mi in columnMembers)
                {
                    UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                    string headerMain;
                    XLColumnAttribute xlColAttr = mi.GetCustomAttribute<XLColumnAttribute>();
                    if (xlColAttr is null) headerMain = mi.Name;
                    else if (xlColAttr.Header is null) headerMain = mi.Name;
                    else headerMain = xlColAttr.Header;

                    if (biomeAttr is not null)
                    {
                        switch (mi.MemberType)
                        {
                            case MemberTypes.Field:
                                IDictionary fieldDict = ((FieldInfo)mi).GetValue(entry) as IDictionary;
                                List<string> fieldKeys = [];
                                foreach(DictionaryEntry de in fieldDict)
                                {
                                    fieldKeys.Add((string)de.Key);
                                }
                                foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                                {
                                    string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                    object fieldValue;
                                    if (fieldKeys.Contains(biomeName))
                                    {
                                        fieldValue = fieldDict[biomeName];
                                    }
                                    else
                                    {
                                        fieldValue = biomeAttr.DefaultValue;
                                    }
                                    Type ct = output.Columns[subheader].DataType;
                                    bool blank = false;
                                    if (biomeAttr.NullOnZeroOrBlank)
                                    {
                                        double? asNum = fieldValue as double?;
                                        string asString = fieldValue as string;
                                        blank |= (asNum is not null && asNum == 0);
                                        blank |= (asString is not null && asString.Length == 0);
                                    }
                                    if (blank) fieldValue = DBNull.Value;
                                    dr[subheader] = fieldValue;
                                }
                                break;
                            case MemberTypes.Property:
                                IDictionary propDict = ((FieldInfo)mi).GetValue(entry) as IDictionary;
                                List<string> propKeys = [];
                                foreach (DictionaryEntry de in propDict)
                                {
                                    propKeys.Add((string)de.Key);
                                }
                                foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                                {
                                    string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                    object propValue;
                                    if (propKeys.Contains(biomeName))
                                    {
                                        propValue = propDict[biomeName];
                                    }
                                    else
                                    {
                                        propValue = biomeAttr.DefaultValue;
                                    }
                                    Type ct = output.Columns[subheader].DataType;
                                    bool blank = false;
                                    if (biomeAttr.NullOnZeroOrBlank)
                                    {
                                        double? asNum = propValue as double?;
                                        string asString = propValue as string;
                                        blank |= (asNum is not null && asNum == 0);
                                        blank |= (asString is not null && asString.Length == 0);
                                    }
                                    if (blank) propValue = DBNull.Value;
                                    dr[subheader] = propValue;
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        switch (mi.MemberType)
                        {
                            case MemberTypes.Field:
                                var fieldPoint = ((FieldInfo)mi).GetValue(entry);
                                if (fieldPoint is null) dr[headerMain] = DBNull.Value;
                                else dr[headerMain] = fieldPoint;
                                break;
                            case MemberTypes.Property:
                                var propertyPoint = ((PropertyInfo)mi).GetValue(entry);
                                if (propertyPoint is null) dr[headerMain] = DBNull.Value;
                                else dr[headerMain] = propertyPoint;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                output.Rows.Add(dr);
            }
            return output;
        }

        public void WriteToExcel(string dstPath)
        {
            using (XLWorkbook wb = new())
            {
                var planetSummWs = wb.AddWorksheet("Planets");
                DataTable planetDataTable = ExpandToColumns(this.PlanetSummaries, this.glossaryInstance);
                {
                    List<MemberInfo> expandableColumns = [];
                    expandableColumns.AddRange(typeof(PlanetSummaryEntry).GetFields());
                    expandableColumns.AddRange(typeof(PlanetSummaryEntry).GetProperties());
                    foreach(MemberInfo mi in expandableColumns)
                    {
                        UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                        if (biomeAttr is null) continue;
                        if (biomeAttr.NumberFormat is not null)
                        {
                            foreach (string biomeName in this.glossaryInstance.BiomeHashByName.Keys)
                            {
                                string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                PlanetSummaryEntry.AddColumnFormat(biomeAttr.NumberFormat, subheader);
                            }
                        }
                        
                    }
                }
                var planetTable = planetSummWs.Cell("A1").InsertTable(planetDataTable, "Planets");
                planetTable.Theme = XLTableTheme.TableStyleMedium4;
                ApplyTableNumberFormats(PlanetSummaryEntry.GetColumnFormats(), planetTable);

                var cityWs = wb.AddWorksheet("Cities");
                var cityTable = cityWs.Cell("A1").InsertTable(this.CitySummaries, "Cities");
                cityTable.Theme = XLTableTheme.TableStyleLight1;
                ApplyTableNumberFormats(CitySummaryEntry.GetColumnFormats(), cityTable);

                var spiritWs = wb.AddWorksheet("Spirits");
                DataTable spiritDataTable = ExpandToColumns(this.SpiritStats.Values, this.glossaryInstance);
                {
                    List<MemberInfo> expandableColumns = [];
                    expandableColumns.AddRange(typeof(SpiritStatEntry).GetFields());
                    expandableColumns.AddRange(typeof(SpiritStatEntry).GetProperties());
                    foreach (MemberInfo mi in expandableColumns)
                    {
                        UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                        if (biomeAttr is null) continue;
                        if (biomeAttr.NumberFormat is not null)
                        {
                            foreach (string biomeName in this.glossaryInstance.BiomeHashByName.Keys)
                            {
                                string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                SpiritStatEntry.AddColumnFormat(biomeAttr.NumberFormat, subheader);
                            }
                        }

                    }
                }
                var spiritTable = spiritWs.Cell("A1").InsertTable(spiritDataTable, "Spirits");
                spiritTable.Theme = XLTableTheme.TableStyleMedium5;
                ApplyTableNumberFormats(SpiritStatEntry.GetColumnFormats(), spiritTable);

                var bioWs = wb.AddWorksheet("Biotica");
                DataTable bioticaDataTable = ExpandToColumns(this.BioticaStats.Values, this.glossaryInstance);
                var bioticaTable = bioWs.Cell("A1").InsertTable(bioticaDataTable, "Biotica");
                bioticaTable.Theme = XLTableTheme.TableStyleMedium3;
                ApplyTableNumberFormats(BioticumStatEntry.GetColumnFormats(), bioticaTable);

                DataTable bioticaVsSpiritCountDataTable = NestDictToDataTable(this.BioticumVsSpiritCounter, "Bioticum");
                var bioVsCharCountWs = wb.AddWorksheet("BioVsCharCounts");
                var bioVsCharCountTable = bioVsCharCountWs.Cell("A1").InsertTable(bioticaVsSpiritCountDataTable);

                DataTable bioticaVsSpiritRatioDataTable = NestDictToDataTable(this.BioticumVsSpiritRatios, "Bioticum");
                var bioVsCharRatioWs = wb.AddWorksheet("BioVsCharRatios");
                var bioVsCharRatioTable = bioVsCharRatioWs.Cell("A1").InsertTable(bioticaVsSpiritRatioDataTable);
                foreach (var col in bioVsCharRatioTable.Columns())
                {
                    col.Style.NumberFormat.Format = "0.0000";
                }

                wb.SaveAs(dstPath);
            }
        }

        public static readonly Dictionary<int, string> TimedChallengeTypes = new()
        {
            { 0, "Daily" },
            { 1, "Weekly" },
        };

        public static readonly Dictionary<int, string> DifficultyNames = new()
        {
            { 0, "Relaxing" },
            { 1, "Human" },
            { 2, "Giant" },
            { 3, "Titan" },
            { 4, "True Titan" },
        };

        private static double? SafePercent(int a0, int b0)
        {
            double? c = SafeDivide(a0, b0);
            if (c is null) return null;
            return Math.Max(Math.Min((double)c, 1.0), 0.0);
        }
        public static double? SafeDivide(int a0, int b0)
        {
            return SafeDivide((double)a0, (double)b0);
        }

        public static double? SafeDivide(double a, double b)
        {
            if (b == 0) return null;
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

            List<double> vSorted = [.. values.OrderBy(v => v)];

            double sumA = 0;
            double sumB = 0;
            for (int i = 0; i < vSorted.Count; i++)
            {
                double vi = vSorted[i];

                sumA += (i + 1) * vi;
                sumB += vi;
            }
            double g = 2 * (sumA / sumB);
            g -= n + 1;
            g /= n;

            return g;
        }
        public static double? GiniCoeff(List<int> values)
        {
            List<double> castValues = [.. values.Select(v => (double)v)];
            return GiniCoeff(castValues);
        }

        public static void ApplyTableNumberFormats(Dictionary<string, List<string>> columnFormats, IXLTable table)
        {
            foreach (KeyValuePair<string, List<string>> kv in columnFormats)
            {
                string format = kv.Key;
                List<string> columns = kv.Value;

                foreach (string colName in columns)
                {
                    try
                    {
                        var col = table.FindColumn(c => c.FirstCell().Value.ToString() == colName);
                        if (col is null) continue;
                        col.Style.NumberFormat.Format = format;
                    }
                    catch { }
                }
            }
        }
    }
}
