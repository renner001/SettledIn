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
    [HarmonyPatch(typeof(Building_WorkTable), nameof(Building_WorkTable.UsedThisTick))]
    public static class Patch_Building_WorkTable_UsedThisTick
    {
        public static bool Prefix(Building_WorkTable __instance)
        {
            // performance enhancement: only do this once every second
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                Log.DebugOnce("patch Patch_Building_WorkTable_UsedThisTick.Prefix() is getting called...");
                var assignableComp = __instance.TryGetComp<CompAssignableToPawn>();
                // we have to assume that the one assigned pawn is the one doing the work. should work out...
                if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && assignableComp.AssignedPawnsForReading.Count > 0) // maybe change to == 1?
                {
                    Map currentMap = Find.CurrentMap;
                    if (currentMap != null && currentMap.IsPlayerHome)
                    {
                        var settlementScoreManager = currentMap.GetComponent<MapComponent_SettlementResources>();
                        if (SettlementLevelUtility.IsBenefitActiveAt(settlementScoreManager.SettlementLevel, SettlementLevelUtility.Benefit_lvl1_PersonalWorkbench))
                        {
                            var pawn = assignableComp.AssignedPawnsForReading[0]; 
                            pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.WorkedOnPersonalWorkbench));
                        }
                    }    
                }
            }
            return true;
        }
    }

}
