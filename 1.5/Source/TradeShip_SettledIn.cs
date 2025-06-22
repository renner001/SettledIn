using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanielRenner.SettledIn
{
    public class TradeShip_SettledIn : TradeShip
    {
        public TradeShip_SettledIn(): base() { }
        public TradeShip_SettledIn(TraderKindDef def, Faction faction = null) : base(def, faction) { }

        // saving and loading the map data
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
