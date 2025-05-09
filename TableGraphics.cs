using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Reus2Surveyor
{
    class TableGraphics
    {
        public static Dictionary<string, byte[]> spiritSquares = new()
        {
            {"Botanist",  Properties.Resources.BotanistSquare},
            {"Diplomat",  Properties.Resources.DiplomatSquare},
            {"General", Properties.Resources.GeneralSquare},
            {"Goddess", Properties.Resources.GoddessSquare},
            {"Huntress", Properties.Resources.HuntressSquare},
            {"Inventor",  Properties.Resources.InventorSquare},
            {"Merchant",  Properties.Resources.MerchantSquare},
            {"Miner",  Properties.Resources.MinerSquare},
            {"Painter",  Properties.Resources.PainterSquare},
            {"Pirate Queen",  Properties.Resources.PirateQueenSquare},
            {"Poet",  Properties.Resources.PoetSquare},
            {"Sage",  Properties.Resources.SageSquare},
            {"Villain",  Properties.Resources.VillainSquare},
        };

        public static Dictionary<string, byte[]> giantSquares = new()
        {
            {"Satari", Properties.Resources.SatariSquare },
            {"Reginald", Properties.Resources.ReginaldSquare },

            {"Khiton", Properties.Resources.KhitonSquare },
            {"Jangwa", Properties.Resources.JangwaSquare },

            {"Atlas", Properties.Resources.AtlasSquare },
            {"Aegir", Properties.Resources.AegirSquare },
            {"Icy Aegir", Properties.Resources.IcyAegirSquare },
        };

        public static Dictionary<string, int> PercentsToWholeNumber(Dictionary<string, double> percents, int totalAmount = 100)
        {
            Dictionary<string, int> output = percents.Select(kv => new KeyValuePair<string, int>(kv.Key, (int)Math.Floor(kv.Value * totalAmount))).ToDictionary();
            int remainder = totalAmount - output.Values.Sum();
            for (; remainder > 0; remainder--)
            {
                output[output.MinBy(kv => kv.Value).Key] += 1;
            }
            return output;
        }

        public static Dictionary<string, Color> BiomeColors = new()
        {
            // Sampled from in-game biome icons
            /*{"Desert", new MagickColor("#FFAE52") },
            {"Forest",  new MagickColor("#A5E622") },k
            {"Ice Age",  new MagickColor("#B9D3DC") },
            {"Ocean",  new MagickColor("#27A3F9") },
            {"Rainforest",  new MagickColor("#127529") },
            {"Savanna",  new MagickColor("#FFD752") },
            {"Taiga",  new MagickColor("#71CCBE") },*/

            {"Desert", Color.ParseHex("FF9B59")},
            {"Forest",  Color.ParseHex("59F704") },
            {"Ice Age",  Color.ParseHex("BFFFFF") },
            {"Ocean",  Color.ParseHex("052DF8") },
            {"Rainforest",  Color.ParseHex("00A571") },
            {"Savanna",  Color.ParseHex("FFE108") },
            {"Taiga",  Color.ParseHex("7CE2D1") },
        };

        public static Image BiomeTypePercentsToMinimap(Dictionary<string, double> percents, int width = 100, int height = 24)
        {
            Dictionary<string, int> biomeStripes = PercentsToWholeNumber(percents, width);

            using MemoryStream s = new();
            Image image = new Image<Rgb24>(width, height);

            int leftPos = 0;
            foreach ((string biomeName, int stripeWidth) in biomeStripes)
            {
                Rectangle biomeBar = new Rectangle(leftPos, 0, stripeWidth, height);
                Brush fillBrush = new SolidBrush(BiomeColors[biomeName]);
                image.Mutate(x => x.Fill(fillBrush, biomeBar));
                leftPos += stripeWidth;
            }
            return image;
        }

        public static Dictionary<int, (string biomeTypeName, int px)> PositionalDictToWholeNumber(
            Dictionary<int, (string biomeTypeName, double percentSize)> biomeInfo,
            int totalAmount = 100)
        {
            Dictionary<int, (string biomeTypeName, int px)> output =
                biomeInfo.Select(kv => new KeyValuePair<int, (string, int)>(
                    kv.Key,
                    (kv.Value.biomeTypeName, (int)Math.Floor(kv.Value.percentSize * totalAmount))))
                .ToDictionary();

            int remainder = totalAmount - output.Values.Select(x => x.px).Sum();
            for (; remainder > 0; remainder--)
            {
                int minKey = output.MinBy(kv => kv.Value.px).Key;
                output[minKey] = (output[minKey].biomeTypeName, output[minKey].px + 1);
            }

            return output;
        }

        public static Image BiomePositionalToMinimap(Dictionary<int, (string biomeTypeName, double percentSize)> biomeInfo, int width = 100, int height = 24)
        {
            Dictionary<int, (string biomeTypeName, int px)> biomeStripes = PositionalDictToWholeNumber(biomeInfo, width);
            List<int> anchorPatches = biomeStripes.Keys.ToList();
            anchorPatches.Sort();

            using MemoryStream s = new();
            Image image = new Image<Rgb24>(width, height);

            int leftPos = 0;
            foreach (int anchorPatch in anchorPatches)
            {
                string biomeName = biomeStripes[anchorPatch].biomeTypeName;
                int stripeWidth = biomeStripes[anchorPatch].px;

                Rectangle biomeBar = new Rectangle(leftPos, 0, stripeWidth, height);
                Brush fillBrush = new SolidBrush(BiomeColors[biomeName]);
                image.Mutate(x => x.Fill(fillBrush, biomeBar));

                leftPos += stripeWidth;
            }
            return image;
        }
    }
}
