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
    [HarmonyPatch(typeof(RoofCollapseUtility), nameof(RoofCollapseUtility.WithinRangeOfRoofHolder))]
    public static class Patch_RoofCollapseUtility_WithinRangeOfRoofHolder
    {
        public static bool Prefix(Map map, ref bool __result)
        {
            Log.DebugOnce("Patch for RoofCollapseUtility.WithinRangeOfRoofHolder() is getting called...");
            // input validation: don't do anything in error cases
            Map currentMap = map;

            if (currentMap != null && currentMap.IsPlayerHome)
            {
                var settlementScoreManager = currentMap.GetComponent<MapComponent_SettlementResources>();
                if (settlementScoreManager != null && SettlementLevelUtility.IsBenefitActiveAt(settlementScoreManager.SettlementLevel,SettlementLevelUtility.Benefit_lvl4_GreatHalls))
                {
                    __result = true;
                    Log.Debug("Patch_RoofCollapseUtility_ConnectedToRoofHolder.Prefix(): ignoring collapse calculation");
                    return false;
                }
            }

            return true;
        }
    }

}
