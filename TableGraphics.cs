using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
