﻿using ClosedXML.Excel;
using ImageMagick.Drawing;
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
            PlanetSummaryEntry planetEntry = new(planet);
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
            planetEntry.AvPros = Statistics.Mean([.. cityProsList]);
            planetEntry.Gini = GiniCoeff(cityProsList);
            planetEntry.HiPros = cityProsList.Max();

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

            planetEntry.HiPop = cityPopList.Max();
            planetEntry.HiTech = cityTechList.Max();
            planetEntry.HiWel = cityWelList.Max();

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

                cityEntry.AvFBioLv = bioticaLevels.Average();
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
                    se.IncrementPlanetScoreTotalAsPrimary((int)planet.gameSession.turningPointPerformances.Last().scoreTotal);
                    se.PrScoreHi = Math.Max(se.PrScoreHi, (int)planet.gameSession.turningPointPerformances.Last().scoreTotal);
                }
                se.IncrementPlanetScoreTotal((int)planet.gameSession.turningPointPerformances.Last().scoreTotal);

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
            public readonly int Ser;
            public readonly DateTime TS;

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
            public double? FillP;

            public int HiPros;
            public double? ProsMdn, AvPros, Gini;
            public int Pop, Tech, Wel;
            public double? PPop, PTech, PWel;
            public int HiPop, HiTech, HiWel;
            public double? MdnPop, MdnTech, MdnWel, AvPop, AvTech, AvWel;

            public int? Biomes, CBiomes;
            public int Biotica, Plants, Animals, Minerals = 0;
            public int UqBiotica, UqPlants, UqAnimals, UqMinerals;
            public double? PPlant, PAnimal, PMineral;
            public int Apex;
            private int OccupiedSlotTotalLevel = 0;
            public double? ApexP, AvFBioLv;
            public double? DesertP, ForestP, IceAgeP, OceanP, RainforestP, SavannaP, TaigaP = null;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { 
                    "PPop", "PTech", "PWel", "PPlant", "PAnimal", "PMineral", "ApexP", "FillP",
                    "DesertP", "ForestP", "IceAgeP", "OceanP", "RainforestP", "SavannaP", "TaigaP",
                } },
                {"0.00", new List<string> { "AvPros", "Gini", "AvPop", "AvTech", "AvWel", "AvFBioLv" } },
                };

            public PlanetSummaryEntry(Planet planet)
            {
                this.N = planet.number;
                this.Name = planet.name;
                this.Ser = planet.epochMinutes;
                this.TS = DateTime.UnixEpoch.AddMinutes(this.Ser);
                this.TS = this.TS.ToLocalTime();
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
            public readonly int PlanetN, CityN;
            public readonly string Name;
            public string Char;
            public int? Level;
            public int Pros, Pop, Tech, Wel;
            public string FoundBiome, CurrBiome;
            public int? Rank, Upset = null;
            public double? PPop, PTech, PWel = null;
            public double? RelPros, RelPop, RelTech, RelWel = null;

            public int Invent, Trades = 0;
            public int TerrPatches = 0;

            public int TPLead = 0;
            public string TP1, TP2, TP3 = null;

            public int Biotica = 0;
            public double? AvFBioLv = null;
            public int FilledSlots = 0;
            public double? FillP = null;
            public int Plants, Animals, Minerals, Apex = 0;
            public double? PPlant, PAnimal, PMineral, ApexP = null;

            public int Buildings = 0;
            public string Lv1B, Lv2B, Lv3B, Era1B, Era2B, Era3B, Temple1, Temple2, Temple3 = null;

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "PPop", "PTech", "PWel", "FillP", "PPlant", "PAnimal", "PMineral", "ApexP"} },
                {"0.00", new List<string> { "RelPros", "RelPop", "RelTech", "RelWel", "AvFBioLv" } },
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
            public readonly string Name;
            public int Count = 0;
            public double? P = null;
            public int Prime = 0;
            public double? MainP = null;
            public double? PrimeP = null;

            private int totalPlanetScore = 0;
            public double? AvScore = null;

            private int totalPlanetScorePrimary = 0;
            public double? AvPrScore = null;

            private int prosTotal, popTotal, techTotal, welTotal = 0;
            private double popPercTotal, techPercTotal, welPercTotal = 0;
            private double prosRelTotal, popRelTotal, techRelTotal, welRelTotal = 0;

            public double? AvPros, AvPop, AvTech, AvWel = null;
            public int PrScoreHi = 0;
            public int HiPros, HiPop, HiTech, HiWel = 0;

            public double? AvPPop, AvPTech, AvPWel = null;
            public double HiPPop, HiPTech, HiPWel = 0;

            public double? AvRelPros, AvRelPop, AvRelTech, AvRelWel = null;
            public double HiRelPros, HiRelPop, HiRelTech, HiRelWel = 0;

            public int Terr = 0;
            public double? TerrAv = null;
            public int Invent = 0;
            public double? InventAv = null;
            public int Trades = 0;
            public double? TradeAv = null;

            private int upsetTotal = 0;
            public double? UpsetAv = null;
            public int PosUpset, NegUpset = 0;
            public double? PosUpsetP, NegUpsetP = null;

            public int Plants, Animals, Minerals = 0;
            private int activeBioticaCount = 0;
            private int totalActiveBioLevel = 0;
            public double? AvFBioLv = null;
            public double? PPlant, PAnimal, PMineral = null;
            //private double plantPercTotal, animalPercTotal, mineralPercTotal = 0;
            //public double? PlantAvP, AnimalAvP, MineralAvP = null;

            public int Apex = 0;
            public double? ApexP = null;
            //private double apexPercTotal = 0;
            //public double? ApexAvP = null;

            // Counts/percents of wild biome patches, per planet
            private int desertUse, forestUse, iceAgeUse, oceanUse, rainforestUse, savannaUse, taigaUse = 0;
            public double? HasDesert, HasForest, HasIceAge, HasOcean, HasRainforest, HasSavanna, HasTaiga = null;

            // Counts of wild patches in territory
            public int DesertSz, ForestSz, IceAgeSz, OceanSz, RainforestSz, SavannaSz, TaigaSz = 0;
            public double? DesertP, ForestP, IceAgeP, OceanP, RainforestP, SavannaP, TaigaP = null;
            /*// Sum/Average of percents of biomes in territory (sum per/average over planets)
            private double desertTotalP, forestTotalP, iceAgeTotalP, oceanTotalP, rainforestTotalP, savannaTotalP, taigaTotalP;
            public double? DesertAvP, ForestAvP, IceAgeAvP, OceanAvP, RainforestAvP, SavannaAvP, TaigaAvP = null;*/

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { 
                    "P", "PrimeP", "MainP",
                    "AvPPop", "AvPTech", "AvPWel", "HiPPop", "HiPTech", "HiPWel", 
                    "PosUpsetP", "NegUpsetP",
                    "PPlant", "PAnimal", "PMineral", "ApexP",
                    "HasDesert", "HasForest", "HasIceAge", "HasOcean", "HasRainforest", "HasSavanna", "HasTaiga",
                    "DesertP", "ForestP", "IceAgeP", "OceanP", "RainforestP", "SavannaP", "TaigaP",
                } },
                {"0.00", new List<string> { 
                    "AvPros", "AvPop", "AvTech", "AvWel", "AvScore", "AvPrScore",
                    "AvRelPros", "AvRelPop", "AvRelTech", "AvRelWel",
                    "HiRelPros", "HiRelPop", "HiRelTech", "HiRelWel",
                    "InventionAv", "TradeRouteAv",
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
