using ClosedXML.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using static Reus2Surveyor.Glossaries;

namespace Reus2Surveyor
{
    public partial class StatCollector
    {
        public class BioticumStatEntry
        {
            private readonly Glossaries.BioticumDefinition Definition;
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public readonly string Type;
            [XLColumn(Order = 2)] public readonly int? Tier;
            [XLColumn(Order = 3)] public readonly string Starter;
            [XLColumn(Order = 4)] public readonly string Apex;

            [XLColumn(Order = 10)][UnpackToBiomes(defaultValue: "")] public readonly Dictionary<string, string> biomesAllowed = [];

            [XLColumn(Order = 20)] public readonly string Hash;
            [XLColumn(Order = 21)] public int Total { get; set; } = 0;
            [XLColumn(Order = 22)] public int Planets { get; set; } = 0;
            [XLColumn(Order = 23)][ColumnFormat("0.00%")] public double? DUsageP { get; private set; } = null;
            [XLColumn(Order = 24)][ColumnFormat("0.00%")] public double? AUsageP { get; private set; } = null;
            [XLColumn(Order = 25)] public int Draft { get; set; } = 0;
            [XLColumn(Order = 26)][ColumnFormat("0.00%")] public double? DraftP { get; set; } = null;
            [XLColumn(Order = 27)] public int Avail { get; set; } = 0;
            [XLColumn(Order = 27)][ColumnFormat("0.000")] public double? AvRate { get; set; } = null;
            [XLColumn(Order = 28)][ColumnFormat("0.00%")] public double? AvailP { get; set; } = null;

            [XLColumn(Order = 30)] public int Legacy { get; set; } = 0;
            [XLColumn(Order = 31)][ColumnFormat("0.00%")] public double? LegacyP { get; private set; } = null;
            [XLColumn(Order = 32)] public int Final { get; set; } = 0;
            [XLColumn(Order = 33)][ColumnFormat("0.00%")] public double? FinalP { get; private set; } = null;

            private List<int> MultiNumberList = [];
            [XLColumn(Order = 40)] public int? Multi { get; set; } = null;
            [XLColumn(Order = 41)][ColumnFormat("0.00%")] public double? MultiP { get; private set; } = null;
            [XLColumn(Order = 42)] public int? MultiMx { get; private set; } = null;
            [XLColumn(Order = 43)][ColumnFormat("0.000")] public double? MultiAv { get; private set; } = null;
            [XLColumn(Order = 44)] public string FavSpirit { get; set; } = null;
            [XLColumn(Order = 45)][ColumnFormat("0.000")] public double? FavRatio { get; set; } = null;
            [XLColumn(Order = 46)] public int Inventions { get; set; } = 0;

            [XLColumn(Order = 50)] public int P1st { get; set; }
            [XLColumn(Order = 51)] public int PLast { get; set; }

            public BioticumStatEntry(Glossaries.BioticumDefinition bioDef, int p1)
            {
                this.Definition = bioDef;
                this.Name = bioDef.Name;
                this.Type = bioDef.Type;
                this.Tier = bioDef.Tier;
                this.Starter = bioDef.Starter ? "Y" : null;
                this.Apex = bioDef.Apex ? "☆" : null;

                foreach ((string biomeName, bool allowed) in bioDef.BiomesAllowed)
                {
                    this.biomesAllowed[biomeName] = allowed ? "Y" : null;
                }

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
                this.Starter = "?";
                this.Apex = "?";
                this.Hash = hash;

                this.P1st = p1;
                this.PLast = p1;
            }

            public void AddMultiValue(int value)
            {
                this.MultiNumberList.Add(value);
            }

