using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace DanielRenner.SettledIn
{
    [HarmonyPatch(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.CanLinkTo))]
    public static class Patch_CompAffectedByFacilities_CanLinkTo
    {
        public static void Postfix(CompAffectedByFacilities __instance, ref Thing facility, ref bool __result)
        {
            Log.DebugOnce("patch Patch_CompAffectedByFacilities_CanLinkTo.Postfix() is getting called...");
            if (__result == false)
                return;
            if (facility == null)
                return;
            if (__instance == null)
                return;
            var compAdjustableFacility = facility.TryGetComp<Comp_AdjustableFacility>();
            if (compAdjustableFacility == null)
                return;
            var affectedThing = __instance.parent;
            if (affectedThing == null)
                return;

            // we override the original checks with a waaaay too large maxDistance that is potentially true with false, if the real range is smaller
            Vector3 vector = GenThing.TrueCenter(affectedThing);
            Vector3 vector2 = GenThing.TrueCenter(facility);
            float num = Vector3.Distance(vector, vector2);
            if (num > compAdjustableFacility.CurrentRange)
            {
                __result = false;
            }
        }
    }
}
