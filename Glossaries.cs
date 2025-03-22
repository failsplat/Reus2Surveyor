using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor
{
    public class Glossaries
    {
        public readonly Dictionary<string, string> SpiritHashByName = [], SpiritNameByHash = [];
        public readonly Dictionary<string, string> BiomeHashByName = [], BiomeNameByHash = [];
        public readonly Dictionary<string, string> LuxuryHashByName = [], LuxuryNameByHash = [];
        public readonly Dictionary<string, string> MicroHashByName = [], MicroNameByHash = [];
        public readonly Dictionary<string, string> EraHashByName = [], EraNameByHash = [];
        public readonly Dictionary<string, string> YieldHashByName = [], YieldNameByHash = [];

        public readonly Dictionary<string, BioticumDefinition> BioticumDefinitionByHash = [];
        public readonly List<BioticumDefinition> BioticumDefinitionList = [];

        public readonly Dictionary<string, GiantDefinition> GiantDefinitionByHash = [];
        public readonly List<GiantDefinition> GiantDefinitionList = [];

        public Glossaries(
            string nbFile, 
            string bioFile, 
            string giantFile, 
            string spiritFile,
            string eraFile
            )
        {
            // NonBiotica 
            /*using (StreamReader nbsr = new StreamReader(nbFile))
            {
                string currentLine;
                string headerLine = nbsr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = nbsr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    string type = data[header.IndexOf("Type")];
                    string name = data[header.IndexOf("Name")];
                    string hash = data[header.IndexOf("Hash")];

                    if (hash.Length != 32) continue;
                    else
                    {
                        switch (type)
                        {
                            case "Character":
                            case "Spirit":
                                this.SpiritHashByName.Add(name, hash);
                                this.SpiritNameByHash.Add(hash, name);
                                break;
                            case "Biome":
                                this.BiomeHashByName.Add(name, hash);
                                this.BiomeNameByHash.Add(hash, name);
                                break;
                            case "Luxury":
                                this.LuxuryHashByName.Add(name, hash);
                                this.LuxuryNameByHash.Add(hash, name);
                                break;
                            case "Micro":
                            case "Aspect":
                            case "Emblem":
                                this.MicroHashByName.Add(name, hash);
                                this.MicroNameByHash.Add(hash, name);
                                break;
                            case "Era":
                            case "TurningPoint":
                            case "Turning Point":
                            case "Age":
                                this.TurningPointHashByName.Add(name, hash);
                                this.TurningPointNameByHash.Add(hash, name);
                                break;
                            case "Yield":
                                this.YieldHashByName.Add(name, hash);
                                this.YieldNameByHash.Add(hash, name);
                                break;
                        }
                    }
                }
            }*/

            using (StreamReader bsr = new StreamReader(bioFile))
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
            foreach (BioticumDefinition bd in this.BioticumDefinitionList)
            {
                if (bd.Hash is null || bd.Hash.Length == 0) continue;
                else this.BioticumDefinitionByHash.Add(bd.Hash, bd);
            }

            using (StreamReader gsr = new StreamReader(giantFile))
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
            foreach (GiantDefinition gd in this.GiantDefinitionList)
            {
                if (gd.Hash is null || gd.Hash.Length == 0) continue;
                else this.GiantDefinitionByHash.Add(gd.Hash, gd);
            }

            using (StreamReader ssr = new StreamReader(spiritFile))
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

                    this.SpiritHashByName[name] = hash;
                    this.SpiritNameByHash[hash] = name;
                }
            }

            using (StreamReader esr = new StreamReader(eraFile))
            {
                string currentLine;
                string headerLine = esr.ReadLine().Trim();
                List<string> header = [.. headerLine.Split(",")];
                while ((currentLine = esr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    List<string> data = [.. currentLine.Split(",")];
                    string name = data[header.IndexOf("Name")];
                    string hash = data[header.IndexOf("Hash")];
                    if (hash is null || hash.Length == 0)
                    {
                        continue;
                    }

                    this.EraHashByName[name] = hash;
                    this.EraNameByHash[hash] = name;
                }
            }
        }

        public Glossaries(string folderPath)
            : this(
                  nbFile: Path.Combine(folderPath, "NonBiotica.csv"),
                  bioFile: Path.Combine(folderPath, "Biotica.csv"),
                  giantFile: Path.Combine(folderPath, "Giants.csv"),
                  spiritFile: Path.Combine(folderPath, "Spirits.csv"),
                  eraFile: Path.Combine(folderPath, "Eras.csv")
                  )
        {
        }

        public static readonly Dictionary<int, string> BiomeNameByInt = new()
        {
            {0, "Forest" },
            {1, "Rainforest" },
            {2, "Taiga" },
            {4, "Savanna" },
            {5, "Desert" },
            {6, "Ocean" },
            {8, "Ice Age" },
        };
        public static readonly Dictionary<string, int> BiomeIntByName = BiomeNameByInt.ToDictionary(x => x.Value, x => x.Key);

        public string BiomeNameFromDef(string def)
        {
            if (BiomeNameByHash.ContainsKey(def))
            {
                return BiomeNameByHash[def];
            }
            else
            {
                return def;
            }
        }

        public string BioticumNameFromHash(string def)
        {
            if (BioticumDefinitionByHash.ContainsKey(def))
            {
                return BioticumDefinitionByHash[def].Name;
            }
            else return def;
        }

        public BioticumDefinition BioticumDefFromHash(string def)
        {
            if (BioticumDefinitionByHash.ContainsKey(def))
            {
                return BioticumDefinitionByHash[def];
            }
            else return null;
        }

        public string SpiritNameFromHash(string hash)
        {
            if (this.SpiritNameByHash.ContainsKey(hash)) return this.SpiritNameByHash[hash];
            else return hash;
        }

        public string EraNameFromHash(string hash)
        {
            if (this.EraNameByHash.ContainsKey(hash)) return this.EraNameByHash[hash];
            else return hash;
        }

        public string GiantNameFromHash(string hash)
        {
            if (this.GiantDefinitionByHash.ContainsKey(hash)) return this.GiantDefinitionByHash[hash].Name;
            else return hash;
        }

        public GiantDefinition TryGiantDefinitionFromhash(string hash)
        {
            if (this.GiantDefinitionByHash.ContainsKey(hash)) return this.GiantDefinitionByHash[hash];
            else return new(hash);
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
                        case string s when s.StartsWith("Biome:"):
                            string biomeName = s.Split(":").Last();
                            this.BiomesAllowed[biomeName] = InterpretEntryBool(d);
                            break;
                    }

                }
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
    }
}
