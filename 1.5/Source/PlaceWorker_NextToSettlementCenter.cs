using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    internal class PlaceWorker_NextToSettlementCenter : PlaceWorker
    {

        // Token: 0x06009BA3 RID: 39843 RVA: 0x00385F30 File Offset: 0x00384130
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            Log.DebugOnce("at least PlaceWorker_NextToSettlementCenter.AllowsPlacing() is getting called...");
            if (!(def is ThingDef))
            {
                Log.WarningOnce("used PlaceWorker_NextToSettlementCenter on non-thing=" + def.ToString() + " - this will never restrict it's placement.");
                return true;
            }
            var settlementCenter = map?.listerThings?.ThingsOfDef(DefOfs_SettledIn.SettlementCenter)?.FirstOrDefault();
            if (settlementCenter == null || settlementCenter.Position.DistanceTo(center) > settlementCenter.def.specialDisplayRadius)
            {
                return "DanielRenner.SettledIn.MustBeNextToSettlementCenter".Translate();
            }
            return true;
        }

    }
}
