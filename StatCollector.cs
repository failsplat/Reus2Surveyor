using MathNet.Numerics.Statistics;
using Reus2Surveyor.GameObjects;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using static Reus2Surveyor.Glossaries;

namespace Reus2Surveyor
{
    public partial class StatCollector
    {
        public OrderedDictionary<string, BioticumStatEntry> BioticaStats { get; private set; } = []; // keyed to definition hash
        public List<PlanetSummaryEntry> PlanetSummaries { get; private set; } = [];
        public List<CitySummaryEntry> CitySummaries { get; private set; } = [];
        public OrderedDictionary<string, SpiritStatEntry> SpiritStats { get; private set; } = []; // keyed to spirit name

        public OrderedDictionary<string, Dictionary<string, int>> BioticumVsSpiritCounter { get; private set; } = [];
        public OrderedDictionary<string, Dictionary<string, double>> BioticumVsSpiritRatios { get; private set; } = [];
        // First key is bioticum
        // Second key is spirit or character
        public OrderedDictionary<string, Dictionary<string, int>> BioticumVsPrSpiritCounter { get; private set; } = [];
        public OrderedDictionary<string, Dictionary<string, double>> BioticumVsPrSpiritRatios { get; private set; } = [];

        public OrderedDictionary<string, LuxuryStatEntry> LuxuryStats { get; private set; } = []; // keyed to definition hash

        private int planetCount = 0;
        private HashSet<string> BioDraftedOrPlacedInProfile { get; set; } = [];

        // Debugging/Spading for inventions
        public Dictionary<string, string> genericBuffNamesByDef = []; // def:name
        public HashSet<string> inventionDefinitions = [];
        public Dictionary<string, string> inventionNamesByDef = [];

        // Keyed to era def hash
        public OrderedDictionary<string, EraStatEntry> EraStats { get; private set; } = [];

        // Keyed to project def hash
        public OrderedDictionary<string, ProjectStatEntry> ProjectStats { get; private set; } = [];
        private Dictionary<string, int> ProjectSlotCount = [];
        public List<TopBioticumSummary> TopBioticumSummaries { get; private set; } = [];
        public StatCollector()
        {
        }

        public void ConsumePlanet(Planet planet, int index)
        {
            if (planet is null) return;

            this.UpdateBioticaStats(planet, index);
            this.UpdateHumanityStats(planet, index);
            this.CountBioticaVsSpirit(planet, index, Glossaries.SpiritNameFromHash(planet.GameSession.StartParameters.SelectedCharacter));
            this.planetCount++;
        }

