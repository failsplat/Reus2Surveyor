using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Features;

namespace Reus2Surveyor
{
    public class StatCollector
    {
        private Glossaries glossaryInstance;
        public Dictionary<string, BioticumStatEntry> BioticaStats { get; private set; } = [];

        public StatCollector(Glossaries g)
        {
            this.glossaryInstance = g;
        }

        public void ConsumePlanet(Planet planet)
        {
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
                    if (glossaryInstance.BioticumDefFromHash(draftDef) is null) BioticaStats[draftDef] = new BioticumStatEntry(draftDef);
                    else BioticaStats[draftDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(draftDef));
                    
                }
                BioticaStats[draftDef].Draft += 1;
            }

            // Make entries for active then archived then complete
            foreach (string activeDef in activeBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(activeDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(activeDef) is null) BioticaStats[activeDef] = new BioticumStatEntry(activeDef);
                    else BioticaStats[activeDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(activeDef));

                }
                BioticaStats[activeDef].Final += activeBioCounter[activeDef];
            }
            foreach (string legacyDef in legacyBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(legacyDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(legacyDef) is null) BioticaStats[legacyDef] = new BioticumStatEntry(legacyDef);
                    else BioticaStats[legacyDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(legacyDef));

                }
                BioticaStats[legacyDef].Legacy += legacyBioCounter[legacyDef];
            }

            foreach (string cDef in completeBioCounter.Keys)
            {
                if (!BioticaStats.ContainsKey(cDef))
                {
                    if (glossaryInstance.BioticumDefFromHash(cDef) is null) BioticaStats[cDef] = new BioticumStatEntry(cDef);
                    else BioticaStats[cDef] = new BioticumStatEntry(this.glossaryInstance.BioticumDefFromHash(cDef));

                }
                BioticaStats[cDef].Planets += 1;
                BioticaStats[cDef].Total += completeBioCounter[cDef];
                if (completeBioCounter[cDef] > 1) BioticaStats[cDef].MultiNumberList.Add(completeBioCounter[cDef]);
            }
        }

        public void FinalizeStats() 
        {
            foreach (BioticumStatEntry bse in this.BioticaStats.Values)
            {
                bse.CalculateStats();
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

        public class BioticumStatEntry
        {
            [property:XLColumn(Ignore = true)] public readonly BioticumDefinition Definition;
            public readonly string Name;
            public readonly string Type;
            public readonly int? Tier;
            public readonly string Apex;

            public readonly string Desert, Forest, IceAge, Ocean, Rainforest, Savanna, Taiga;

            public int Draft { get; set; } = 0;
            public int Planets { get; set; } = 0;
            public double? DraftP { get; private set; } = null;

            public int Total { get; set; } = 0;
            public int Legacy { get; set; } = 0;
            public double? LegacyP { get; private set; } = null;
            public int Final { get; set; } = 0;
            public double? FinalP { get; private set; } = null;

            public List<int> MultiNumberList { get; private set; } = [];
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


            public readonly string Hash;

            public BioticumStatEntry(BioticumDefinition bioDef)
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
            }

            public BioticumStatEntry(string hash)
            {
                this.Definition = null;
                this.Name = "?";
                this.Type = "?";
                this.Tier = null;
                this.Apex = "?";

                this.Desert = null;
                this.Forest = null;
                this.IceAge = null;
                this.Ocean = null;
                this.Savanna = null;
                this.Taiga = null;
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

            public void CalculateStats()
            {
                this.DraftP = SafeDivide(this.Planets, this.Draft);
                this.LegacyP = SafeDivide(this.Legacy, this.Total);
                this.FinalP = SafeDivide(this.Final, this.Total);

                if (this.MultiNumberList.Count > 0)
                {
                    this.Multi = this.MultiNumberList.Count;
                    this.MultiAvg = this.MultiNumberList.Average();
                    this.MultiMax = this.MultiNumberList.Max();
                    this.MultiP = SafeDivide((int)this.Multi, this.Planets);
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

            private static double? SafeDivide(int a0, int b0)
            {
                if (b0 == 0) return null;
                double a = (double)a0;
                double b = (double)b0;
                return a / b;
            }
        }
    }
}