            public void CalculateStats(int planetCount)
            {
                this.Draft = Math.Min(this.Avail, this.Draft);

                this.AUsageP = SafePercent(this.Planets, this.Avail);
                this.DUsageP = SafePercent(this.Planets, this.Draft);
                this.LegacyP = SafePercent(this.Legacy, this.Total);
                this.FinalP = SafePercent(this.Final, this.Total);
                this.DraftP = SafePercent(this.Draft, this.Avail);
                this.AvRate = SafeDivide(this.Total, this.Avail);
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

            [XLColumn(Order = 20)] public int Cities;
            [XLColumn(Order = 21)] public string Char1;
            [XLColumn(Order = 22)] public string Char2;
            [XLColumn(Order = 23)] public string Char3;
            [XLColumn(Order = 24)] public string Char4;
            [XLColumn(Order = 25)] public string Char5;

            [XLColumn(Order = 30)] public string Era1Name;
            [XLColumn(Order = 31)] public int? Era1Star;
            [XLColumn(Order = 32)] public int? Era1Score;
            [XLColumn(Order = 33)] public string Era2Name;
            [XLColumn(Order = 34)] public int? Era2Star;
            [XLColumn(Order = 35)] public int? Era2Score;
            [XLColumn(Order = 36)] public string Era3Name;
            [XLColumn(Order = 37)] public int? Era3Star;
            [XLColumn(Order = 38)] public int? Era3Score;

            [XLColumn(Order = 50)] public int HiPros;
            [XLColumn(Order = 51)] public double? ProsMdn;
            [XLColumn(Order = 52)][ColumnFormat("0.000")] public double? AvPros;
            [XLColumn(Order = 53)][ColumnFormat("0.000")] public double? Gini;

            [XLColumn(Order = 60)] public int Pop;
            [XLColumn(Order = 61)] public int Tech;
            [XLColumn(Order = 62)] public int Wel;
            [XLColumn(Order = 70)][ColumnFormat("0.00%")] public double? PPop;
            [XLColumn(Order = 71)][ColumnFormat("0.00%")] public double? PTech;
            [XLColumn(Order = 72)][ColumnFormat("0.00%")] public double? PWel;
            [XLColumn(Order = 80)] public int HiPop;
            [XLColumn(Order = 81)] public int HiTech;
            [XLColumn(Order = 82)] public int HiWel;
            [XLColumn(Order = 90)] public double? MdnPop;
            [XLColumn(Order = 91)] public double? MdnTech;
            [XLColumn(Order = 92)] public double? MdnWel;
            [XLColumn(Order = 100)][ColumnFormat("0.000")] public double? AvPop;
            [XLColumn(Order = 101)][ColumnFormat("0.000")] public double? AvTech;
            [XLColumn(Order = 102)][ColumnFormat("0.000")] public double? AvWel;

            [XLColumn(Order = 110)] public int Prjs;
            [XLColumn(Order = 111)][ColumnFormat("0.000")] public double? PrjAv;
            [XLColumn(Order = 112)] public int Invent = 0;
            [XLColumn(Order = 113)][ColumnFormat("0.000")] public double? InventAv;
            [XLColumn(Order = 114)] public int Trades = 0;
            [XLColumn(Order = 115)][ColumnFormat("0.000")] public double? TradeAv;

            [XLColumn(Order = 120)] public int? Biomes;
            [XLColumn(Order = 121)] public int? CBiomes;

            [XLColumn(Order = 122)] public int SzT;
            [XLColumn(Order = 123)] public int SzWld;
            [XLColumn(Order = 124)] public int FilledSlots = 0;
            [XLColumn(Order = 125)][ColumnFormat("0.00%")] public double? FillP;

            [XLColumn(Order = 130)] public int Biotica = 0;
            [XLColumn(Order = 131)] public int Plants = 0;
            [XLColumn(Order = 132)] public int Animals = 0;
            [XLColumn(Order = 133)] public int Minerals = 0;

            [XLColumn(Order = 140)] public int UqBiotica;
            [XLColumn(Order = 141)] public int UqPlants;
            [XLColumn(Order = 142)] public int UqAnimals;
            [XLColumn(Order = 143)] public int UqMinerals;
            [XLColumn(Order = 144)][ColumnFormat("0.00%")] public double? PPlant;
            [XLColumn(Order = 144)][ColumnFormat("0.00%")] public double? PAnimal;
            [XLColumn(Order = 144)][ColumnFormat("0.00%")] public double? PMineral;

            [XLColumn(Order = 150)] public int Apex;
            [XLColumn(Order = 151)] private int OccupiedSlotTotalLevel = 0;
            [XLColumn(Order = 152)][ColumnFormat("0.00%")] public double? ApexP;
            [XLColumn(Order = 153)][ColumnFormat("0.000")] public double? AvFBioLv;

            [XLColumn(Order = 160)] public int Creeks = 0;
            [XLColumn(Order = 161)] public int InvasiveSpots = 0;
            [XLColumn(Order = 162)] public int Anomalies = 0;
            [XLColumn(Order = 163)] public int Sanctuaries = 0;
            [XLColumn(Order = 164)] public int MountainSlots = 0;

            [XLColumn(Order = 160), UnpackToBiomes(defaultValue: (double)0, suffix: "P", numberFormat: "0.00%")]
            public Dictionary<string, double> biomePercents = [];

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

            [XLColumn(Order = 40)][ColumnFormat("0.00%")] public double? PPop, PTech, PWel = null;

            [XLColumn(Order = 50)][ColumnFormat("0.000")] public double? RelPros = null;
            [XLColumn(Order = 51)][ColumnFormat("0.000")] public double? RelPop, RelTech, RelWel = null;

            [XLColumn(Order = 60)] public int Invent = 0;
            [XLColumn(Order = 61)] public int Trades = 0;

            [XLColumn(Order = 70)] public int TerrPatches = 0;

            [XLColumn(Order = 80)] public int TPLead = 0;
            [XLColumn(Order = 81)] public string TP1, TP2, TP3 = null;

            [XLColumn(Order = 90)] public int Biotica = 0;
            [XLColumn(Order = 91)][ColumnFormat("0.000")] public double? AvFBioLv = null;
            [XLColumn(Order = 92)] public int FilledSlots = 0;
            [XLColumn(Order = 93)][ColumnFormat("0.00%")] public double? FillP = null;

            [XLColumn(Order = 100)] public int Plants = 0;
            [XLColumn(Order = 101)] public int Animals = 0;
            [XLColumn(Order = 102)] public int Minerals = 0;
            [XLColumn(Order = 103)] public int Apex = 0;

            [XLColumn(Order = 104)][ColumnFormat("0.00%")] public double? PPlant = null;
            [XLColumn(Order = 105)][ColumnFormat("0.00%")] public double? PAnimal = null;
            [XLColumn(Order = 106)][ColumnFormat("0.00%")] public double? PMineral = null;
            [XLColumn(Order = 107)][ColumnFormat("0.00%")] public double? ApexP = null;

            //[XLColumn(Order = 108)] public int UqPlant = 0;
            //[XLColumn(Order = 109)] public int UqAnimal = 0;
            //[XLColumn(Order = 110)] public int UqMineral = 0;
            //[XLColumn(Order = 111)] public int UqApex = 0;

            [XLColumn(Order = 120)] public int Buildings = 0;
            [XLColumn(Order = 130)] public string Lv1B, Lv2B, Lv3B = null;
            [XLColumn(Order = 140)] public string Era1B, Era2B, Era3B = null;
            [XLColumn(Order = 150)] public string Temple1, Temple2, Temple3 = null;

            [XLColumn(Order = 160)]
            [UnpackToBiomes(defaultValue: (double)0, suffix: "P", numberFormat: "0.00%")]
            public Dictionary<string, double> BiomePercents = [];
            private Dictionary<string, int> biomePatchCounts = [];

            public CitySummaryEntry(int planetN, int cityN, string name)
            {
                this.PlanetN = planetN;
                this.CityN = cityN;
                this.Name = name;
            }

            public void IncrementPatchBiomeCounter(string biomeName)
            {
                if (!biomePatchCounts.ContainsKey(biomeName))
                {
                    this.biomePatchCounts[biomeName] = 0;
                }
                this.biomePatchCounts[biomeName]++;
            }

            public void CalculateBiomePercentages(int terrSize)
            {
                foreach ((string biomeName, int count) in this.biomePatchCounts)
                {
                    this.BiomePercents[biomeName] = (double)count / (double)terrSize;
                }
            }
        }

