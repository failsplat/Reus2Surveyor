﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace Reus2Surveyor
{
    public class Glossaries
    {
        public readonly Dictionary<string, string> SpiritHashByName = [], SpiritNameByHash = [];
        public readonly Dictionary<string, string> BiomeHashByName = [], BiomeNameByHash = [];
        //public readonly Dictionary<string, string> LuxuryHashByName = [], LuxuryNameByHash = [];
        //public readonly Dictionary<string, string> MicroHashByName = [], MicroNameByHash = [];

        public readonly Dictionary<string, string> YieldHashByName = [], YieldNameByHash = [];

        public readonly Dictionary<string, BioticumDefinition> BioticumDefinitionByHash = [];
        public readonly List<BioticumDefinition> BioticumDefinitionList = [];

        public readonly Dictionary<string, GiantDefinition> GiantDefinitionByHash = [];
        public readonly List<GiantDefinition> GiantDefinitionList = [];

        public readonly Dictionary<string, CityProjectDefinition> ProjectDefinitionByHash = [];
        public readonly List<CityProjectDefinition> ProjectDefinitionList = [];

        public readonly Dictionary<string, EraDefinition> EraDefinitionByHash = [];
        public readonly List<EraDefinition> EraDefinitionList = [];

        public Glossaries(
            string bioFile, 
            string giantFile, 
            string spiritFile,
            string eraFile,
            string projectFile,
            string biomeFile
            )
        {

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

            using (StreamReader biomesr = new StreamReader(biomeFile))
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
                    if (hash is null || hash.Length == 0)
                    {
                        continue;
                    }

                    this.BiomeHashByName[name] = hash;
                    this.BiomeNameByHash[hash] = name;
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
                    EraDefinitionList.Add(new(header, data));
                }
            }
            foreach (EraDefinition ed in this.EraDefinitionList)
            {
                if (ed.Hash is null || ed.Hash.Length == 0) continue;
                else this.EraDefinitionByHash.Add(ed.Hash, ed);
            }

            using (StreamReader psr = new StreamReader(projectFile))
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
            foreach (CityProjectDefinition pd in this.ProjectDefinitionList)
            {
                if (pd.Hash is null || pd.Hash.Length == 0) continue;
                else this.ProjectDefinitionByHash.Add(pd.Hash, pd);
            }
        }

        public Glossaries(string folderPath)
            : this(
                  bioFile: Path.Combine(folderPath, "Biotica.csv"),
                  giantFile: Path.Combine(folderPath, "Giants.csv"),
                  spiritFile: Path.Combine(folderPath, "Spirits.csv"),
                  eraFile: Path.Combine(folderPath, "Eras.csv"),
                  projectFile: Path.Combine(folderPath, "Projects.csv"),
                  biomeFile: Path.Combine(folderPath, "Biomes.csv")
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

        public string BiomeNameFromHash(string def)
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
            if (this.EraDefinitionByHash.ContainsKey(hash)) return this.EraDefinitionByHash[hash].Name;
            else return hash;
        }

        public EraDefinition TryEraDefinitionFromHash(string hash)
        {
            if (this.EraDefinitionByHash.ContainsKey(hash)) return this.EraDefinitionByHash[hash];
            else return new(hash);
        }

        public string GiantNameFromHash(string hash)
        {
            if (this.GiantDefinitionByHash.ContainsKey(hash)) return this.GiantDefinitionByHash[hash].Name;
            else return hash;
        }

        public GiantDefinition TryGiantDefinitionFromHash(string hash)
        {
            if (this.GiantDefinitionByHash.ContainsKey(hash)) return this.GiantDefinitionByHash[hash];
            else return new(hash);
        }

        public CityProjectDefinition TrProjectDefinitionFromHash(string hash)
        {
            if (this.ProjectDefinitionByHash.ContainsKey(hash)) return this.ProjectDefinitionByHash[hash];
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
            }
        }
    }
}
