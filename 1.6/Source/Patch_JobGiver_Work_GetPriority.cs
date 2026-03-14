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
    [HarmonyPatch(typeof(JobGiver_Work), nameof(JobGiver_Work.GetPriority))]
    public static class Patch_JobGiver_Work_GetPriority
    {
        public static void Postfix(JobGiver_Work __instance, ref float __result, ref Pawn pawn)
        {
            Log.DebugOnce("patch Patch_JobGiver_Work_GetPriority.Postfix() is getting called...");
            if (__result > 0)
            {
                TimeAssignmentDef timeAssignmentDef = ((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment);
                if (timeAssignmentDef == TimeAssignmentDefOf.Joy || timeAssignmentDef == TimeAssignmentDefOf.Sleep || timeAssignmentDef == TimeAssignmentDefOf.Meditate)
                {
                    __result = 0f;
                }
            }

        }
    }

}
