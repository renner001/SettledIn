using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{
    public class PlaceWorker_ShowAdjustableFacilitiesRange : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Log.DebugOnce("at least PlaceWorker_ShowAdjustableFacilitiesRange is gettign called..");
            if (thing == null && def != null)
            {
                var prop = def.GetCompProperties<CompProperties_AdjustableFacility>();
                if (prop == null) 
                {
                    return;
                }
                GenDraw.DrawRadiusRing(center, prop.maxDistance - 0.1f);
            }
            else if (thing != null)
            {
                var compAdjustableFacilities = thing.TryGetComp<Comp_AdjustableFacility>();
                if (compAdjustableFacilities == null)
                    return;
                GenDraw.DrawRadiusRing(center, compAdjustableFacilities.CurrentRange - 0.1f);
            }
            
        }
    }
}
