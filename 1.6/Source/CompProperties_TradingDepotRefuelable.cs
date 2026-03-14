using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanielRenner.SettledIn
{
    public class CompProperties_TradingDepotRefuelable : CompProperties_Refuelable
    {
        public CompProperties_TradingDepotRefuelable()
        {
            this.compClass = typeof(Comp_TradingDepotRefuelable);
        }
    }
}
