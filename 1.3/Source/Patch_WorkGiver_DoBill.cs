using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DanielRenner.SettledIn
{
    // not necessary, but if we really want to do this for performance reaseons, we need to do it on WorkGiver_Scanner base class
    /*
    [HarmonyPatch(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.HasJobOnThing),)]
    public static class Patch_WorkGiver_DoBill_HasJobOnThing
    {
        public static bool Prefix(WorkGiver_DoBill __instance, ref bool __result, Pawn pawn, Thing t, bool forced)
        {
            Log.DebugOnce("patch Patch_WorkGiver_DoBill_HasJobOnThing.Prefix() is getting called...");
            var assignableComp = t.TryGetComp<CompAssignableToPawn>();
            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && !assignableComp.AssignedPawnsForReading.Contains(pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    */

    [HarmonyPatch(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.JobOnThing))]
    public static class Patch_WorkGiver_DoBill_JobOnThing
    {
        public static bool Prefix(WorkGiver_DoBill __instance, ref Job __result, Pawn pawn, Thing thing, bool forced)
        {
            Log.DebugOnce("patch Patch_WorkGiver_DoBill_JobOnThing.Prefix() is getting called...");
            var assignableComp = thing.TryGetComp<CompAssignableToPawn>();
            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && assignableComp.AssignedPawnsForReading.Count > 0 && !assignableComp.AssignedPawnsForReading.Contains(pawn))
            {
                __result = null;
                return false;
            }
            return true;
        }
    }

}
