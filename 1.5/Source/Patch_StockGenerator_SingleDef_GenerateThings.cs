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
    /*
    [HarmonyPatch(typeof(StockGenerator_SingleDef), nameof(StockGenerator_SingleDef.GenerateThings))]
    public static class Patch_StockGenerator_SingleDef_GenerateThings
    {
        public static void Postfix(StockGenerator_SingleDef __instance, int forTile, Faction faction, IEnumerable<Thing> __result)
        {
            Log.DebugOnce("Patch_StockGenerator_SingleDef_GenerateThings.Postfix() is getting called...");
            if (__result == null)
            {
                return;
            }
            var thingDef = GetThingDef(__instance);
            var results = __result.ToArray();
            Log.Debug("Patch_StockGenerator_SingleDef_GenerateThings: thingDef=" + thingDef + ", __result=" + __result);
            if (thingDef == ThingDefOf.Silver && results.Length > 0 && results[0].def == ThingDefOf.Silver)
            {
                var silverResult = results[0];
                var prev = silverResult.stackCount;
                silverResult.stackCount += 1000000;
                Log.Debug("increasing stack of silver from " + prev + " to " + silverResult.stackCount);
                // todo: use a real value based on the settlement score instead of a flat 100000
                __result = results;
            }
        }

        public static ThingDef GetThingDef(StockGenerator_SingleDef instance)
        {
            Type stockGeneratorType = typeof(StockGenerator_SingleDef);
            FieldInfo fieldInfo = stockGeneratorType.GetField("thingDef", BindingFlags.NonPublic | BindingFlags.Instance);
            var val = fieldInfo.GetValue(instance) as ThingDef;
            return val;
        }
    }
    */
    
    [HarmonyPatch(typeof(StockGenerator), "RandomCountOf")]
    static class Patch_StockGenerator_RandomCountOf
    {
        static void Postfix(ref int __result, StockGenerator __instance)
        {
            Log.DebugOnce("Patch for StockGenerator.RandomCountOf() is getting called...");
            Map currentMap = Find.CurrentMap;
            if (currentMap != null && currentMap.IsPlayerHome)
            {
                var settlementScoreManager = currentMap.GetComponent<MapComponent_SettlementResources>();
                var statistics = settlementScoreManager.cachedStatistics;
                if (SettlementLevelUtility.IsBenefitActiveAt(settlementScoreManager.SettlementLevel, SettlementLevelUtility.Benefit_lvl2_TraderStock))
                {
                    var factor = SettlementScoreUtility.GetTraderQuantityMultiplierFromSettlementScore(statistics.totalPointsByScore + statistics.totalPointsByRoomTypes);
                    var prevValue = __result;
                    __result = (int)(factor * __result);
                    Log.Debug("Patch_StockGenerator_RandomCountOf.Postfix(): changing stock from prev=" + prevValue + " to now=" + __result);
                }
            }
        }
    }
}
