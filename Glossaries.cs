using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public readonly Dictionary<string, string> TurningPointHashByName = [], TurningPointNameByHash = [];
        public readonly Dictionary<string, string> YieldHashByName = [], YieldNameByHash = [];

        public readonly Dictionary<string, BioticumDefinition> BioticumDefinitionByHash = [];
        public readonly List<BioticumDefinition> BioticumDefinitionList = [];

        public Glossaries(string nbFile, string bioFile)
        {
            // The main constructor
            using (StreamReader nbsr = new StreamReader(nbFile))
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


            }

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
        }

        public Glossaries(string folderPath)
            : this(nbFile: Path.Combine(folderPath, "NonBiotica.csv"), bioFile: Path.Combine(folderPath, "Biotica.csv"))
        {
        }

        public static readonly Dictionary<int,string> BiomeNameByInt = new()
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
}

    public class BioticumDefinition
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public int Tier { get; private set; }
        public bool Apex { get; private set; }

        public bool Desert { get; private set; } = false;
        public bool Forest { get; private set; } = false;
        public bool IceAge { get; private set; } = false;
        public bool Ocean { get; private set; } = false;
        public bool Rainforest { get; private set; } = false;
        public bool Savanna { get; private set; } = false;
        public bool Taiga { get; private set; } = false;

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

                    case "Desert":
                    case "Forest":
                    case "IceAge":
                    case "Ocean":
                    case "Rainforest":
                    case "Savanna":
                    case "Taiga":
                        bool res = false;
                        if (d.Length == 0)
                        {
                            this.GetType().GetProperty(thisCol).SetValue(this, false);
                            break;
                        }
                        switch (d)
                        {
                            case "1":
                            case "y":
                            case "Y":
                            case "t":
                            case "T":
                            case "true":
                            case "True":
                                this.GetType().GetProperty(thisCol).SetValue(this, true);
                                break;
                            case "0":
                            case "n":
                            case "N":
                            case "f":
                            case "F":
                            case "false":
                            case "False":
                            default:
                                this.GetType().GetProperty(thisCol).SetValue(this, false);
                                break;
                        }
                        break;
                }

            }
        }
    }
}
