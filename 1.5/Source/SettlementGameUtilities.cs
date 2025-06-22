using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public static class SettlementGameUtilities
    {
        public static Pawn GetBestPlayerNegotiatorOnMap(Map map, TraderKindDef forTraderKind, Faction acceptedByFaction = null)
        {
            Pawn negotiator = null;
            float negotiatorSkill = -1f;
            var colonistsOnMap = map.mapPawns.AllHumanlike.Where(humanlike => { return humanlike.Faction != null && humanlike.Faction.IsPlayer; });
            // todo: think about skipping unconcious pawns, etc. But wouldn't the pawn "get up for trading?" if in a settlement?
            foreach (var colonist in colonistsOnMap)
            {
                if (acceptedByFaction != null && !colonist.CanTradeWith(acceptedByFaction, forTraderKind).Accepted)
                    continue;
                if (StatDefOf.TradePriceImprovement.Worker.IsDisabledFor(colonist) /*|| colonist.mindState*/)
                    continue;
                float statValue = colonist.GetStatValue(StatDefOf.TradePriceImprovement, true, -1);
                if (negotiator == null || statValue > negotiatorSkill)
                {
                    negotiator = colonist;
                    negotiatorSkill = statValue;
                }
            }
            if (negotiator == null)
            {
                Log.Warning("could not find a valid negotiator in colony");
            }
            return negotiator;
        }
    }
}
