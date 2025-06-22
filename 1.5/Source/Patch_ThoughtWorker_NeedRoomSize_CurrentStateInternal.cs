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
    [HarmonyPatch(typeof(ThoughtWorker_NeedRoomSize), "CurrentStateInternal")]
    public static class Patch_ThoughtWorker_NeedRoomSize_CurrentStateInternal
    {
        public static void Postfix(Pawn p, ref ThoughtState __result)
        {
            Log.DebugOnce("Patch for ThoughtWorker_NeedRoomSize.CurrentStateInternal() is getting called...");
            var map = p?.Map;

            if (map != null && map.IsPlayerHome)
            {
                var settlementScoreManager = map.GetComponent<MapComponent_SettlementResources>();
                if (settlementScoreManager != null && SettlementLevelUtility.IsBenefitActiveAt(settlementScoreManager.SettlementLevel, SettlementLevelUtility.Benefit_lvl3_CozyRooms))
                {
                    if (__result.StageIndex == 0 || __result.StageIndex == 1)
                    {
                        Log.DebugOnce("Patch_ThoughtWorker_NeedRoomSize_CurrentStateInternal.Postfix(): prevented cramped debuff");
                        __result = ThoughtState.Inactive;
                    }
                }
            }
        }
    }

}
