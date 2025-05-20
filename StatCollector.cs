using ClosedXML.Attributes;
using ClosedXML.Excel;
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
        public Dictionary<string, SpiritStatEntry> SpiritStats { get; private set; } = [];

        public Dictionary<string, Dictionary<string, int>> BioticumVsSpiritCounter { get; private set; } = [];
        private int planetCount = 0;
        private HashSet<string> draftedOrPlaced { get; set; } = [];

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

            
            // Count if drafted
            foreach (string draftDef in planet.MasteredBioticaDefSet)
            {
                draftedOrPlaced.Add(draftDef);
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
                draftedOrPlaced.Add(activeDef);
            }
            foreach (string legacyDef in legacyBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(legacyDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(legacyDef) is null) BioticaStats[legacyDef] = new BioticumStatEntry(legacyDef, planet.number);
                    else BioticaStats[legacyDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(legacyDef), planet.number);

                }
                BioticaStats[legacyDef].Legacy += legacyBioCounter[legacyDef];
                draftedOrPlaced.Add(legacyDef);
            }

            foreach (string draftDef in draftedOrPlaced)
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
                    }
                }
            }
            HashSet<string> missedDraft = [.. draftedOrPlaced.Except(biomeMatchingBiotica)];
            HashSet<string> availBiotica = [..biomeMatchingBiotica.Intersect(draftedOrPlaced)];
            foreach (string availDef in availBiotica)
            {
                if (BioticaStats.ContainsKey(availDef))
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
                if (planet.natureBioticumDictionary.ContainsKey((int)slot.bioticumId))
                {
                    planetEntry.FilledSlots += 1;
                    if (slot.slotLevel is not null) planetEntry.IncrementSlotTotalLevel((int)slot.slotLevel);
                }
            }

            Dictionary<int, Patch> wildPatches = planet.patchDictionary.Where(kv => kv.Value.IsWildPatch()).ToDictionary();
            int wildSlots = wildPatches.SelectMany(kv => kv.Value.GetActiveSlotIndices()).Count();
            planetEntry.FillP = SafePercent(planetEntry.FilledSlots, wildSlots);

            foreach ((string biomeName, double percent) in planet.BiomePercentages)
            {
                switch (biomeName) 
                {
                    case "Desert":
                        planetEntry.DesertP = percent;
                        break;
                    case "Forest":
                        planetEntry.ForestP = percent;
                        break;
                    case "Ice Age":
                    case "IceAge":
                        planetEntry.IceAgeP = percent;
                        break;
                    case "Ocean":
                        planetEntry.OceanP = percent;
                        break;
                    case "Rainforest":
                        planetEntry.RainforestP = percent;
                        break;
                    case "Savanna":
                        planetEntry.SavannaP = percent;
                        break;
                    case "Taiga":
                        planetEntry.TaigaP = percent;
                        break;
                }
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
                if (!this.SpiritStats.ContainsKey(founderName)) { this.SpiritStats[founderName] = new(founderName); }

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
                ApplyTableNumberFormats(CitySummaryEntry.GetColumnFormats(), cityTable);

                var spiritWs = wb.AddWorksheet("Spirits");
                var spiritTable = spiritWs.Cell("A1").InsertTable(this.SpiritStats.Values, "Spirits");
                spiritTable.Theme = XLTableTheme.TableStyleMedium5;
                ApplyTableNumberFormats(SpiritStatEntry.GetColumnFormats(), spiritTable);

                var bioWs = wb.AddWorksheet("Biotica");
                var bioticaTable = bioWs.Cell("A1").InsertTable(this.BioticaStats.Values, "Biotica");
                bioticaTable.Theme = XLTableTheme.TableStyleMedium3;
                ApplyTableNumberFormats(BioticumStatEntry.GetColumnFormats(), bioticaTable);

                DataTable bioticaVsSpiritTable = NestDictToDataTable(this.BioticumVsSpiritCounter, "Bioticum");
                var bioVsCharWs = wb.AddWorksheet("BioticaVsChar");
                var bioVsCharTable = bioVsCharWs.Cell("A1").InsertTable(bioticaVsSpiritTable);

                wb.SaveAs(dstPath);
            }
        }

        public class BioticumStatEntry
        {
            private readonly Glossaries.BioticumDefinition Definition;
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public readonly string Type;
            [XLColumn(Order = 2)] public readonly int? Tier;
            [XLColumn(Order = 3)] public readonly string Apex;

            [XLColumn(Order = 4)] public readonly string Desert, Forest, IceAge, Ocean, Rainforest, Savanna, Taiga;

            [XLColumn(Order = 20)] public readonly string Hash;
            [XLColumn(Order = 21)] public int Avail { get; set; } = 0;
            [XLColumn(Order = 22)] public double? AvailP { get; set; } = null;
            [XLColumn(Order = 23)] public int Draft { get; set; } = 0;
            [XLColumn(Order = 24)] public double? DraftP { get; set; } = null;
            [XLColumn(Order = 25)] public int Planets { get; set; } = 0;
            [XLColumn(Order = 26)] public double? AUsageP { get; private set; } = null;
            [XLColumn(Order = 27)] public double? DUsageP { get; private set; } = null;

            [XLColumn(Order = 30)] public int Total { get; set; } = 0;
            [XLColumn(Order = 31)] public int Legacy { get; set; } = 0;
            [XLColumn(Order = 32)] public double? LegacyP { get; private set; } = null;
            [XLColumn(Order = 33)] public int Final { get; set; } = 0;
            [XLColumn(Order = 34)] public double? FinalP { get; private set; } = null;

            private List<int> MultiNumberList = [];
            [XLColumn(Order = 40)] public int? Multi { get; set; } = null;
            [XLColumn(Order = 41)] public double? MultiP { get; private set; } = null;
            [XLColumn(Order = 42)] public int? MultiMx { get; private set; } = null;
            [XLColumn(Order = 43)] public double? MultiAv { get; private set; } = null;

            [XLColumn(Order = 50)] public int P1st { get; set; }
            [XLColumn(Order = 51)] public int PLast { get; set; }
            

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "AvailP", "DraftP", "AUsageP", "DUsageP", "LegacyP", "FinalP", "MultiP", } },
                {"0.000", new List<string> { "MultiAv", } },
                };

            public BioticumStatEntry(Glossaries.BioticumDefinition bioDef, int p1)
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
                this.P1st = p1;
                this.PLast = p1;
            }

            public BioticumStatEntry(string hash, int p1)
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

                this.P1st = p1;
                this.PLast = p1;
            }

            public void AddMultiValue(int value)
            {
                this.MultiNumberList.Add(value);
            }

            public void CalculateStats(int planetCount)
            {
                this.AUsageP = SafePercent(this.Planets, this.Avail);
                this.DUsageP = SafePercent(this.Planets, this.Draft);
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
            [XLColumn(Order = 0)] public readonly int N;
            [XLColumn(Order = 1)] public readonly string Name;
            [XLColumn(Order = 2)] public readonly int Ser;
            [XLColumn(Order = 3)] public readonly DateTime TS;

            private readonly int DiffValue;
            [XLColumn(Order = 5)] public readonly string Difficulty;
            [XLColumn(Order = 6)] public readonly int? ChIndex;
            [XLColumn(Order = 7)] public readonly string? ChType;
            [XLColumn(Order = 8)] public readonly DateTime? ChTS;

            [XLColumn(Order = 10)] public int Score;
            [XLColumn(Order = 11)] public int Pros;
            [XLColumn(Order = 12)] public string Giant1;
            [XLColumn(Order = 13)] public string Giant2;
            [XLColumn(Order = 14)] public string Giant3;
            [XLColumn(Order = 15)] public string Spirit;

            [XLColumn(Order = 16)] public int Cities;
            [XLColumn(Order = 17)] public int Prjs;

            [XLColumn(Order = 20)] public string Era1Name;
            [XLColumn(Order = 21)] public int? Era1Star; 
            [XLColumn(Order = 22)] public int? Era1Score;
            [XLColumn(Order = 23)] public string Era2Name;
            [XLColumn(Order = 24)] public int? Era2Star; 
            [XLColumn(Order = 25)] public int? Era2Score;
            [XLColumn(Order = 26)] public string Era3Name;
            [XLColumn(Order = 27)] public int? Era3Star;
            [XLColumn(Order = 28)] public int? Era3Score;

            [XLColumn(Order = 30)] public string Char1, Char2, Char3, Char4, Char5, Char6;

            [XLColumn(Order = 40)] public int SzT;
            [XLColumn(Order = 41)] public int SzWld;
            [XLColumn(Order = 42)] public int FilledSlots = 0;
            [XLColumn(Order = 43)] public double? FillP;

            [XLColumn(Order = 50)] public int HiPros;
            [XLColumn(Order = 51)] public double? ProsMdn;
            [XLColumn(Order = 52)] public double? AvPros;
            [XLColumn(Order = 53)] public double? Gini;

            [XLColumn(Order = 60)] public int Pop;
            [XLColumn(Order = 61)] public int Tech;
            [XLColumn(Order = 62)] public int Wel;
            [XLColumn(Order = 70)] public double? PPop;
            [XLColumn(Order = 71)] public double? PTech;
            [XLColumn(Order = 72)] public double? PWel;
            [XLColumn(Order = 80)] public int HiPop;
            [XLColumn(Order = 81)] public int HiTech;
            [XLColumn(Order = 82)] public int HiWel;
            [XLColumn(Order = 90)] public double? MdnPop;
            [XLColumn(Order = 91)] public double? MdnTech;
            [XLColumn(Order = 92)] public double? MdnWel;
            [XLColumn(Order = 100)] public double? AvPop;
            [XLColumn(Order = 101)] public double? AvTech;
            [XLColumn(Order = 102)] public double? AvWel;

            [XLColumn(Order = 110)] public int? Biomes;
            [XLColumn(Order = 111)] public int? CBiomes;

            [XLColumn(Order = 120)] public int Biotica = 0;
            [XLColumn(Order = 121)] public int Plants = 0;
            [XLColumn(Order = 122)] public int Animals = 0;
            [XLColumn(Order = 123)] public int Minerals = 0;

            [XLColumn(Order = 130)] public int UqBiotica;
            [XLColumn(Order = 131)] public int UqPlants;
            [XLColumn(Order = 132)] public int UqAnimals;
            [XLColumn(Order = 133)] public int UqMinerals;
            [XLColumn(Order = 134)] public double? PPlant, PAnimal, PMineral;

            [XLColumn(Order = 140)] public int Apex;
            [XLColumn(Order = 141)] private int OccupiedSlotTotalLevel = 0;
            [XLColumn(Order = 142)] public double? ApexP;
            [XLColumn(Order = 143)] public double? AvFBioLv;

            [XLColumn(Order = 150)] public double? DesertP, ForestP, IceAgeP, OceanP, RainforestP, SavannaP, TaigaP = null;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { 
                    "PPop", "PTech", "PWel", "PPlant", "PAnimal", "PMineral", "ApexP", "FillP",
                    "DesertP", "ForestP", "IceAgeP", "OceanP", "RainforestP", "SavannaP", "TaigaP",
                } },
                {"0.000", new List<string> { "AvPros", "AvPop", "AvTech", "AvWel", "AvFBioLv" } },
                {"0.0000", new List<string>  {"Gini"} },
                };

            public PlanetSummaryEntry(Planet planet)
            {
                this.N = planet.number;
                this.Name = planet.name;
                this.Ser = planet.epochMinutes;
                this.TS = DateTime.UnixEpoch.AddMinutes(this.Ser);
                this.TS = this.TS.ToLocalTime();

                this.DiffValue = (int)planet.gameSession.sessionDifficulty;
                if (DifficultyNames.TryGetValue(this.DiffValue, out string diffName))
                {
                    this.Difficulty = diffName;
                }
                else
                {
                    this.Difficulty = null;
                }

                this.ChIndex = planet.gameSession.challengeIndex > 0 ? planet.gameSession.challengeIndex : null;
                if (this.ChIndex is not null)
                {
                    this.ChTS = DateTime.UnixEpoch.AddSeconds((int)planet.gameSession.challengeTimestamp);
                    this.ChTS = ((DateTime)this.ChTS).ToLocalTime();
                    if (TimedChallengeTypes.TryGetValue((int)planet.gameSession.timedChallengeType, out string challengeType))
                    {
                        this.ChType = challengeType;
                    }
                    else
                    {
                        this.ChType = planet.gameSession.timedChallengeType.ToString();
                    }
                }
            }

            public void IncrementSlotTotalLevel(int value)
            {
                this.OccupiedSlotTotalLevel += value;
            }

            public void CalculateStats()
            {
                this.AvFBioLv = SafeDivide(this.OccupiedSlotTotalLevel, this.FilledSlots);
                this.PAnimal = SafePercent(this.Animals, this.Biotica);
                this.PPlant = SafePercent(this.Plants, this.Biotica);
                this.PMineral = SafePercent(this.Minerals, this.Biotica);
                this.ApexP = SafePercent(this.Apex, this.Biotica);
            }

            public static Dictionary<string, List<string>> GetColumnFormats()
            {
                return columnFormats;
            }

        }

        public class CitySummaryEntry
        {
            [XLColumn(Order = 0)] public readonly int PlanetN;
            [XLColumn(Order = 1)] public readonly int CityN;
            [XLColumn(Order = 2)] public readonly string Name;
            [XLColumn(Order = 3)] public string Char;
            [XLColumn(Order = 4)] public int? Level;

            [XLColumn(Order = 10)] public int Pros;
            [XLColumn(Order = 11)] public int Pop, Tech, Wel;

            [XLColumn(Order = 20)] public string FoundBiome;
            [XLColumn(Order = 21)] public string CurrBiome;

            [XLColumn(Order = 30)] public int? Rank = null;
            [XLColumn(Order = 31)] public int? Upset = null;

            [XLColumn(Order = 40)] public double? PPop, PTech, PWel = null;

            [XLColumn(Order = 50)] public double? RelPros = null;
            [XLColumn(Order = 51)] public double? RelPop, RelTech, RelWel = null;

            [XLColumn(Order = 60)] public int Invent = 0;
            [XLColumn(Order = 61)] public int Trades = 0;

            [XLColumn(Order = 70)] public int TerrPatches = 0;

            [XLColumn(Order = 80)] public int TPLead = 0;
            [XLColumn(Order = 81)] public string TP1, TP2, TP3 = null;

            [XLColumn(Order = 90)] public int Biotica = 0;
            [XLColumn(Order = 91)] public double? AvFBioLv = null;
            [XLColumn(Order = 92)] public int FilledSlots = 0;
            [XLColumn(Order = 93)] public double? FillP = null;

            [XLColumn(Order = 100)] public int Plants = 0;
            [XLColumn(Order = 101)] public int Animals = 0;
            [XLColumn(Order = 102)] public int Minerals = 0;
            [XLColumn(Order = 103)] public int Apex = 0;

            [XLColumn(Order = 110)] public double? PPlant = null;
            [XLColumn(Order = 111)] public double? PAnimal = null;
            [XLColumn(Order = 112)] public double? PMineral = null;
            [XLColumn(Order = 113)] public double? ApexP = null;

            [XLColumn(Order = 120)] public int Buildings = 0;
            [XLColumn(Order = 130)] public string Lv1B, Lv2B, Lv3B = null;
            [XLColumn(Order = 140)] public string Era1B, Era2B, Era3B = null;
            [XLColumn(Order = 150)] public string Temple1, Temple2, Temple3 = null;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "PPop", "PTech", "PWel", "FillP", "PPlant", "PAnimal", "PMineral", "ApexP"} },
                {"0.000", new List<string> { "RelPros", "RelPop", "RelTech", "RelWel", "AvFBioLv" } },
                };

            public static Dictionary<string, List<string>> GetColumnFormats() { return columnFormats; }

            public CitySummaryEntry(int planetN, int cityN, string name)
            {
                this.PlanetN = planetN;
                this.CityN = cityN;
                this.Name = name;
            }
        }

        public class SpiritStatEntry
        {
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public int Count = 0;
            [XLColumn(Order = 2)] public double? P = null;
            [XLColumn(Order = 3)] public int Prime = 0;
            [XLColumn(Order = 4)] public double? MainP = null;
            [XLColumn(Order = 5)] public double? PrimeP = null;

            private int totalPlanetScore = 0;
            [XLColumn(Order = 10)] public double? AvScore = null;

            private int totalPlanetScorePrimary = 0;
            [XLColumn(Order = 11)] public double? AvPrScore = null;

            private int prosTotal = 0;
            private int popTotal, techTotal, welTotal = 0;

            private double popPercTotal, techPercTotal, welPercTotal = 0;
            private double prosRelTotal = 0;
            private double popRelTotal, techRelTotal, welRelTotal = 0;

            [XLColumn(Order = 20)] public double? AvPros = null;
            [XLColumn(Order = 21)] public double? AvPop, AvTech, AvWel = null;
            [XLColumn(Order = 22)] public int PrScoreHi = 0;
            [XLColumn(Order = 23)] public int HiPros = 0;
            [XLColumn(Order = 24)] public int HiPop, HiTech, HiWel = 0;

            [XLColumn(Order = 30)] public double? AvPPop, AvPTech, AvPWel = null;
            [XLColumn(Order = 40)] public double HiPPop, HiPTech, HiPWel = 0;

            [XLColumn(Order = 50)] public double? AvRelPros = null;
            [XLColumn(Order = 51)] public double? AvRelPop, AvRelTech, AvRelWel = null;
            [XLColumn(Order = 60)] public double HiRelPros = 0;
            [XLColumn(Order = 61)] public double HiRelPop, HiRelTech, HiRelWel = 0;

            [XLColumn(Order = 70)] public int Terr = 0;
            [XLColumn(Order = 71)] public double? TerrAv = null;
            [XLColumn(Order = 80)] public int Invent = 0;
            [XLColumn(Order = 81)] public double? InventAv = null;
            [XLColumn(Order = 90)] public int Trades = 0;
            [XLColumn(Order = 91)] public double? TradeAv = null;

            private int upsetTotal = 0;
            [XLColumn(Order = 100)] public double? UpsetAv = null;
            [XLColumn(Order = 101)] public int PosUpset = 0;
            [XLColumn(Order = 102)] public int NegUpset = 0;
            [XLColumn(Order = 103)] public double? PosUpsetP = null;
            [XLColumn(Order = 104)] public double? NegUpsetP = null;

            [XLColumn(Order = 110)] public int Plants = 0;
            [XLColumn(Order = 111)] public int Animals = 0;
            [XLColumn(Order = 112)] public int Minerals = 0;
            private int activeBioticaCount = 0;
            private int totalActiveBioLevel = 0;
            [XLColumn(Order = 113)] public double? AvFBioLv = null;
            [XLColumn(Order = 114)] public double? PPlant = null;
            [XLColumn(Order = 115)] public double? PAnimal = null;
            [XLColumn(Order = 116)] public double? PMineral = null;

            [XLColumn(Order = 117)] public int Apex = 0;
            [XLColumn(Order = 118)] public double? ApexP = null;

            // Counts/percents of wild biome patches, per planet
            private int desertUse, forestUse, iceAgeUse, oceanUse, rainforestUse, savannaUse, taigaUse = 0;
            [XLColumn(Order = 120)] public double? HasDesert, HasForest, HasIceAge, HasOcean, HasRainforest, HasSavanna, HasTaiga = null;

            // Counts of wild patches in territory
            [XLColumn(Order = 130)] public int DesertSz, ForestSz, IceAgeSz, OceanSz, RainforestSz, SavannaSz, TaigaSz = 0;
            [XLColumn(Order = 140)] public double? DesertP, ForestP, IceAgeP, OceanP, RainforestP, SavannaP, TaigaP = null;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { 
                    "P", "PrimeP", "MainP",
                    "AvPPop", "AvPTech", "AvPWel", "HiPPop", "HiPTech", "HiPWel", 
                    "PosUpsetP", "NegUpsetP",
                    "PPlant", "PAnimal", "PMineral", "ApexP",
                    "HasDesert", "HasForest", "HasIceAge", "HasOcean", "HasRainforest", "HasSavanna", "HasTaiga",
                    "DesertP", "ForestP", "IceAgeP", "OceanP", "RainforestP", "SavannaP", "TaigaP",
                } },
                {"0.000", new List<string> { 
                    "AvPros", "AvPop", "AvTech", "AvWel", "AvScore", "AvPrScore",
                    "AvRelPros", "AvRelPop", "AvRelTech", "AvRelWel",
                    "HiRelPros", "HiRelPop", "HiRelTech", "HiRelWel",
                    "InventAv", "TradeAv",
                    "UpsetAv", "AvFBioLv", "TerrAv"
                } },
                };

            public static Dictionary<string, List<string>> GetColumnFormats() { return columnFormats; }

            public SpiritStatEntry(string spiritName)
            {
                this.Name = spiritName;
            }

            public void IncrementProsperityTotals(int pros, int pop, int tech, int wel)
            {
                this.prosTotal += pros;
                this.popTotal += pop;
                this.techTotal += tech;
                this.welTotal += wel;
            }

            public void IncrementProsperityPercentTotals(double popPerc, double techPerc, double welPerc)
            {
                this.popPercTotal += popPerc;
                this.techPercTotal += techPerc;
                this.welPercTotal += welPerc;
            }

            public void IncrementProsperityRelTotals(double RelPros, double RelPop, double RelTech, double RelWel)
            {
                this.prosRelTotal += RelPros;
                this.popRelTotal += RelPop;
                this.techRelTotal += RelTech;
                this.welRelTotal += RelWel;
            }

            public void IncrementUpsetTotal(int upset)
            {
                this.upsetTotal += upset;
            }

            //public void IncrementBioticaPercentTotals(double PPlant, double PAnimal, double PMineral, double apexP)
            /*public void IncrementBioticaPercentTotals(double apexP)
            {
                //this.plantPercTotal += PPlant;
                //this.animalPercTotal += PAnimal;
                //this.mineralPercTotal += PMineral;
                //this.apexPercTotal += apexP;
            }*/

            public void IncrementBioticaLevelTotal(int level)
            {
                this.totalActiveBioLevel += level;
                this.activeBioticaCount += 1;
            }

            public void IncrementBioticaLevelTotal(List<int> levels)
            {
                foreach(int level in levels)
                {
                    this.IncrementBioticaLevelTotal(level);
                }
            }

            public void IncrementBiomeUsage(Dictionary<string, int> BiomePatchCounts)
            {
                foreach (string bn in BiomePatchCounts.Keys)
                {
                    int patches = BiomePatchCounts[bn];
                    this.Terr += patches;
                    switch (bn)
                    {
                        case "Desert":
                            this.desertUse += 1;
                            this.DesertSz += patches;
                            break;
                        case "Forest":
                            this.forestUse += 1;
                            this.ForestSz += patches;
                            break;
                        case "Ice Age":
                        case "IceAge":
                            this.iceAgeUse += 1;
                            this.IceAgeSz += patches;
                            break;
                        case "Ocean":
                            this.oceanUse += 1;
                            this.OceanSz += patches;
                            break;
                        case "Rainforest":
                            this.rainforestUse += 1;
                            this.RainforestSz += patches;
                            break;
                        case "Savanna":
                            this.savannaUse += 1;
                            this.SavannaSz += patches;
                            break;
                        case "Taiga":
                            this.taigaUse += 1;
                            this.TaigaSz += patches;
                            break;
                        default: break;
                    }
                }
            }

            public void IncrementPlanetScoreTotal(int score)
            {
                this.totalPlanetScore += score;
            }

            public void IncrementPlanetScoreTotalAsPrimary(int score)
            {
                this.totalPlanetScorePrimary += score;
            }

            public void CalculateStats(int planetCount) 
            {
                this.P = SafePercent(this.Count, planetCount);
                this.PrimeP = SafePercent(this.Prime, this.Count);
                this.MainP = SafePercent(this.Prime, planetCount);

                this.AvScore = SafeDivide(this.totalPlanetScore, this.Count);
                this.AvPrScore = SafeDivide(this.totalPlanetScorePrimary, this.Prime);

                this.AvPros = SafeDivide(this.prosTotal, this.Count);
                this.AvPop = SafeDivide(this.popTotal, this.Count);
                this.AvTech = SafeDivide(this.techTotal, this.Count);
                this.AvWel = SafeDivide(this.welTotal, this.Count);

                this.AvPPop = SafeDivide(this.popPercTotal, this.Count);
                this.AvPTech = SafeDivide(this.techPercTotal, this.Count);
                this.AvPWel = SafeDivide(this.welPercTotal, this.Count);

                this.AvRelPros = SafeDivide(this.prosRelTotal, this.Count);
                this.AvRelPop = SafeDivide(this.popRelTotal, this.Count);
                this.AvRelTech = SafeDivide(this.techRelTotal, this.Count);
                this.AvRelWel = SafeDivide(this.welRelTotal, this.Count);

                this.TerrAv = SafeDivide(this.Terr, this.Count);
                this.InventAv = SafeDivide(this.Invent, this.Count);
                this.TradeAv = SafeDivide(this.Trades, this.Count);

                this.UpsetAv = SafeDivide(this.upsetTotal, this.Count);
                this.PosUpsetP = SafeDivide(this.PosUpset, this.Count);
                this.NegUpsetP = SafeDivide(this.NegUpset, this.Count);

                int bioticaCount = this.Plants + this.Animals + this.Minerals;
                this.AvFBioLv = SafeDivide(this.totalActiveBioLevel, this.activeBioticaCount);
                this.PPlant = SafePercent(this.Plants, bioticaCount);
                this.PAnimal = SafePercent(this.Animals, bioticaCount);
                this.PMineral = SafePercent(this.Minerals, bioticaCount);

                //this.PlantAvP = SafeDivide(this.plantPercTotal, this.Count);
                //this.AnimalAvP = SafeDivide(this.animalPercTotal, this.Count);
                //this.MineralAvP = SafeDivide(this.mineralPercTotal, this.Count);

                this.ApexP = SafePercent(this.Apex, bioticaCount);
                //this.ApexAvP = SafeDivide(this.apexPercTotal, this.Count);

                this.HasDesert = SafePercent(this.desertUse, this.Count);
                this.HasForest = SafePercent(this.forestUse, this.Count);
                this.HasIceAge = SafePercent(this.iceAgeUse, this.Count);
                this.HasOcean = SafePercent(this.oceanUse, this.Count);
                this.HasRainforest = SafePercent(this.rainforestUse, this.Count);
                this.HasSavanna = SafePercent(this.savannaUse, this.Count);
                this.HasTaiga = SafePercent(this.taigaUse, this.Count);

                this.DesertP = SafePercent(this.DesertSz, this.Terr);
                this.ForestP = SafePercent(this.ForestSz, this.Terr);
                this.IceAgeP = SafePercent(this.IceAgeSz, this.Terr);
                this.OceanP = SafePercent(this.OceanSz, this.Terr);
                this.RainforestP = SafePercent(this.RainforestSz, this.Terr);
                this.SavannaP = SafePercent(this.SavannaSz, this.Terr);
                this.TaigaP = SafePercent(this.TaigaSz, this.Terr);
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

            List<double> vSorted = [..values.OrderBy(v => v)];

            double sumA = 0;
            double sumB = 0;
            for (int i = 0; i < vSorted.Count; i++)
            {
                double vi = vSorted[i];

                sumA += (i+1) * vi;
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
