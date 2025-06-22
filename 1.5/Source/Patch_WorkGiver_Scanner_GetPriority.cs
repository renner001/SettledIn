using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{   
    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.GetPriority), new[] { typeof(Pawn), typeof(TargetInfo) })]
    static class Patch_WorkGiver_Scanner_GetPriority
    {
        static void Postfix(ref float __result, Pawn pawn, TargetInfo t, WorkGiver_Scanner __instance)
        {
            if (!(__instance is WorkGiver_DoBill))
                return;
            Log.DebugOnce("Patch for WorkGiver_Scanner.GetPriority() is getting called...");
            if (t == null)
                return;
            if (t.Thing == null || !(t.Thing is Building))
                return;
            var comp = (t.Thing as Building).GetComp<CompAssignableToPawn>();
            if (comp == null)
                return;
            if (comp.AssignedPawns.Count() == 0)
                return;

            Log.DebugOnce($"Increased Priority for pawn {pawn} on thing {t.Thing} by 50000f to foster use of personal benches over public benches");
            __result += 50000f;
        }
    }
}
