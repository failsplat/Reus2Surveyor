using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using ImageMagick.Drawing;

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

        public static Dictionary<string, int> PercentsToWholeNumber(Dictionary<string, double> percents, int totalAmount=100)
        {
            Dictionary<string, int> output = percents.Select(kv => new KeyValuePair<string, int>(kv.Key, (int)Math.Floor(kv.Value*totalAmount))).ToDictionary();
            int remainder = totalAmount - output.Values.Sum();
            for (; remainder > 0; remainder--) {
                output[output.MinBy(kv => kv.Value).Key] += 1;
            }
            return output;
        }

        public static Dictionary<string, MagickColor> BiomeColors = new()
        {
            // Sampled from in-game biome icons
            /*{"Desert", new MagickColor("#FFAE52") },
            {"Forest",  new MagickColor("#A5E622") },k
            {"Ice Age",  new MagickColor("#B9D3DC") },
            {"Ocean",  new MagickColor("#27A3F9") },
            {"Rainforest",  new MagickColor("#127529") },
            {"Savanna",  new MagickColor("#FFD752") },
            {"Taiga",  new MagickColor("#71CCBE") },*/

            {"Desert", new MagickColor("#FF9B59") },
            {"Forest",  new MagickColor("#59F704") },
            {"Ice Age",  new MagickColor("#BFFFFF") },
            {"Ocean",  new MagickColor("#052DF8") },
            {"Rainforest",  new MagickColor("#00A571") },
            {"Savanna",  new MagickColor("#FFE108") },
            {"Taiga",  new MagickColor("#7CE2D1") },
        };

        public static MagickImage BiomePercentsToMinimap(Dictionary<string, double> percents, int width=100, int height=24)
        {
            Dictionary<string, int> biomeStripes = PercentsToWholeNumber(percents, width);

            using MemoryStream s = new();
            MagickImage image = new MagickImage(MagickColors.Transparent, (uint)width, (uint)height);
            image.Format = MagickFormat.Png;

            int leftPos = 0;
            Drawables dr = new Drawables();
            foreach ((string biomeName, int stripeWidth) in biomeStripes)
            {
                dr.FillColor(BiomeColors[biomeName]);
                dr.Rectangle(leftPos, 0, leftPos + stripeWidth, height);
                leftPos += stripeWidth;
            }
            dr.Draw(image);
            return image;
        }
    }
}