        public class SpiritStatEntry
        {
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public int Count = 0;
            [XLColumn(Order = 2)][ColumnFormat("0.00%")] public double? P = null;
            [XLColumn(Order = 3)] public int Prime = 0;
            [XLColumn(Order = 4)][ColumnFormat("0.00%")] public double? MainP = null;
            [XLColumn(Order = 5)][ColumnFormat("0.00%")] public double? PrimeP = null;

            private int totalPlanetScore = 0;
            [XLColumn(Order = 10)][ColumnFormat("0.000")] public double? AvScore = null;

            private int totalPlanetScorePrimary = 0;
            [XLColumn(Order = 11)][ColumnFormat("0.000")] public double? AvPrTScore = null;
            private double totalPlanetCityProsAverage = 0;
            [XLColumn(Order = 12)][ColumnFormat("0.000")] public double? AvPrAPros = null;

            private int prosTotal = 0;
            private int popTotal, techTotal, welTotal = 0;

            private double popPercTotal, techPercTotal, welPercTotal = 0;
            private double prosRelTotal = 0;
            private double popRelTotal, techRelTotal, welRelTotal = 0;

            [XLColumn(Order = 20)][ColumnFormat("0.000")] public double? AvPros = null;
            [XLColumn(Order = 21)][ColumnFormat("0.000")] public double? AvPop, AvTech, AvWel = null;
            [XLColumn(Order = 22)] public int HiPrScore = 0;
            [XLColumn(Order = 23)] public int HiPros = 0;
            [XLColumn(Order = 24)] public int HiPop, HiTech, HiWel = 0;