        public void UpdateBioticaStats(Planet planet, int index)
        {
            if (planet is null) return;

            Dictionary<string, int> activeBioCounter = [];
            Dictionary<string, int> legacyBioCounter = [];
            //Dictionary<(string,string), int> bioPropertyDict = [];

            foreach ((string def, int count) in planet.LegacyBioticaDefCounter)
            {
                IncrementCounter(legacyBioCounter, def, count);
            }
            foreach ((string def, int count) in planet.ActiveBioticaDefCounter)
            {
                IncrementCounter(activeBioCounter, def, count);
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
            foreach (string draftDef in planet.GameplayController.MasteredBiotica)
            {
                BioDraftedOrPlacedInProfile.Add(draftDef);
                draftedOrPlacedInSession.Add(draftDef);
            }

            // Make entries for active then archived then complete
            foreach (string activeDef in activeBioCounter.Keys)
            {
                CheckBioticaStatEntry(activeDef, planet.Number);
                BioticaStats[activeDef].Final += activeBioCounter[activeDef];
                if (planet.GameSession.TopBioticaSummaries.Count > 0)
                {
                    int activeCount = activeBioCounter[activeDef];
                    int allCount = activeCount;
                    if (legacyBioCounter.TryGetValue(activeDef, out int legacyCount)) allCount += legacyCount;
                    BioticaStats[activeDef].IncrementTop5Available(activeCount, allCount);
                }
                BioDraftedOrPlacedInProfile.Add(activeDef);
                draftedOrPlacedInSession.Add(activeDef);
            }
            foreach (string legacyDef in legacyBioCounter.Keys)
            {
                CheckBioticaStatEntry(legacyDef, planet.Number);
                BioticaStats[legacyDef].Legacy += legacyBioCounter[legacyDef];
                BioDraftedOrPlacedInProfile.Add(legacyDef);
                draftedOrPlacedInSession.Add(legacyDef);
            }

            // Count all biotica that are available in available biomes
            // Only increment if it has been drafted or placed in this planet or previous planets
            // (Could be not unavailable by level or DLC)
            HashSet<string> biomeMatchingBiotica = [];
            foreach (string giantHash in planet.GameSession.StartParameters.SelectedGiantDefinitions)
            {
                Glossaries.GiantDefinition gd = Glossaries.TryGiantDefinitionFromHash(giantHash);
                if (gd.Biome1 is null || gd.Biome2 is null) continue; // Unknown giant, don't calculate biome-matching biotica
                foreach (Glossaries.BioticumDefinition bd in Glossaries.BioticumDefinitionList)
                {
                    bool b1match = bd.IsBiomeAllowed(gd.Biome1);
                    bool b2match = bd.IsBiomeAllowed(gd.Biome2);
                    if (b1match || b2match)
                    {
                        biomeMatchingBiotica.Add(bd.Hash);
                        if (bd.Starter)
                        {
                            BioDraftedOrPlacedInProfile.Add(bd.Hash);
                            draftedOrPlacedInSession.Add(bd.Hash);
                        }
                    }
                }
            }

            foreach (string draftDef in draftedOrPlacedInSession)
            {
                CheckBioticaStatEntry(draftDef, planet.Number);
                BioticaStats[draftDef].Draft += 1;
            }

            foreach (string cDef in completeBioCounter.Keys)
            {
                // CheckBioticaStatEntry(cDef, planet.number);
                BioticaStats[cDef].Planets += 1;
                BioticaStats[cDef].Total += completeBioCounter[cDef];
                BioticaStats[cDef].PLast = planet.Number;
                if (completeBioCounter[cDef] > 1) BioticaStats[cDef].AddMultiValue(completeBioCounter[cDef]);
            }

            HashSet<string> missedDraft = [.. draftedOrPlacedInSession.Except(biomeMatchingBiotica)];
            HashSet<string> availBiotica = [.. biomeMatchingBiotica.Intersect(BioDraftedOrPlacedInProfile)];

            // Special case: The Farmer's Frontier Farm special biotica
            // Marked with -1 in all land biomes
            bool farmBioOk = Glossaries.BioticumDefinitionByName.TryGetValue("Frontier Farm", out BioticumDefinition farmBioDef);
            bool aqFarmBioOk = Glossaries.BioticumDefinitionByName.TryGetValue("Aquatic Frontier Farm", out BioticumDefinition aqFarmBioDef);

            if (Glossaries.SpiritNameFromHash(planet.GameSession.StartParameters.SelectedCharacter) == "Farmer" && farmBioOk)
            {
                availBiotica.Add(farmBioDef.Hash);
                availBiotica.Add(aqFarmBioDef.Hash);
            }
            else if (farmBioOk)
            {
                availBiotica.Remove(farmBioDef.Hash);
                availBiotica.Remove(aqFarmBioDef.Hash);
            }

            foreach (string availDef in availBiotica)
            {
                if (BioDraftedOrPlacedInProfile.Contains(availDef))
                {
                    BioticaStats[availDef].Avail += 1;
                }
            }

            List<TopBioticumSummary> planetTopBio = [];
            foreach (SessionSummary.TopBioticumSummary tbe in planet.GameSession.TopBioticaSummaries)
            {
                planetTopBio.Add(new TopBioticumSummary(index, 0, tbe));
                this.BioticaStats[tbe.BioticumType].Top5 += 1;
                this.BioticaStats[tbe.BioticumType].AddTop5Score(tbe.TotalValue);
            }
            planetTopBio = [.. planetTopBio.OrderBy(tbe => -tbe.TotalValue)];
            for (int topBioIndex = 0; topBioIndex < planetTopBio.Count; topBioIndex++)
            {
                planetTopBio[topBioIndex].SetRank(topBioIndex + 1);
            }
            planetTopBio.Reverse();
            this.TopBioticumSummaries.AddRange(planetTopBio);
            // Rank descending, add to end 
            // Reversed during finalization, most recent planet first, 1st place first
        }

        public void UpdateHumanityStats(Planet planet, int index)
        {
            // Planet Summary
            PlanetSummaryEntry planetEntry = new(planet);
            List<TurningPointPerformance> eraPerformances = planet.GameSession.EraPerformances;
            if (eraPerformances.Count > 0)
            {
                planetEntry.Score = eraPerformances.Last().TotalScore;

                foreach (TurningPointPerformance tpp in eraPerformances)
                {
                    Glossaries.EraDefinition eraDef = Glossaries.TryEraDefinitionFromHash(tpp.Definition);
                    if (!this.EraStats.TryGetValue(eraDef.Hash, out EraStatEntry ese))
                    {
                        this.EraStats[eraDef.Hash] = new(eraDef);
                    }
                    ese = this.EraStats[eraDef.Hash];
                    ese.Count += 1;
                    ese.eraScores.Add(tpp.TotalScore);

                    switch (tpp.starRating)
                    {
                        case 3:
                            ese.Star3 += 1;
                            continue;
                        case 2:
                            ese.Star2 += 1;
                            continue;
                        case 1:
                            ese.Star1 += 1;
                            continue;
                        case 0:
                            ese.Star0 += 1;
                            continue;
                    }
                }
            }

            List<string> giantNames = [..planet.GameSession.StartParameters.SelectedGiantDefinitions
                .Select(s => Glossaries.TryGiantDefinitionFromHash(s))
                .Select(gd => (gd.Name, gd.Position))
                .OrderBy(t => t.Position)
                .Select(t => t.Name)
                ];

            planetEntry.Giant1 = giantNames[0];
            planetEntry.Giant2 = giantNames[1];
            planetEntry.Giant3 = giantNames[2];

            planetEntry.Spirit = Glossaries.SpiritNameFromHash(planet.GameSession.StartParameters.SelectedCharacter);

            List<int> cityProsList = [];
            List<int> cityPopList = [];
            List<int> cityTechList = [];
            List<int> cityWelList = [];

            HashSet<string> luxuriesPresent = [];

            planetEntry.Cities = planet.Cities.Count;

            int cityIndex = 0; // Starts at 1, increments at beginning of loop
            foreach (City city in planet.Cities.Values)
            {
                cityIndex += 1;

                planetEntry.Prjs += city.Projects.Count;
                planetEntry.Invent += city.LuxuryGoods.Count;
                planetEntry.Trades += city.TradeGoods.Count;

                if (city.CivSummary is not null)
                {
                    cityProsList.Add((int)city.CivSummary.prosperity);
                    cityPopList.Add((int)city.CivSummary.population);
                    cityTechList.Add((int)city.CivSummary.innovation);
                    cityWelList.Add((int)city.CivSummary.wealth);
                }

                string founderName = Glossaries.SpiritNameFromHash(city.FoundingCharacterDef);
                typeof(PlanetSummaryEntry).GetField("Char" + cityIndex.ToString()).SetValue(planetEntry, founderName);
            }

            planetEntry.PrjAv = SafeDivide(planetEntry.Prjs, planetEntry.Cities);
            planetEntry.InventAv = SafeDivide(planetEntry.Invent, planetEntry.Cities);
            planetEntry.TradeAv = SafeDivide(planetEntry.Trades, planetEntry.Cities);

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

            int eraCount = planet.GameSession.EraPerformances.Count;
            if (eraCount >= 1)
            {
                planetEntry.Era1Name = Glossaries.EraNameFromHash(planet.GameSession.EraPerformances[0].Definition);
                planetEntry.Era1Score = planet.GameSession.EraPerformances[0].TotalScore;
                planetEntry.Era1Star = planet.GameSession.EraPerformances[0].starRating;
            }
            if (eraCount >= 2)
            {
                planetEntry.Era2Name = Glossaries.EraNameFromHash(planet.GameSession.EraPerformances[1].Definition);
                planetEntry.Era2Score = planet.GameSession.EraPerformances[1].TotalScore;
                planetEntry.Era2Star = planet.GameSession.EraPerformances[1].starRating;
            }
            if (eraCount >= 3)
            {
                planetEntry.Era3Name = Glossaries.EraNameFromHash(planet.GameSession.EraPerformances[2].Definition);
                planetEntry.Era3Score = planet.GameSession.EraPerformances[2].TotalScore;
                planetEntry.Era3Star = planet.GameSession.EraPerformances[2].starRating;
            }

            planetEntry.SzT = planet.TotalSize;
            planetEntry.SzWld = planet.WildSize;

            List<Biome> activeBiomes = [.. planet.Biomes.Values.ToList().Where(b => b.AnchorPatch is not null)];
            planetEntry.Biomes = activeBiomes.Count;
            planetEntry.CBiomes = planet.GameSession.CoolBiomeCount;

            foreach ((string bioHash, int count) in planet.ActiveBioticaDefCounter)
            {
                BioticumDefinition bd = Glossaries.BioticumDefFromHash(bioHash);
                if (bd.Apex) planetEntry.Apex += count;
                switch (bd.Type)
                {
                    case "Plant":
                        planetEntry.Plants += count;
                        continue;
                    case "Animal":
                        planetEntry.Animals += count;
                        continue;
                    case "Mineral":
                        planetEntry.Minerals += count;
                        continue;
                }
            }
            foreach ((string bioHash, int count) in planet.LegacyBioticaDefCounter)
            {
                BioticumDefinition bd = Glossaries.BioticumDefFromHash(bioHash);
                if (bd.Apex) planetEntry.Apex += count;
                switch (bd.Type)
                {
                    case "Plant":
                        planetEntry.Plants += count;
                        continue;
                    case "Animal":
                        planetEntry.Animals += count;
                        continue;
                    case "Mineral":
                        planetEntry.Minerals += count;
                        continue;
                }
            }
            HashSet<string> uqBioHashes = [.. planet.ActiveBioticaDefCounter.Keys, .. planet.LegacyBioticaDefCounter.Keys];
            foreach (string bioHash in uqBioHashes)
            {
                BioticumDefinition bd = Glossaries.BioticumDefFromHash(bioHash);
                switch (bd.Type)
                {
                    case "Plant":
                        planetEntry.UqPlants += 1;
                        continue;
                    case "Animal":
                        planetEntry.UqAnimals += 1;
                        continue;
                    case "Mineral":
                        planetEntry.UqMinerals += 1;
                        continue;
                }
            }

            planetEntry.Biotica = planet.ActiveBioticaDefCounter.Values.Sum() + planet.LegacyBioticaDefCounter.Values.Sum();
            planetEntry.UqBiotica = uqBioHashes.Count;
            foreach (BioticumSlot slot in planet.BioticumSlots.Values)
            {
                if (slot.ActiveBioticum is null) continue;
                if (slot.isInvasiveSlot) planetEntry.InvasiveSpots += 1;
                if (planet.ActiveBiotica.ContainsKey((int)slot.BioticumIndex))
                {
                    planetEntry.FilledSlots += 1;
                    planetEntry.IncrementSlotTotalLevel((int)slot.slotLevel);
                }
            }

            Dictionary<int, Patch> wildPatches = planet.Patches.Where(kv => kv.Value.IsWild).ToDictionary();
            int wildSlots = wildPatches.SelectMany(kv => kv.Value.ActiveSlotIndices).Count();
            planetEntry.FillP = SafePercent(planetEntry.FilledSlots, wildSlots);

            planetEntry.Creeks = wildPatches.Values.Where(p => p.SpecialNaturalFeatureValue == (int)Glossaries.SpecialNaturalFeatures.Creek).Count();
            planetEntry.Anomalies = wildPatches.Values.Where(p => p.SpecialNaturalFeatureValue == (int)Glossaries.SpecialNaturalFeatures.Anomaly).Count();
            planetEntry.Sanctuaries = wildPatches.Values.Where(p => p.SpecialNaturalFeatureValue == (int)Glossaries.SpecialNaturalFeatures.Sanctuary).Count();
            planetEntry.MountainSlots = wildPatches.Values.Where(p => p.MountainPart > 0).Count();

            foreach ((string biomeName, double percent) in planet.BiomeSizeMap.Values)
            {
                planetEntry.biomePercents[biomeName] = percent;
            }

            this.PlanetSummaries.Add(planetEntry);

            // City Summary and Spirit Stats
            List<CitySummaryEntry> thisPlanetCitySummaries = [];
            List<City> citiesInOrder = [.. planet.Cities.ToList().OrderBy(kv => kv.Key).Select(kv => kv.Value)];
            Dictionary<int, City> citiesByLuxuryBuffHandler = [];
            int? cannedSludgeCity = null;
            string cannedSludgeHash = null;
            int cityN = 0;
            Dictionary<string, HashSet<string>> cityBioBySpiritName = [];
            foreach (City city in citiesInOrder)
            {
                cityN++;
                CitySummaryEntry cityEntry = new(index, cityN, city.fancyName);

                string founderName = Glossaries.SpiritNameFromHash(city.FoundingCharacterDef);

                cityEntry.Char = founderName;
                cityEntry.Level = city.currentVisualStage + 1;

                cityEntry.Pros = city.CivSummary.prosperity;
                cityEntry.Pop = city.CivSummary.population;
                cityEntry.Tech = city.CivSummary.innovation;
                cityEntry.Wel = city.CivSummary.wealth;

                cityEntry.FoundBiome = Glossaries.BiomeNameFromHash(city.SettledBiome);
                cityEntry.CurrBiome = Glossaries.BiomeNameFromHash(city.CurrentBiomeDefinition);

                cityEntry.PPop = SafePercent(cityEntry.Pop, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);
                cityEntry.PTech = SafePercent(cityEntry.Tech, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);
                cityEntry.PWel = SafePercent(cityEntry.Wel, cityEntry.Pop + cityEntry.Tech + cityEntry.Wel);

                cityEntry.RelPros = cityEntry.Pros / planetEntry.ProsMdn;
                cityEntry.RelPop = cityEntry.Pop / planetEntry.MdnPop;
                cityEntry.RelTech = cityEntry.Tech / planetEntry.MdnTech;
                cityEntry.RelWel = cityEntry.Wel / planetEntry.MdnWel;

                cityEntry.Invent = city.LuxuryGoods.Count;
                cityEntry.Trades = city.TradeGoods.Count();
                cityEntry.TerrPatches = city.PatchesInTerritory.Where(p => p.IsWild).Count();

                foreach (CityObjects.LuxurySlot luxSlot in city.LuxurySlots)
                {
                    CityObjects.LuxuryGood good = luxSlot.LuxuryGood;
                    if (good is null) continue;
                    string luxHash = good.Definition;
                    this.inventionDefinitions.Add(luxHash);

                    LuxuryDefinition luxDef = Glossaries.TryLuxuryDefinitionFromHash(luxHash);
                    if (!this.LuxuryStats.TryGetValue(luxHash, out LuxuryStatEntry lse))
                    {
                        LuxuryStatEntry newEntry = new(luxDef);
                        lse = newEntry;
                        this.LuxuryStats.Add(luxHash, lse);
                    }

                    string? inspiringBio = good.BioDefinition;
                    lse.Copies += 1;
                    if (good.OriginCityId == city.TokenIndex)
                    {
                        if (this.LuxuryStats[luxHash].LeaderCountsOri.ContainsKey(founderName))
                        {
                            this.LuxuryStats[luxHash].LeaderCountsOri[founderName] += 1;
                            this.LuxuryStats[luxHash].ICount += 1;
                        }

                        if (inspiringBio is not null && Glossaries.BioticumDefinitionByHash.TryGetValue(inspiringBio, out BioticumDefinition luxSrcBioDef))
                        {
                            if (this.LuxuryStats[luxHash].BioticaSourceCounts.ContainsKey(luxSrcBioDef.Name)) this.LuxuryStats[luxHash].BioticaSourceCounts[luxSrcBioDef.Name] += 1;
                            else this.LuxuryStats[luxHash].BioticaSourceCounts[luxSrcBioDef.Name] = 1;
                        }
                    }
                    if (lse.LeaderCounts.ContainsKey(founderName))
                    {
                        lse.LeaderCounts[founderName] += 1;
                    }
                    luxuriesPresent.Add(luxHash);

                    if (luxDef.Name == "Canned Sludge")
                    {
                        cannedSludgeCity = city.TokenIndex;
                        cannedSludgeHash = luxHash;
                    }

                    if (inspiringBio is not null)
                    {
                        CheckBioticaStatEntry(inspiringBio, index);
                        this.BioticaStats[inspiringBio].Inventions += 1;
                    }
                }
                citiesByLuxuryBuffHandler[(int)city.LuxuryBuffControllerId] = city;
                foreach (CityObjects.LuxurySlot tradeSlot in city.TradeSlots)
                {
                    if (tradeSlot is null) continue;
                    if (tradeSlot.LuxuryGood is null) continue; // Empty trade slot
                    string importHash = tradeSlot.LuxuryGood.Definition;
                    LuxuryDefinition importDef = Glossaries.TryLuxuryDefinitionFromHash(importHash);

                    if (!this.LuxuryStats.TryGetValue(importHash, out LuxuryStatEntry lse))
                    {
                        LuxuryStatEntry newEntry = new(importDef);
                        lse = newEntry;
                        this.LuxuryStats.Add(importHash, lse);
                    }

                    lse.Copies += 1;
                    if (lse.LeaderCounts.ContainsKey(founderName))
                    {
                        lse.LeaderCounts[founderName] += 1;
                    }
                }

                cityEntry.TPLead = city.InitiatedTurningPoints.Count;
                foreach (string cityStartedEras in city.InitiatedTurningPoints)
                {
                    EraDefinition thisEra = Glossaries.TryEraDefinitionFromHash(cityStartedEras);
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
                foreach (Patch wildPatch in city.PatchesInTerritory.Where(p => p.IsWild))
                {
                    foreach (int slotIndex in wildPatch.ActiveSlotIndices)
                    {
                        BioticumSlot slot = planet.BioticumSlots[slotIndex];
                        slotCount += 1;
                        if (slot.BioticumIndex is not null) cityEntry.FilledSlots += 1;
                    }
                }
                cityEntry.FillP = SafePercent(cityEntry.FilledSlots, slotCount);

                cityEntry.Biotica = city.BioticaInTerritory.Count;
                List<int> bioticaLevels = []; // Ending levels, active bio only
                HashSet<string> bioticaInCity = []; // All bio hashes

                // Active biotica only!
                foreach (NatureBioticum nb in city.BioticaInTerritory)
                {
                    if (Glossaries.BioticumDefinitionByHash.TryGetValue(nb.Definition, out BioticumDefinition thisBio))
                    {
                        bioticaLevels.Add(thisBio.Tier);
                        bioticaInCity.Add(thisBio.Hash);
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
                    foreach (int slotIndex in patch.ActiveSlotIndices)
                    {
                        BioticumSlot slot = planet.BioticumSlots[slotIndex];
                        foreach (string abd in slot.ArchivedBioticaDefs)
                        {
                            BioticumDefinition thisLegBio = Glossaries.BioticumDefFromHash(abd);
                            if (thisLegBio is null) continue;
                            bioticaInCity.Add(thisLegBio.Hash);
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

                    if (Glossaries.BiomeNameByHash.TryGetValue(patch.BiomeDefinition, out string patchBiome))
                    {
                        cityEntry.IncrementPatchBiomeCounter(patchBiome);
                    }
                }
                cityEntry.CalculateBiomePercentages(city.PatchIdsInTerritory.Count());

                cityEntry.AvFBioLv = bioticaLevels.Count > 0 ? bioticaLevels.Average() : 0;
                cityEntry.PPlant = SafePercent(cityEntry.Plants, cityEntry.Biotica);
                cityEntry.PAnimal = SafePercent(cityEntry.Animals, cityEntry.Biotica);
                cityEntry.PMineral = SafePercent(cityEntry.Minerals, cityEntry.Biotica);
                cityEntry.ApexP = SafePercent(cityEntry.Apex, cityEntry.Biotica);

                /*foreach (string bdic in bioticaInCity)
                {
                    BioticumDefinition cityBioDef = Glossaries.BioticumDefFromHash(bdic);
                    if (cityBioDef is null) continue;
                    switch (cityBioDef.Type)
                    {
                        case "Plant":
                            cityEntry.UqPlant += 1;
                            break;
                        case "Animal":
                            cityEntry.UqAnimal += 1;
                            break;
                        case "Mineral":
                            cityEntry.UqMineral += 1;
                            break;
                    }
                    if (cityBioDef.Apex) cityEntry.UqApex += 1;
                }*/

                cityBioBySpiritName[founderName] = bioticaInCity;

                foreach (CityObjects.Project project in city.Projects)
                {
                    cityEntry.Buildings += 1;
                    if (Glossaries.ProjectDefinitionByHash.ContainsKey(project.Definition))
                    {
                        CityProjectDefinition projectDef = Glossaries.TrProjectDefinitionFromHash(project.Definition);
                        if (!ProjectStats.TryGetValue(projectDef.Hash, out ProjectStatEntry pse))
                        {
                            pse = new(projectDef);
                            this.ProjectStats[projectDef.Hash] = pse;
                        }

                        pse.IncrementCounts(founderName);

                        if (!this.ProjectSlotCount.ContainsKey(projectDef.Slot)) this.ProjectSlotCount[projectDef.Slot] = 0;
                        this.ProjectSlotCount[projectDef.Slot] += 1;

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
                            case "Special":
                                cityEntry.SpecialProject = projectDef.DisplayName;
                                break;
                            default:
                                Trace.TraceError($"Unknown project/project slot: {projectDef.DisplayName}");
                                break;
                        }
                    }
                    else
                    {
                        CityProjectDefinition projectDef = Glossaries.TrProjectDefinitionFromHash(project.Definition, project.name);
                        if (!ProjectStats.TryGetValue(projectDef.Hash, out ProjectStatEntry pse))
                        {
                            pse = new(projectDef);
                            this.ProjectStats[projectDef.Hash] = pse;
                        }

                        pse.IncrementCounts(founderName);
                    }
                }

                thisPlanetCitySummaries.Add(cityEntry);

                Dictionary<string, int> biomePatchesInCity = [];
                foreach (Patch patch in city.PatchesInTerritory)
                {
                    if (!patch.IsWild) continue;
                    string patchBiome = Glossaries.BiomeNameFromHash(patch.BiomeDefinition);
                    if (!biomePatchesInCity.ContainsKey(patchBiome)) biomePatchesInCity[patchBiome] = 0;
                    biomePatchesInCity[patchBiome] += 1;
                }
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
                if (!this.SpiritStats.TryGetValue(founderName, out SpiritStatEntry se))
                {
                    se = new(founderName);
                    this.SpiritStats[founderName] = se;
                }

                se.Count += 1;
                if (ce.CityN == 1)
                {
                    se.Prime += 1;
                    if (eraPerformances.Count > 0)
                    {
                        se.IncrementPlanetScoreTotalAsPrimary((int)eraPerformances.Last().TotalScore);
                        se.IncrementPlanetProsAverageAsPrimary(Statistics.Mean([.. cityProsList]));
                        se.HiPrScore = Math.Max(se.HiPrScore, (int)eraPerformances.Last().TotalScore);
                    }
                }
                if (eraPerformances.Count > 0) se.IncrementPlanetScoreTotal((int)eraPerformances.Last().TotalScore);

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
                se.IncrementUpsetTotal(upset, (int)ce.CityN == 1, (int)ce.Rank == 1);
                if (upset > 0) se.PosUpset += 1;
                if (upset < 0) se.NegUpset += 1;

                se.Plants += ce.Plants;
                se.Animals += ce.Animals;
                se.Minerals += ce.Minerals;

                //se.IncrementBioticaPercentTotals((double)ce.PPlant, (double)ce.PAnimal, (double)ce.PMineral, (double)ce.ApexP);
                //se.IncrementBioticaPercentTotals((double)ce.ApexP);
                se.Apex += ce.Apex;

                se.AddBioUsed(cityBioBySpiritName[founderName]);
            }

            foreach (City city in planet.Cities.Values)
            {
                List<int> bioticaLevels = [];
                string founderName = Glossaries.SpiritNameFromHash(city.FoundingCharacterDef);
                foreach (NatureBioticum nb in city.BioticaInTerritory)
                {
                    if (Glossaries.BioticumDefinitionByHash.TryGetValue(nb.Definition, out BioticumDefinition thisBio))
                    {
                        bioticaLevels.Add(thisBio.Tier);
                    }
                }
                this.SpiritStats[founderName].IncrementBioticaLevelTotal(bioticaLevels);

                Dictionary<string, int> biomePatchesInCity = [];
                foreach (Patch patch in city.PatchesInTerritory)
                {
                    if (!patch.IsWild) continue;
                    string patchBiome = Glossaries.BiomeNameFromHash(patch.BiomeDefinition);
                    if (!biomePatchesInCity.ContainsKey(patchBiome)) biomePatchesInCity[patchBiome] = 0;
                    biomePatchesInCity[patchBiome] += 1;
                }
                this.SpiritStats[founderName].IncrementBiomeUsage(biomePatchesInCity);
            }

            this.CitySummaries.AddRange(thisPlanetCitySummaries);

            // Generic buff checking
            foreach (GenericBuff buff in planet.GenericBuffs.Values)
            {
                // for spading/debug
                this.genericBuffNamesByDef.TryAdd(buff.Definition, buff.name);

                if (buff.name == "Canned Sludge")
                {
                    if (citiesByLuxuryBuffHandler.TryGetValue(buff.Owner, out City buffCity))
                    {
                        string founderName = Glossaries.SpiritNameFromHash(buffCity.FoundingCharacterDef);
                        if (buffCity.TokenIndex != cannedSludgeCity)
                        {
                            this.LuxuryStats[cannedSludgeHash].Copies += 1;
                            this.LuxuryStats[cannedSludgeHash].LeaderCounts[founderName] += 1;
                        }
                    }
                }
            }
            foreach (string luxHash in luxuriesPresent)
            {
                this.LuxuryStats[luxHash].Planets += 1;
            }
        }

        public void CountBioticaVsSpirit(Planet planet, int index, string primarySpirit)
        {
            foreach (City city in planet.Cities.Values)
            {
                string spirit = Glossaries.SpiritNameFromHash(city.FoundingCharacterDef);
                foreach (NatureBioticum nb in city.BioticaInTerritory)
                {
                    if (Glossaries.BioticumDefinitionByHash.ContainsKey(nb.Definition))
                    {
                        string activeBioName = Glossaries.BioticumNameFromHash(nb.Definition);
                        this.IncrementSpiritVsBioticaCounters(activeBioName, spirit, primarySpirit);
                    }
                }
                foreach (int slotIndex in city.PatchesInTerritory.SelectMany(p => p.ActiveSlotIndices))
                {
                    BioticumSlot slot = planet.BioticumSlots[slotIndex];
                    foreach (string legacyDef in slot.ArchivedBioticaDefs)
                    {
                        if (Glossaries.BioticumDefinitionByHash.ContainsKey(legacyDef))
                        {
                            string legacyBioName = Glossaries.BioticumNameFromHash(legacyDef);
                            this.IncrementSpiritVsBioticaCounters(legacyBioName, spirit, primarySpirit);
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
            foreach (SpiritStatEntry sse in this.SpiritStats.Values)
            {
                sse.CalculateStats(this.planetCount);
            }
            this.BioticumVsSpiritRatios = NestedCounterToNestedRatioDictionary(this.BioticumVsSpiritCounter);
            this.BioticumVsPrSpiritRatios = NestedCounterToNestedRatioDictionary(this.BioticumVsPrSpiritCounter);
            this.inventionNamesByDef = this.genericBuffNamesByDef.Where(kv => this.inventionDefinitions.Contains(kv.Key)).ToDictionary();

            Dictionary<string, Dictionary<string, int>> luxuryLeaderCounts = [];
            double spiritTotal = (double)this.SpiritStats.Values.Select((SpiritStatEntry sse) => sse.Count).Sum();
            Dictionary<string, double> leaderPercents = this.SpiritStats.ToDictionary(kv => kv.Key, kv => (double)kv.Value.Count / spiritTotal);
            foreach (LuxuryStatEntry lse in this.LuxuryStats.Values)
            {
                //luxuryLeaderCounts[lse.Hash] = lse.LeaderCounts;
                lse.LeaderRatios = lse.LeaderCountsOri.ToDictionary(kv => kv.Key,
                    kv => leaderPercents.TryGetValue(kv.Key, out double leaderPerc) ?
                    ((double)kv.Value / (double)lse.Copies) / leaderPerc :
                    0
                    );

                lse.FavSpirit = lse.LeaderRatios.MaxBy(kv => kv.Value).Key;
                lse.FavSpRatio = lse.LeaderRatios.MaxBy(kv => kv.Value).Value;

                Dictionary<string, double> srcBioRatios = [];
                foreach (string srcBioName in lse.BioticaSourceCounts.Keys)
                {
                    if (Glossaries.BioticumDefinitionByName.TryGetValue(srcBioName, out BioticumDefinition srcBioDef))
                    {
                        srcBioRatios[srcBioName] = (double)this.BioticaStats[srcBioDef.Hash].Total / lse.BioticaSourceCounts[srcBioName];
                    }
                }
                if (srcBioRatios.Count > 0)
                {
                    lse.FavSourceBioticum = srcBioRatios.MaxBy(kv => kv.Value).Key;
                    lse.FavBioRatio = srcBioRatios.MaxBy(kv => kv.Value).Value;
                }

                lse.CalculateStats(this.planetCount);
            }

            foreach ((string bioticumName, Dictionary<string, double> ratios) in this.BioticumVsSpiritRatios)
            {
                if (Glossaries.BioticumDefinitionByName.TryGetValue(bioticumName, out Glossaries.BioticumDefinition bioDef))
                {
                    this.BioticaStats[bioDef.Hash].FavSpirit = ratios.MaxBy(kv => kv.Value).Key;
                    this.BioticaStats[bioDef.Hash].FavRatio = ratios.MaxBy(kv => kv.Value).Value;
                }
            }

            Dictionary<int, int> stageCounter = [];
            for (int i = 0; i < 5; i++)
            {
                stageCounter[i] = 0;
            }
            foreach (EraStatEntry ese in this.EraStats.Values)
            {
                stageCounter[ese.Era] += ese.Count;
            }
            foreach (EraStatEntry ese in this.EraStats.Values)
            {
                ese.CalculateStats(stageCounter[ese.Era]);
            }

            // Preparing counters to gather project slot usage
            List<string> distinctProjectSlots = [..Glossaries.ProjectDefinitionList
                .Select(d => d.Slot).Distinct()];
            Dictionary<string, int> projectSlotCounter = distinctProjectSlots.ToDictionary(k => k, k => 0);
            Dictionary<(string, string), int> projectSlotCountByLeader = [];
            foreach (string leaderName in Glossaries.SpiritHashByName.Keys)
            {
                foreach (string projectSlot in distinctProjectSlots)
                {
                    projectSlotCountByLeader[(leaderName, projectSlot)] = 0;
                }
            }

            foreach (ProjectStatEntry pse in this.ProjectStats.Values)
            {
                if (pse.Slot is not null) 
                {
                    projectSlotCounter[pse.Slot] += pse.Count;
                    foreach (string leader in pse.LeaderCounts.Keys)
                    {
                        projectSlotCountByLeader[(leader, pse.Slot)] += pse.LeaderCounts[leader];
                    }
                }
            }
            foreach (ProjectStatEntry pse in this.ProjectStats.Values)
            {
                pse.CalculateStats(projectSlotCounter, projectSlotCountByLeader);
            }

            // Bradley-Terry for spirit rankings

            // Count overtakes
            Dictionary<(string, string), int> btSpiritMatchups = [];
            foreach (string spiritName1 in Glossaries.SpiritHashByName.Keys)
            {
                foreach (string spiritName2 in Glossaries.SpiritHashByName.Keys)
                {
                    if (spiritName1 != spiritName2)
                    {
                        btSpiritMatchups[(spiritName1, spiritName2)] = 0;
                    }
                }
            }

            HashSet<string> btHasDefeat = [];
            foreach (IGrouping<int,CitySummaryEntry> planetCities in this.CitySummaries.GroupBy(cs => cs.PlanetN))
            {
                List<CitySummaryEntry> citiesInOrder = [..planetCities.ToList().OrderBy(cs => cs.CityN)];
                for (int cnA = 0; cnA < citiesInOrder.Count; cnA++)
                {
                    for (int cnB = 0; cnB < cnA; cnB++)
                    {
                        CitySummaryEntry cityA = citiesInOrder[cnA];
                        CitySummaryEntry cityB = citiesInOrder[cnB];

                        if ((cityA.CityN > cityB.CityN) && (cityA.Rank < cityB.Rank)) 
                        {
                            btSpiritMatchups[(cityA.Char, cityB.Char)] += 1;
                            btHasDefeat.Add(cityB.Char);
                        }
                    }
                }
            }

            // Iteration fails for any undefeated spirits
            // Ok to have no-wins (weight goes to 0)
            Dictionary<string, double> btWeights = [];
            foreach (string defeatedSpirit in btHasDefeat)
            {
                btWeights[defeatedSpirit] = 1;
            }

            // Iterate until weight changes small enough
            double convergenceLimit = 0.01;
            double lastDevSquared = 1;
            int btIterCount = 0;
            List<string> btSpirits = [.. btHasDefeat]; // change to list to make sure iteration order is same every time
            while (lastDevSquared > convergenceLimit && btIterCount < 100)
            {
                btIterCount++;
                Dictionary<string, double> newWeights = btWeights.ToDictionary();

                foreach (string spiritI in btSpirits)
                {
                    double numer = 0;
                    double denom = 0;

                    double pI = newWeights[spiritI];

                    foreach (string spiritJ in btSpirits)
                    {
                        if (spiritI == spiritJ) continue;

                        int winsIJ = btSpiritMatchups[(spiritI, spiritJ)];
                        int winsJI = btSpiritMatchups[(spiritJ, spiritI)];

                        // if both pI and pJ are zero (both have 0 wins), the formula fails
                        // skip this matchup, leave numerator zero

                        double pJ = newWeights[spiritJ];
                        if ((pI + pJ) == 0) continue;
                        numer += (winsIJ * pJ) / (pI + pJ);
                        denom += winsJI / (pI + pJ);
                    }

                    if (denom > 0) newWeights[spiritI] = numer / denom;
                    else if (numer == 0 && denom == 0) newWeights[spiritI] = 0;
                }

                lastDevSquared = 0;
                foreach (string sp in btSpirits)
                {
                    lastDevSquared += Math.Pow((newWeights[sp] - btWeights[sp]), 2);
                }

                btWeights = newWeights;
            }

            foreach (string sp in btWeights.Keys)
            {
                if (!Double.IsNaN(btWeights[sp])) this.SpiritStats[sp].BtWeight = btWeights[sp];
            }

            this.TopBioticumSummaries.Reverse();
        }

        public void CheckBioticaStatEntry(string bioHash, int planetNum)
        {
            if (!BioticaStats.ContainsKey(bioHash))
            {
                if (Glossaries.BioticumDefFromHash(bioHash) is null) this.BioticaStats[bioHash] = new BioticumStatEntry(bioHash, planetNum);
                else this.BioticaStats[bioHash] = new BioticumStatEntry(Glossaries.BioticumDefFromHash(bioHash), planetNum);
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

        public void IncrementSpiritVsBioticaCounters(string bioName, string spiritName, string primarySpirit)
        {
            if (!BioticumVsSpiritCounter.ContainsKey(bioName)) this.BioticumVsSpiritCounter[bioName] = new();
            if (!BioticumVsSpiritCounter[bioName].ContainsKey(spiritName)) this.BioticumVsSpiritCounter[bioName][spiritName] = 0;
            this.BioticumVsSpiritCounter[bioName][spiritName] += 1;

            if (!BioticumVsPrSpiritCounter.ContainsKey(bioName)) this.BioticumVsPrSpiritCounter[bioName] = new();
            if (!BioticumVsPrSpiritCounter[bioName].ContainsKey(primarySpirit)) this.BioticumVsPrSpiritCounter[bioName][primarySpirit] = 0;
            this.BioticumVsPrSpiritCounter[bioName][primarySpirit] += 1;
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
    }
}
