using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    internal class PlaceWorker_MinSettlementLevel : PlaceWorker
    {

        // Token: 0x06009BA3 RID: 39843 RVA: 0x00385F30 File Offset: 0x00384130
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            Log.DebugOnce("at least PlaceWorker_MinSettlementLevel.AllowsPlacing() is getting called...");
            if (!(def is ThingDef))
            {
                Log.WarningOnce("used PlaceWorker_MinSettlementLevel on non-thing=" + def.ToString() + " - this will never restrict it's placement.");
                return true;
            }
            var thingDef = def as ThingDef;
            if (map == null)
            {
                return true;
            }
            var settlementComp = map.GetComponent<MapComponent_SettlementResources>();
            if (settlementComp == null)
                return "DanielRenner.SettledIn.RequiresSettlementCenterOnMap".Translate();
            var requiredLevel = thingDef.GetCompProperties<CompProperties_SettlementLevelRequired>().MinSettlementLevel;
            if (requiredLevel > settlementComp.SettlementLevel)
            {
                return "DanielRenner.SettledIn.RequiresHigherSettlementLevel".Translate();
            }
            return true;
        }

    }
}
