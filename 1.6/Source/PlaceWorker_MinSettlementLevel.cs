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
            var settlementLevelComp = thingDef.GetCompProperties<CompProperties_SettlementLevelRequired>();
            if (settlementLevelComp == null)
            {
                var error = $"def {def} is missing CompProperties_SettlementLevelRequired";
                Log.Error(error);
                return error;
            }
            var requiredLevel = settlementLevelComp.MinSettlementLevel;
            if (requiredLevel > settlementComp.SettlementLevel)
            {
                return "DanielRenner.SettledIn.RequiresHigherSettlementLevel".Translate();
            }
            return true;
        }

        public override bool IsBuildDesignatorVisible(BuildableDef def)
        {
            Log.DebugOnce("at least PlaceWorker_MinSettlementLevel.IsBuildDesignatorVisible() is getting called..");

            /* this is not required as it is already part of the ArtchitectTab logic
            // God mode check (Standard practice so devs can see everything)
            if (DebugSettings.godMode) return true;*/

            // Map check (The Architect menu always has a CurrentMap context)
            Map map = Find.CurrentMap;
            if (map == null) 
                return false;

            // Get the requirement from your CompProperties
            if (def is ThingDef thingDef)
            {
                var props = thingDef.GetCompProperties<CompProperties_SettlementLevelRequired>();
                if (props != null)
                {
                    var settlementComp = map.GetComponent<MapComponent_SettlementResources>();

                    // 4. If no center exists yet, or level is too low, hide it!
                    if (settlementComp == null || settlementComp.SettlementLevel < props.MinSettlementLevel)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