            [XLColumn(Order = 30)][ColumnFormat("0.000")] public double? AvPPop, AvPTech, AvPWel = null;
            [XLColumn(Order = 40)][ColumnFormat("0.00%")] public double HiPPop, HiPTech, HiPWel = 0;

            [XLColumn(Order = 50)][ColumnFormat("0.000")] public double? AvRelPros = null;
            [XLColumn(Order = 51)][ColumnFormat("0.000")] public double? AvRelPop, AvRelTech, AvRelWel = null;
            [XLColumn(Order = 60)][ColumnFormat("0.000")] public double HiRelPros = 0;
            [XLColumn(Order = 61)][ColumnFormat("0.000")] public double HiRelPop, HiRelTech, HiRelWel = 0;

            [XLColumn(Order = 70)] public int Terr = 0;
            [XLColumn(Order = 71)][ColumnFormat("0.000")] public double? TerrAv = null;
            [XLColumn(Order = 80)] public int Invent = 0;
            [XLColumn(Order = 81)][ColumnFormat("0.000")] public double? InventAv = null;
            [XLColumn(Order = 90)] public int Trades = 0;
            [XLColumn(Order = 91)][ColumnFormat("0.000")] public double? TradeAv = null;

            private int upsetTotal = 0;
            private int primaryDownCount = 0;
            private int upToTopCount = 0;
            [XLColumn(Order = 100)][ColumnFormat("0.000")] public double? UpsetAv = null;
            [XLColumn(Order = 101)] public int PosUpset = 0;
            [XLColumn(Order = 102)] public int NegUpset = 0;
            [XLColumn(Order = 103)][ColumnFormat("0.00%")] public double? PosUpsetP = null;
            [XLColumn(Order = 104)][ColumnFormat("0.00%")] public double? NegUpsetP = null;
            [XLColumn(Order = 105)][ColumnFormat("0.00%")] public double? Over1stP = null;
            [XLColumn(Order = 106)][ColumnFormat("0.00%")] public double? PrDownP = null;

            [XLColumn(Order = 110)] public int Plants = 0;
            [XLColumn(Order = 111)] public int Animals = 0;
            [XLColumn(Order = 112)] public int Minerals = 0;
            private int activeBioticaCount = 0;
            private int totalActiveBioLevel = 0;
            [XLColumn(Order = 113)][ColumnFormat("0.000")] public double? AvFBioLv = null;
            [XLColumn(Order = 114)][ColumnFormat("0.00%")] public double? PPlant = null;
            [XLColumn(Order = 115)][ColumnFormat("0.00%")] public double? PAnimal = null;
            [XLColumn(Order = 116)][ColumnFormat("0.00%")] public double? PMineral = null;

