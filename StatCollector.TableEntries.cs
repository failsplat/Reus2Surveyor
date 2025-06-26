using ClosedXML.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            [XLColumn(Order = 10), UnpackToBiomes(defaultValue: "")] public readonly Dictionary<string, string> biomesAllowed = [];

            [XLColumn(Order = 20)] public readonly string Hash;
            [XLColumn(Order = 21)] public int Total { get; set; } = 0;
            [XLColumn(Order = 22)] public int Planets { get; set; } = 0;
            [XLColumn(Order = 23)] public double? DUsageP { get; private set; } = null;
            [XLColumn(Order = 24)] public double? AUsageP { get; private set; } = null;
            [XLColumn(Order = 25)] public int Draft { get; set; } = 0;
            [XLColumn(Order = 26)] public double? DraftP { get; set; } = null;
            [XLColumn(Order = 27)] public int Avail { get; set; } = 0;
            [XLColumn(Order = 27)] public double? AvRate { get; set; } = null;
            [XLColumn(Order = 28)] public double? AvailP { get; set; } = null;

            [XLColumn(Order = 30)] public int Legacy { get; set; } = 0;
            [XLColumn(Order = 31)] public double? LegacyP { get; private set; } = null;
            [XLColumn(Order = 32)] public int Final { get; set; } = 0;
            [XLColumn(Order = 33)] public double? FinalP { get; private set; } = null;

            private List<int> MultiNumberList = [];
            [XLColumn(Order = 40)] public int? Multi { get; set; } = null;
            [XLColumn(Order = 41)] public double? MultiP { get; private set; } = null;
            [XLColumn(Order = 42)] public int? MultiMx { get; private set; } = null;
            [XLColumn(Order = 43)] public double? MultiAv { get; private set; } = null;

            [XLColumn(Order = 50)] public int P1st { get; set; }
            [XLColumn(Order = 51)] public int PLast { get; set; }


            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> { "AvailP", "DraftP", "AUsageP", "DUsageP", "LegacyP", "FinalP", "MultiP", } },
                {"0.000", new List<string> { "MultiAv", "AvRate", } },
                {"mm/dd/yyyy hh:mm", new List<string>{"TS", } },
                {"mm/dd/yyyy", new List<string>{"ChTS", } },
                };

            public BioticumStatEntry(Glossaries.BioticumDefinition bioDef, int p1)
            {
                this.Definition = bioDef;
                this.Name = bioDef.Name;
                this.Type = bioDef.Type;
                this.Tier = bioDef.Tier;
                this.Starter = bioDef.Starter ? "Y" : null;
                this.Apex = bioDef.Apex ? "☆" : null;

                foreach((string biomeName, bool allowed) in bioDef.BiomesAllowed)
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

            [XLColumn(Order = 20)] public string Char1, Char2, Char3, Char4, Char5;

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

            [XLColumn(Order = 112)] public int SzT;
            [XLColumn(Order = 113)] public int SzWld;
            [XLColumn(Order = 114)] public int FilledSlots = 0;
            [XLColumn(Order = 115)] public double? FillP;

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

            [XLColumn(Order = 150)] public int Creeks = 0;
            [XLColumn(Order = 150)] public int InvasiveSpots = 0;
            [XLColumn(Order = 150)] public int Anomalies = 0;
            [XLColumn(Order = 150)] public int Sanctuaries = 0;
            [XLColumn(Order = 150)] public int MountainSlots = 0;

            [XLColumn(Order = 160), UnpackToBiomes(defaultValue: (double)0, suffix: "P", numberFormat: "0.00%")] 
            public Dictionary<string, double> biomePercents = [];

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> {
                    "PPop", "PTech", "PWel", "PPlant", "PAnimal", "PMineral", "ApexP", "FillP",
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

            public static void AddColumnFormat(string format, string column)
            {
                if (columnFormats.TryGetValue(format, out List<string> columns)) columns.Add(column);
                else columnFormats[format] = [column];
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
            private Dictionary<string, int> biomeUsageCounts = [];
            [XLColumn(Order = 120)]
            [UnpackToBiomes(defaultValue: (double)0, prefix: "Has", numberFormat: "0.00%")] 
            public Dictionary<string, double?> biomeUsagePercents = [];

            // Counts of wild patches in territory
            [XLColumn(Order = 130)]
            [UnpackToBiomes(defaultValue: (int)0, suffix: "Sz")]
            public Dictionary<string,int> biomeSizes = [];

            [XLColumn(Order = 140)]
            [UnpackToBiomes(defaultValue: (double)0, suffix: "P", numberFormat: "0.00%")]
            public Dictionary<string,double?> biomeSizePercents = [];

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> {
                    "P", "PrimeP", "MainP",
                    "AvPPop", "AvPTech", "AvPWel", "HiPPop", "HiPTech", "HiPWel",
                    "PosUpsetP", "NegUpsetP",
                    "PPlant", "PAnimal", "PMineral", "ApexP",
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

            public static void AddColumnFormat(string format, string column)
            {
                if (columnFormats.TryGetValue(format, out List<string> columns)) columns.Add(column);
                else columnFormats[format] = [column];
            }

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
                foreach (int level in levels)
                {
                    this.IncrementBioticaLevelTotal(level);
                }
            }

            public void InitializeBiomeCounters(Glossaries g)
            {
                foreach(string bn in g.BiomeHashByName.Keys)
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
                    this.biomeUsageCounts[bn] += 1;
                    this.biomeSizes[bn] += patches;
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

                this.biomeUsagePercents = this.biomeUsageCounts.ToDictionary(kv => kv.Key, kv => SafePercent(kv.Value, this.Count));
                this.biomeSizePercents = this.biomeSizes.ToDictionary(kv => kv.Key, kv => SafePercent(kv.Value, this.Terr));
            }
        }

        public class LuxuryStatEntry
        {
            [XLColumn(Order = 0)] public readonly string Name;
            [XLColumn(Order = 1)] public readonly string Type;

            [XLColumn(Order = 10)] public int Count = 0;
            [XLColumn(Order = 11)] public int Planets = 0;
            [XLColumn(Order = 12)] public double PlanetP = 0;

            [XLColumn(Order = 20)] public readonly string Hash;

            [XLColumn(Order = 30)]
            [UnpackToSpirits(defaultValue:(int)0)]
            public Dictionary<string, int> LeaderCounts = [];

            [XLColumn(Order = 40)]
            [UnpackToSpirits(defaultValue: (double)0, suffix: "Ra", numberFormat: "0.0000")]
            public Dictionary<string, double> LeaderRatios = [];

            [XLColumn(Order = 50)]
            [UnpackToSpirits(defaultValue: (int)0, prefix: "From")]
            public Dictionary<string, int> LeaderCountsOri = [];

            private static Dictionary<string, List<string>> columnFormats = new() {
                {"0.00%", new List<string> {
                    "PlanetP",
                } },
                {"0.000", new List<string> {

                } },
                };

            public LuxuryStatEntry(Glossaries.LuxuryDefinition luxDef, Glossaries gloss)
            {
                this.Name = luxDef.Name;
                this.Type = luxDef.Type;
                this.Hash = luxDef.Hash;
                this.InitializeLeaderSubtables(gloss);
            }

            public void InitializeLeaderSubtables(Glossaries gloss)
            {
                foreach(string leaderName in gloss.SpiritHashByName.Keys)
                {
                    LeaderCounts[leaderName] = 0;
                    LeaderCountsOri[leaderName] = 0;
                }
            }

            public void CalculateStats(int planetCount)
            {
                this.PlanetP = (double)SafePercent(this.Planets, planetCount);
            }

            public static Dictionary<string, List<string>> GetColumnFormats()
            {
                return columnFormats;
            }

            public static void AddColumnFormat(string format, string column)
            {
                if (columnFormats.TryGetValue(format, out List<string> columns)) columns.Add(column);
                else columnFormats[format] = [column];
            }
        }
    }
}
