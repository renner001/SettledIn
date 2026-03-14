using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class CompProperties_SettlementLevelRequired : CompProperties
    {

		public CompProperties_SettlementLevelRequired()
		{
			compClass = typeof(Comp_SettlementLevelRequired);
		}

		public int MinSettlementLevel = 1;
    }
}
