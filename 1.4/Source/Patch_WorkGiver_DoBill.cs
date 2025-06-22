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
