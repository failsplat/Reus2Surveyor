using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Features;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Reus2Surveyor
{
    public class StatCollector
    {
        private Glossaries glossaryInstance;
        public Dictionary<string, BioticumStatEntry> BioticaStats { get; private set; } = [];
        private int planetCount = 0;

        public StatCollector(Glossaries g)
        {
            this.glossaryInstance = g;
        }

        public void ConsumePlanet(Planet planet, int index)
        {
            if (planet is null) return;

            // Count all biotica that are available in available biomes

            Dictionary<string, int> activeBioCounter = [];
            Dictionary<string, int> legacyBioCounter = [];
            //Dictionary<(string,string), int> bioPropertyDict = [];

            foreach (KeyValuePair<string,int> legKv in planet.LegacyBioticaCounterDefs)
            {
                IncrementCounter(legacyBioCounter, legKv.Key, legKv.Value);
            }
            foreach (NatureBioticum nb in planet.natureBioticumDictionary.Values)
            {
                IncrementCounter(activeBioCounter, nb.definition, 1);
            }

            Dictionary<string, int> completeBioCounter = [];
            foreach (KeyValuePair<string,int> kv in activeBioCounter)
            {
                IncrementCounter(completeBioCounter, kv.Key, kv.Value);
            }
            foreach (KeyValuePair<string,int> kv in legacyBioCounter)
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
                BioticaStats[cDef].Total += completeBioCounter[cDef];
                if (completeBioCounter[cDef] > 1) BioticaStats[cDef].AddMultiValue(completeBioCounter[cDef]);
            }

            this.planetCount++;
        }

        public void FinalizeStats() 
        {
            foreach (BioticumStatEntry bse in this.BioticaStats.Values)
            {
                bse.CalculateStats(this.planetCount);
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
                var bioWs = wb.AddWorksheet("Biotica");
                bioWs.Cell("A1").InsertTable(this.BioticaStats.Values, "Biotica");
                wb.SaveAs(dstPath);
            }

                
        }

        public class BioticumStatEntry
        {
            private readonly BioticumDefinition Definition;
            public readonly string Name;
            public readonly string Type;
            public readonly int? Tier;
            public readonly string Apex;

            public readonly string Desert, Forest, IceAge, Ocean, Rainforest, Savanna, Taiga;

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
            public int? MultiMax { get; private set; } = null;
            public double? MultiAvg { get; private set; } = null;



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

            public BioticumStatEntry(BioticumDefinition bioDef, string p1name)
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
                        field.SetValue(this, (int)(field.GetValue(this))+ 1);
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
                this.UsageP = SafePercent(this.Planets, this.Draft);
                this.LegacyP = SafePercent(this.Legacy, this.Total);
                this.FinalP = SafePercent(this.Final, this.Total);
                this.DraftP = SafePercent(this.Draft, planetCount);

                if (this.MultiNumberList.Count > 0)
                {
                    this.Multi = this.MultiNumberList.Count;
                    this.MultiAvg = this.MultiNumberList.Average();
                    this.MultiMax = this.MultiNumberList.Max();
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
        }
    }
}
