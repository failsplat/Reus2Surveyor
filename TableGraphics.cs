using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
            {"Entomologist",  Properties.Resources.EntomologistSquare},
            {"General", Properties.Resources.GeneralSquare},
            {"Goddess", Properties.Resources.GoddessSquare},
            {"Huntress", Properties.Resources.HuntressSquare},
            {"Inventor",  Properties.Resources.InventorSquare},
            {"Merchant",  Properties.Resources.MerchantSquare},
            {"Miner",  Properties.Resources.MinerSquare},
            {"Painter",  Properties.Resources.PainterSquare},
            {"Pirate Queen",  Properties.Resources.PirateQueenSquare},
            {"Poet",  Properties.Resources.PoetSquare},
            {"Ranger",  Properties.Resources.RangerSquare},
            {"Romantic",  Properties.Resources.RomanticSquare},
            {"Sage",  Properties.Resources.SageSquare},
            {"Villain",  Properties.Resources.VillainSquare},
        };

        public static Dictionary<string, byte[]> giantSquares = new()
        {
            {"Satari", Properties.Resources.SatariSquare },
            {"Reginald", Properties.Resources.ReginaldSquare },
            {"Wet Reginald", Properties.Resources.WetReginaldSquare },

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

        public static Image BiomePositionalToMinimap(
            Dictionary<int, (string biomeTypeName, double percentSize)> biomeInfo, Glossaries glossInstance, int width = 125, int height = 24
            )
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
                Brush fillBrush = new SolidBrush(Color.ParseHex(glossInstance.GetBiomeColor(biomeName)));
                image.Mutate(x => x.Fill(fillBrush, biomeBar));

                leftPos += stripeWidth;
            }
            return image;
        }
    }
}
