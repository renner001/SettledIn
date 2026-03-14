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
            if (thing == null)
            {
                return true;
            }
            //Log.DebugOnce($"patch Patch_WorkGiver_DoBill_JobOnThing.Prefix() is getting called for {thing.def}...");
            if (forced)
            {
                return true;
            }
            /*
            var containedInRoom = thing.GetRoom();
            var onMap = thing.Map;
            if (onMap != null && onMap.IsPlayerHome)
            {
                var settlementScoreManager = onMap.GetComponent<MapComponent_SettlementResources>();
                if (settlementScoreManager != null)
                {
                    containedInRoom.ID
                    settlementScoreManager.
                }
            }*/
            var assignableComp = thing.TryGetComp<CompAssignableToPawn>();
            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && assignableComp.AssignedPawnsForReading.Count > 0 && !assignableComp.AssignedPawnsForReading.Contains(pawn))
            {
                Log.DebugOnce($"patch Patch_WorkGiver_DoBill_JobOnThing.Prefix() for {thing.def} equated to false according to comp {assignableComp}");
                __result = null;
                return false;
            }
            //Log.DebugOnce($"patch Patch_WorkGiver_DoBill_JobOnThing.Prefix() for {thing.def} equated to true according to comp {assignableComp}");
            return true;
        }
    }

}