            [XLColumn(Order = 117)] public int Apex = 0;
            [XLColumn(Order = 118)][ColumnFormat("0.00%")] public double? ApexP = null;

            private HashSet<string> bioUsed = [];
            [XLColumn(Order = 119)] public int UqPlant = 0;
            [XLColumn(Order = 120)] public int UqAnimal = 0;
            [XLColumn(Order = 121)] public int UqMineral = 0;
            [XLColumn(Order = 122)] public int UqApex = 0;

            // Counts/percents of wild biome patches, per planet
            private Dictionary<string, int> biomeUsageCounts = [];
            [XLColumn(Order = 130)]
            [UnpackToBiomes(defaultValue: (double)0, prefix: "Has", numberFormat: "0.00%")]
            public Dictionary<string, double?> biomeUsagePercents = [];

            // Counts of wild patches in territory
            [XLColumn(Order = 140)]
            [UnpackToBiomes(defaultValue: (int)0, suffix: "Sz")]
            public Dictionary<string, int> biomeSizes = [];

            [XLColumn(Order = 150)]
            [UnpackToBiomes(defaultValue: (double)0, suffix: "P", numberFormat: "0.00%")]
            public Dictionary<string, double?> biomeSizePercents = [];

            public SpiritStatEntry(string spiritName, Glossaries glosInstance)
            {
                this.Name = spiritName;
                this.InitializeBiomeCounters(glosInstance);
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

            public void IncrementUpsetTotal(int upset, bool isPrimary, bool toTop)
            {
                this.upsetTotal += upset;
                if (isPrimary && upset < 0) { this.primaryDownCount++; }
                if (toTop && upset > 0) { this.upToTopCount++; }
            }

            //public void IncrementBioticaPercentTotals(double PPlant, double PAnimal, double PMineral, double apexP)
            /*public void IncrementBioticaPercentTotals(double apexP)
            {
                //this.plantPercTotal += PPlant;
                //this.animalPercTotal += PAnimal;
                //this.mineralPercTotal += PMineral;
                //this.apexPercTotal += apexP;
            }*/

            public void AddBioUsed(IEnumerable<string> bioInCity)
            {
                this.bioUsed.UnionWith(bioInCity);
            }

            public void IncrementBioticaLevelTotal(int level)
            {
                this.totalActiveBioLevel += level;
                this.activeBioticaCount += 1;
            }

            public void IncrementBioticaLevelTotal(List<int> levels)
            {
                foreach (int level in levels)
                {
                    this.IncrementBioticaLevelTotal(level);
                }
            }

            public void InitializeBiomeCounters(Glossaries g)
            {
                foreach (string bn in g.BiomeHashByName.Keys)
                {
                    this.biomeUsageCounts[bn] = 0;
                    this.biomeSizes[bn] = 0;
                }
            }

            public void IncrementBiomeUsage(Dictionary<string, int> BiomePatchCounts)
            {
                foreach (string bn in BiomePatchCounts.Keys)
                {
                    int patches = BiomePatchCounts[bn];
                    this.Terr += patches;
                    if (this.biomeUsageCounts.ContainsKey(bn))
                    {
                        this.biomeUsageCounts[bn] += 1;
                        this.biomeSizes[bn] += patches;
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

            public void IncrementPlanetProsAverageAsPrimary(double avpros)
            {
                this.totalPlanetCityProsAverage += avpros;
            }

            public void CalculateStats(int planetCount, Glossaries glossaryInstance)
            {
                this.P = SafePercent(this.Count, planetCount);
                this.PrimeP = SafePercent(this.Prime, this.Count);
                this.MainP = SafePercent(this.Prime, planetCount);

                this.AvScore = SafeDivide(this.totalPlanetScore, this.Count);
                this.AvPrTScore = SafeDivide(this.totalPlanetScorePrimary, this.Prime);
                this.AvPrAPros = SafeDivide(this.totalPlanetCityProsAverage, this.Prime);

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
                this.PrDownP = SafeDivide(this.primaryDownCount, this.Count);
                this.Over1stP = SafeDivide(this.upToTopCount, this.Count);

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

                this.biomeUsagePercents = this.biomeUsageCounts.ToDictionary(kv => kv.Key, kv => SafePercent(kv.Value, this.Count));
                this.biomeSizePercents = this.biomeSizes.ToDictionary(kv => kv.Key, kv => SafePercent(kv.Value, this.Terr));

                foreach (string bdic in bioUsed)
                {
                    BioticumDefinition cityBioDef = glossaryInstance.BioticumDefFromHash(bdic);
                    if (cityBioDef is null) continue;
                    switch (cityBioDef.Type)
                    {
                        case "Plant":
                            this.UqPlant += 1;
                            break;
                        case "Animal":
                            this.UqAnimal += 1;
                            break;
                        case "Mineral":
                            this.UqMineral += 1;
                            break;
                    }
                    if (cityBioDef.Apex) this.UqApex += 1;
                }
            }
        }

        public class LuxuryStatEntry
        {
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public readonly string Type;

            [XLColumn(Order = 10)] public int Count = 0;
            [XLColumn(Order = 11)] public int Planets = 0;
            [XLColumn(Order = 12)][ColumnFormat("0.00%")] public double PlanetP = 0;

            [XLColumn(Order = 20)] public readonly string Hash;
            [XLColumn(Order = 21)] public string FavSpirit;
            [XLColumn(Order = 22)][ColumnFormat("0.000")] public double FavRatio;
            [XLColumn(Order = 23, Header = "HiBioSrc")] public string BioticaModalSource;
            [XLColumn(Ignore = true)] public Dictionary<string,int> BioticaSourceCounts = [];

            [XLColumn(Order = 30)]
            [UnpackToSpirits(defaultValue: (int)0, prefix: "From")]
            public Dictionary<string, int> LeaderCountsOri = [];

            [XLColumn(Order = 31)]
            [UnpackToSpirits(defaultValue: (double)0, suffix: "Ra", numberFormat: "0.0000")]
            public Dictionary<string, double> LeaderRatios = [];

            [XLColumn(Order = 32)]
            [UnpackToSpirits(defaultValue: (int)0)]
            public Dictionary<string, int> LeaderCounts = [];

            public LuxuryStatEntry(Glossaries.LuxuryDefinition luxDef, Glossaries gloss)
            {
                this.Name = luxDef.Name;
                this.Type = luxDef.Type;
                this.Hash = luxDef.Hash;
                this.InitializeLeaderSubtables(gloss);
            }

            public void InitializeLeaderSubtables(Glossaries gloss)
            {
                foreach (string leaderName in gloss.SpiritHashByName.Keys)
                {
                    LeaderCounts[leaderName] = 0;
                    LeaderCountsOri[leaderName] = 0;
                }
            }

            public void CalculateStats(int planetCount)
            {
                this.PlanetP = (double)SafePercent(this.Planets, planetCount);
                if (this.BioticaSourceCounts.Count > 0) this.BioticaModalSource = this.BioticaSourceCounts.OrderBy(kv => -kv.Value).First().Key;
            }
        }

        public class EraStatEntry
        {
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public readonly int Era;
            [XLColumn(Order = 2)] public readonly string Hash;

            [XLColumn(Order = 10)] public int Count = 0;
            [XLColumn(Order = 11)][ColumnFormat("0.00%")] public double? PickP;

            [XLColumn(Ignore = true)] public List<int> eraScores = [];
            [XLColumn(Order = 20)][ColumnFormat("0.000")] public double? AvScore;
            [XLColumn(Order = 21)] public int? HiScore;

            [XLColumn(Order = 30, Header = "0Star")] public int Star0 = 0;
            [XLColumn(Order = 31, Header = "1Star")] public int Star1 = 0;
            [XLColumn(Order = 32, Header = "2Star")] public int Star2 = 0;
            [XLColumn(Order = 33, Header = "3Star")] public int Star3 = 0;

            [XLColumn(Order = 40, Header = "0StarP")][ColumnFormat("0.00%")] public double? Star0P;
            [XLColumn(Order = 41, Header = "1StarP")][ColumnFormat("0.00%")] public double? Star1P;
            [XLColumn(Order = 42, Header = "2StarP")][ColumnFormat("0.00%")] public double? Star2P;
            [XLColumn(Order = 43, Header = "3StarP")][ColumnFormat("0.00%")] public double? Star3P;

            public EraStatEntry(Glossaries.EraDefinition eraDef)
            {
                this.Name = eraDef.Name;
                this.Era = eraDef.Era;
                this.Hash = eraDef.Hash;
            }

            public void CalculateStats(int stageCount)
            {
                this.PickP = SafePercent(this.Count, stageCount);
                if (this.eraScores.Count > 0)
                {
                    this.AvScore = this.eraScores.Average();
                    this.HiScore = this.eraScores.Max();
                }

                this.Star0P = SafePercent(this.Star0, this.Count);
                this.Star1P = SafePercent(this.Star1, this.Count);
                this.Star2P = SafePercent(this.Star2, this.Count);
                this.Star3P = SafePercent(this.Star3, this.Count);
            }
        }

        public class ProjectStatEntry
        {
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public readonly string Slot;
            [XLColumn(Order = 2)] public readonly string Hash;
            [XLColumn(Order = 3)] public readonly string InName;

            [XLColumn(Order = 10)] public int Count = 0;
            [XLColumn(Order = 11)][ColumnFormat("0.00%")] public double? SlotP;
            [XLColumn(Order = 12)][ColumnFormat("0.00%")] public double? ASlotP;

            [XLColumn(Order = 20)]
            [UnpackToSpirits(defaultValue: (int)0)]
            public Dictionary<string, int> LeaderCounts = [];

            [XLColumn(Order = 30)]
            [UnpackToSpirits(defaultValue: (double)0, prefix: "P", numberFormat: "0.00%", nullOnZeroOrBlank: true)]
            public Dictionary<string, double?> LeaderPickRates = [];

            public ProjectStatEntry(Glossaries.CityProjectDefinition projectDef, Glossaries gloss)
            {
                this.Name = projectDef.DisplayName;
                this.Slot = projectDef.Slot;
                this.Hash = projectDef.Hash;
                this.InName = projectDef.InternalName;
                this.InitializeLeaderSubtables(gloss);
            }

            public void InitializeLeaderSubtables(Glossaries gloss)
            {
                foreach (string leaderName in gloss.SpiritHashByName.Keys)
                {
                    LeaderCounts[leaderName] = 0;
                    LeaderPickRates[leaderName] = 0;
                }
            }

            public void CalculateStats(Dictionary<string, int> slotCounts, Dictionary<(string, string), int> slotCountsByLeader)
            {
                if (slotCounts.TryGetValue(this.Slot, out int slotUses))
                {
                    this.SlotP = SafePercent(this.Count, slotUses);
                }
                else this.SlotP = null;

                int leaderSlotUseTotal = 0;
                foreach (string leader in this.LeaderPickRates.Keys)
                {
                    if (slotCountsByLeader.TryGetValue((leader, this.Slot), out int leaderSlotUse))
                    {
                        if (this.LeaderCounts[leader] > 0)
                        {
                            leaderSlotUseTotal += leaderSlotUse;
                            this.LeaderPickRates[leader] = SafePercent(this.LeaderCounts[leader], leaderSlotUse);
                        }
                        else
                        {
                            this.LeaderPickRates[leader] = 0;
                        }
                    }
                    else
                    {
                        this.LeaderPickRates[leader] = 0;
                    }
                }
                this.ASlotP = SafePercent(Count, leaderSlotUseTotal);
            }

            public void IncrementCounts(string spiritName)
            {
                this.Count += 1;
                this.LeaderCounts[spiritName] += 1;
            }
        }
    }
}
