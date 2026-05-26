using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Reus2Surveyor
{
    public static class Glossaries
    {
        public static readonly Dictionary<string, string> SpiritHashByName = [], SpiritNameByHash = [];
        public static readonly Dictionary<string, string> BiomeHashByName = [], BiomeNameByHash = [];
        //public readonly Dictionary<string, string> LuxuryHashByName = [], LuxuryNameByHash = [];
        //public readonly Dictionary<string, string> MicroHashByName = [], MicroNameByHash = [];

        public static readonly Dictionary<string, string> YieldHashByName = [], YieldNameByHash = [];

        public static readonly Dictionary<string, BioticumDefinition> BioticumDefinitionByHash = [];
        public static readonly Dictionary<string, BioticumDefinition> BioticumDefinitionByName = [];
        public static readonly List<BioticumDefinition> BioticumDefinitionList = [];

        public static readonly Dictionary<string, GiantDefinition> GiantDefinitionByHash = [];
        public static readonly List<GiantDefinition> GiantDefinitionList = [];

        public static readonly Dictionary<string, CityProjectDefinition> ProjectDefinitionByHash = [];
        public static readonly List<CityProjectDefinition> ProjectDefinitionList = [];

        public static readonly Dictionary<string, EraDefinition> EraDefinitionByHash = [];
        public static readonly List<EraDefinition> EraDefinitionList = [];

        public static readonly Dictionary<string, LuxuryDefinition> LuxuryDefinitionsByHash = [];
        public static readonly List<LuxuryDefinition> LuxuryDefinitionList = [];

        public static readonly Dictionary<string, string> BiomeColors = [];

        public static readonly Dictionary<string, MicroDefinition> MicroDefinitionsByHash = [];
        public static readonly List<MicroDefinition> MicroDefinitionList = [];

        public static readonly string BioFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Biotica.csv");
        public static readonly string GiantFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Giants.csv");
        public static readonly string SpiritFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Spirits.csv");
        public static readonly string EraFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Eras.csv");
        public static readonly string ProjectFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Projects.csv");
        public static readonly string BiomeFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Biomes.csv");
        public static readonly string LuxuryFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Luxuries.csv");
        public static readonly string MicroFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Glossaries", "Micros.csv");

        public static readonly Dictionary<int, string> BiomeNameByInt = [];
        public static readonly Dictionary<string, int> BiomeIntByName = [];

        static Glossaries ()
        {
            using (StreamReader bsr = new StreamReader(BioFile))
            {
                string currentLine;
                string headerLine = bsr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = bsr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    BioticumDefinitionList.Add(new(header, data));
                }
            }
            foreach (BioticumDefinition bd in BioticumDefinitionList)
            {
                if (bd.Hash is null || bd.Hash.Length == 0) continue;
                else
                {
                    BioticumDefinitionByHash.Add(bd.Hash, bd);
                    BioticumDefinitionByName.Add(bd.Name, bd);
                }
            }

            using (StreamReader gsr = new StreamReader(GiantFile))
            {
                string currentLine;
                string headerLine = gsr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = gsr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    GiantDefinitionList.Add(new(header, data));
                }
            }
            foreach (GiantDefinition gd in GiantDefinitionList)
            {
                if (gd.Hash is null || gd.Hash.Length == 0) continue;
                else GiantDefinitionByHash.Add(gd.Hash, gd);
            }

            using (StreamReader ssr = new StreamReader(SpiritFile))
            {
                string currentLine;
                string headerLine = ssr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = ssr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    string name = data[header.IndexOf("Name")];
                    string hash = data[header.IndexOf("Hash")];
                    if (hash is null || hash.Length == 0)
                    {
                        continue;
                    }

                    SpiritHashByName[name] = hash;
                    SpiritNameByHash[hash] = name;
                }
            }

            using (StreamReader biomesr = new StreamReader(BiomeFile))
            {
                string currentLine;
                string headerLine = biomesr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = biomesr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    string name = data[header.IndexOf("Name")];
                    string hash = data[header.IndexOf("Hash")];
                    string num = data[header.IndexOf("Num")];
                    string color = data[header.IndexOf("Color")];
                    if (hash is null || hash.Length == 0)
                    {
                        continue;
                    }

                    BiomeHashByName[name] = hash;
                    BiomeNameByHash[hash] = name;
                    BiomeNameByInt[Int32.Parse(num)] = name;
                    BiomeIntByName[name] = Int32.Parse(num);
                    BiomeColors[name] = color;
                }
            }

            using (StreamReader esr = new StreamReader(EraFile))
            {
                string currentLine;
                string headerLine = esr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = esr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    EraDefinitionList.Add(new(header, data));
                }
            }
            foreach (EraDefinition ed in EraDefinitionList)
            {
                if (ed.Hash is null || ed.Hash.Length == 0) continue;
                else EraDefinitionByHash.Add(ed.Hash, ed);
            }

            using (StreamReader psr = new StreamReader(ProjectFile))
            {
                string currentLine;
                string headerLine = psr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = psr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    ProjectDefinitionList.Add(new(header, data));
                }
            }
            foreach (CityProjectDefinition pd in ProjectDefinitionList)
            {
                if (pd.Hash is null || pd.Hash.Length == 0) continue;
                else ProjectDefinitionByHash.Add(pd.Hash, pd);
            }

            using (StreamReader luxSr = new StreamReader(LuxuryFile))
            {
                string currentLine;
                string headerLine = luxSr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = luxSr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    LuxuryDefinitionList.Add(new(header, data));
                }
            }
            foreach (LuxuryDefinition pd in LuxuryDefinitionList)
            {
                if (pd.Hash is null || pd.Hash.Length == 0) continue;
                else LuxuryDefinitionsByHash.Add(pd.Hash, pd);
            }

            using (StreamReader microSr = new StreamReader(MicroFile))
            {
                string currentLine;
                string headerLine = microSr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = microSr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    MicroDefinitionList.Add(new(header, data));
                }
            }
            foreach (MicroDefinition md in MicroDefinitionList)
            {
                if (md.Hash is null || md.Hash.Length == 0) continue;
                else MicroDefinitionsByHash.Add(md.Hash, md);
            }
        }

        public static string GetBiomeNameFromInt(int id)
        {
            if (BiomeNameByInt.TryGetValue(id, out string name)) return name;
            else return $"UNKNOWN BIOME {id}";
        }

        public static string BiomeNameFromHash(string def)
        {
            if (BiomeNameByHash.TryGetValue(def, out string value))
            {
                return value;
            }
            else
            {
                return def;
            }
        }

        public static string BioticumNameFromHash(string def)
        {
            if (BioticumDefinitionByHash.TryGetValue(def, out BioticumDefinition value))
            {
                return value.Name;
            }
            else return def;
        }

        public static BioticumDefinition BioticumDefFromHash(string def)
        {
            if (BioticumDefinitionByHash.TryGetValue(def, out BioticumDefinition value))
            {
                return value;
            }
            else return null;
        }

        public static string SpiritNameFromHash(string hash)
        {
            if (SpiritNameByHash.TryGetValue(hash, out string value)) return value;
            else return hash;
        }

        public static string EraNameFromHash(string hash)
        {
            if (EraDefinitionByHash.TryGetValue(hash, out EraDefinition value)) return value.Name;
            else return hash;
        }

        public static EraDefinition TryEraDefinitionFromHash(string hash)
        {
            if (EraDefinitionByHash.TryGetValue(hash, out EraDefinition value)) return value;
            else return new(hash);
        }

        public static string GiantNameFromHash(string hash)
        {
            if (GiantDefinitionByHash.TryGetValue(hash, out GiantDefinition value)) return value.Name;
            else return hash;
        }

        public static GiantDefinition TryGiantDefinitionFromHash(string hash)
        {
            if (GiantDefinitionByHash.TryGetValue(hash, out GiantDefinition value)) return value;
            else return new(hash);
        }

        public static CityProjectDefinition TrProjectDefinitionFromHash(string hash)
        {
            if (ProjectDefinitionByHash.TryGetValue(hash, out CityProjectDefinition value)) return value;
            else return new(hash);
        }
        public static CityProjectDefinition TrProjectDefinitionFromHash(string hash, string name)
        {
            if (ProjectDefinitionByHash.TryGetValue(hash, out CityProjectDefinition value)) return value;
            else return new(hash, name);
        }

        public static LuxuryDefinition TryLuxuryDefinitionFromHash(string hash)
        {
            if (LuxuryDefinitionsByHash.TryGetValue(hash, out LuxuryDefinition value)) return value;
            else return new(hash);
        }

        public static string MicroNameFromHash(string hash)
        {
            if (MicroDefinitionsByHash.TryGetValue(hash, out MicroDefinition value)) return value.Name;
            else return hash;
        }

        public static bool InterpretEntryBool(string d)
        {
            return d switch
            {
                "1" or "y" or "Y" or "t" or "T" or "true" or "True" => true,
                _ => false,
            };
        }

        public class BioticumDefinition
        {
            public string Name { get; private set; }
            public string Type { get; private set; }

            public int Tier { get; private set; }
            public bool Apex { get; private set; }
            public bool Starter { get; private set; }

            public Dictionary<string, bool> BiomesAllowed { get; private set; } = [];

            public string Hash { get; private set; }

            public BioticumDefinition(List<string> header, List<string> data)
            {
                int i = -1;
                foreach (string d in data)
                {
                    i++;
                    string thisCol = header[i];
                    switch (thisCol)
                    {
                        case "Name":
                        case "Type":
                        case "Hash":
                            this.GetType().GetProperty(thisCol).SetValue(this, d);
                            break;
                        case "Tier":
                            this.Tier = Convert.ToInt32(d);
                            break;
                        case "Apex":
                            this.Apex = InterpretEntryBool(d);
                            break;
                        case "Starter":
                            this.Starter = InterpretEntryBool(d);
                            break;
                        case string s when s.StartsWith("Biome:"):
                            string biomeName = s.Split(":").Last();
                            this.BiomesAllowed[biomeName] = InterpretEntryBool(d);
                            break;
                    }

                }
            }

            public bool IsBiomeAllowed(string biomeName)
            {
                if (this.BiomesAllowed.TryGetValue(biomeName, out bool allowed))
                {
                    return allowed;
                }
                else return false;
            }
        }

        public class GiantDefinition
        {
            public string Name { get; private set; }
            public string Biome1 { get; private set; }
            public string Biome2 { get; private set; }
            public string Hash { get; private set; }
            public int Position { get; private set; }

            public GiantDefinition(List<string> header, List<string> data)
            {
                int i = -1;
                foreach (string d in data)
                {
                    i++;
                    string thisCol = header[i];
                    if (thisCol == "Position")
                    {
                        this.Position = System.Convert.ToInt32(d);
                        continue;
                    }
                    this.GetType().GetProperty(thisCol).SetValue(this, d);
                }
            }

            // Empty constructor
            // Use only when making blanks in StatCollector
            public GiantDefinition(string hash)
            {
                this.Hash = hash;
                this.Name = hash;
            }
        }

        public class EraDefinition
        {
            public readonly string Name, Hash;
            public readonly int Era;

            public EraDefinition(List<string> header, List<string> data)
            {
                int i = -1;
                foreach (string d in data)
                {
                    i++;
                    string thisCol = header[i];
                    if (thisCol == "Era")
                    {
                        this.Era = System.Convert.ToInt32(d);
                        continue;
                    }
                    this.GetType().GetField(thisCol).SetValue(this, d);
                }
            }

            public EraDefinition(string hash)
            {
                this.Hash = hash;
                this.Name = hash;
                this.Era = 0;
            }
        }

        public class CityProjectDefinition
        {
            public readonly string InternalName, DisplayName, Slot, Hash;

            public CityProjectDefinition(List<string> header, List<string> data)
            {
                int i = -1;
                foreach (string d in data)
                {
                    i++;
                    string thisCol = header[i];
                    this.GetType().GetField(thisCol).SetValue(this, d);
                }
            }

            // Empty constructor
            // Use only when making blanks in StatCollector
            public CityProjectDefinition(string hash)
            {
                this.Hash = hash;
                this.DisplayName = hash;
                this.InternalName = hash;
            }
            public CityProjectDefinition(string hash, string name)
            {
                this.Hash = hash;
                this.DisplayName = name;
                this.InternalName = name;
            }
        }

        public class LuxuryDefinition
        {
            public readonly string Name, Type, Hash;

            public LuxuryDefinition(List<string> header, List<string> data)
            {
                int i = -1;
                foreach (string d in data)
                {
                    i++;
                    string thisCol = header[i];
                    this.GetType().GetField(thisCol).SetValue(this, d);
                }
            }

            // Empty constructor
            // Use only when making blanks in StatCollector
            public LuxuryDefinition(string hash)
            {
                this.Hash = hash;
                this.Name = hash;
                this.Type = "?";
            }
        }

        public class MicroDefinition
        {
            public readonly string Name;
            public readonly string Hash;
            public readonly string Tier;
            public readonly string? Biome;
            public readonly int BaseCost = 0;
            public readonly double ScalingCost = 0;

            public MicroDefinition(List<string> header, List<string> data)
            {
                int i = -1;
                foreach (string d in data)
                {
                    i++;
                    string thisCol = header[i];

                    switch (thisCol) 
                    {
                        case "Biome":
                            this.Biome = d.Length > 0 ? d : null;
                            break;
                        case "BaseCost":
                            if (d.Length > 0) this.BaseCost = System.Convert.ToInt32(d);
                            break;
                        case "ScalingCost":
                            if (d.Length > 0) this.ScalingCost = System.Convert.ToDouble(d);
                            break;
                        default:
                            this.GetType().GetField(thisCol).SetValue(this, d);
                            break;
                    }
                }
            }
        }

        public static string GetBiomeColor(string biomeName)
        {
            if (BiomeColors.TryGetValue(biomeName, out string hex))
            {
                return hex;
            }
            else return "FF00FF";
        }

        public enum SpecialNaturalFeatures
        {
            Creek = 1,
            Sanctuary = 2,
            Anomaly = 3,
        }
    }
}
