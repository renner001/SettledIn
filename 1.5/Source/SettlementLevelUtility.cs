using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace DanielRenner.SettledIn
{
    public static class SettlementLevelUtility
    {
        /**
         * maximum level the settlement can reach
         * */
        public const int MaxLevel = 5;
        public static bool CheckRequirements(int settlementLevel, Map targetMap, out string description)
        {
            if (settlementLevel > MaxLevel)
            {
                Log.WarningOnce("SettlementLevelUtility.CheckRequirements() with settlementLevel=" + settlementLevel + " was called. This level does not exist.");
                description = "level does not exist";
                return false;
            }

            var settlementResources = targetMap.GetComponent<MapComponent_SettlementResources>();
            if (settlementResources == null)
            {
                Log.WarningOnce("SettlementLevelUtility.CheckRequirements(): could not find settlement resources on map=" + targetMap.ToString());
                description = "please wait for the map to load fully";
                return false;
            }

            /*
            if (settlementResources.SettlementLevel >= settlementLevel)
            {
                description = "Already reached!";
                return true;
            }*/

            const string colorBad = "<color=#ff2222>";
            const string colorGood = "<color=#22ff22>";
            const string colorEnd = "</color>";

            switch (settlementLevel)
            {
                case 0:
                    var settlementCenterExists = targetMap.listerThings.ThingsOfDef(DefOfs_SettledIn.SettlementCenter).Count() > 0;
                    description = "";
                    description += settlementCenterExists ? colorGood + "1/1" : colorBad + "0/1";
                    description += " Build Settlement Center" + colorEnd;
                    return settlementCenterExists;
                case 1:
                    var achieved3RoomTypes = settlementResources.cachedStatistics.achievedRoomTypes >= 3;
                    description = "";
                    description += achieved3RoomTypes ? colorGood : colorBad;
                    description += settlementResources.cachedStatistics.achievedRoomTypes + "/3 Room Types" + colorEnd;
                    return achieved3RoomTypes;
                case 2:
                    var achieved6RoomTypes = settlementResources.cachedStatistics.achievedRoomTypes >= 6;
                    description = "";
                    description += achieved6RoomTypes ? colorGood : colorBad;
                    description += settlementResources.cachedStatistics.achievedRoomTypes + "/6 Room Types" + colorEnd;
                    return achieved6RoomTypes;
                case 3:
                    var achieved9RoomTypes = settlementResources.cachedStatistics.achievedRoomTypes >= 9;
                    description = "";
                    description += achieved9RoomTypes ? colorGood : colorBad;
                    description += settlementResources.cachedStatistics.achievedRoomTypes + "/9 Room Types" + colorEnd;
                    return achieved9RoomTypes;
                case 4:
                    var achieved12RoomTypes = settlementResources.cachedStatistics.achievedRoomTypes >= 12;
                    description = "";
                    description += achieved12RoomTypes ? colorGood : colorBad;
                    description += settlementResources.cachedStatistics.achievedRoomTypes + "/12 Room Types" + colorEnd;
                    return achieved12RoomTypes;
                case 5:
                    var achieved15RoomTypes = settlementResources.cachedStatistics.achievedRoomTypes >= 15;
                    description = "";
                    description += achieved15RoomTypes ? colorGood : colorBad;
                    description += settlementResources.cachedStatistics.achievedRoomTypes + "/15 Room Types" + colorEnd;
                    return achieved15RoomTypes;
                /*case 6:
                    var achieved18RoomTypes = settlementResources.cachedStatistics.achievedRoomTypes >= 18;
                    description = "";
                    description += achieved18RoomTypes ? colorGood : colorBad;
                    description += settlementResources.cachedStatistics.achievedRoomTypes + "/18 Room Types" + colorEnd;
                    return achieved18RoomTypes;*/
                default:
                    Log.ErrorOnce("SettlementLevelUtility.CheckRequirements(): Missing requirements for settlementLevel=" + settlementLevel, 866392065);
                    description = "error";
                    return false;
            }
        }


        public static string Benefit_lvl0_SettlementCenter = "DanielRenner.SettledIn.BenefitSettlementCenter";
        public static string Benefit_lvl0_Immigrants = "DanielRenner.SettledIn.BenefitImmigrants";
        public static string Benefit_lvl1_ManagedSettlements = "DanielRenner.SettledIn.BenefitManagedSettlements";
        public static string Benefit_lvl1_PersonalWorkbench = "DanielRenner.SettledIn.BenefitPersonalWorkbench";
        public static string Benefit_lvl1_RoomFocus = "DanielRenner.SettledIn.BenefitRoomFocus";
        public static string Benefit_lvl2_ColonistFocus = "DanielRenner.SettledIn.BenefitColonistFocus";
        public static string Benefit_lvl2_CommodityConsumption = "DanielRenner.SettledIn.BenefitCommodityConsumption";
        public static string Benefit_lvl2_TraderStock = "DanielRenner.SettledIn.BenefitTraderStock";
        public static string Benefit_lvl3_CozyRooms = "DanielRenner.SettledIn.BenefitCozyRooms";
        public static string Benefit_lvl3_WalkSpeed = "DanielRenner.SettledIn.BenefitWalkSpeed";
        public static string Benefit_lvl4_GreatHalls = "DanielRenner.SettledIn.BenefitGreatHalls";
        public static string Benefit_lvl4_TidySettlements = "DanielRenner.SettledIn.BenefitTidySettlements";
        public static string Benefit_lvl5_RecreationFocus = "DanielRenner.SettledIn.BenefitRecreationFocus";
        public static string Benefit_lvl5_PristineClothing = "DanielRenner.SettledIn.BenefitPristineClothing";

        public static IEnumerable<string> GetBenefitListOfLevel(int settlementLevel)
        {
            switch (settlementLevel)
            {
                case 0: return new[] { Benefit_lvl0_SettlementCenter, Benefit_lvl0_Immigrants };
                case 1: return new[] { Benefit_lvl1_ManagedSettlements, Benefit_lvl1_PersonalWorkbench, Benefit_lvl1_RoomFocus };
                case 2: return new[] { Benefit_lvl2_ColonistFocus, Benefit_lvl2_CommodityConsumption, Benefit_lvl2_TraderStock };
                case 3: return new[] { Benefit_lvl3_CozyRooms, Benefit_lvl3_WalkSpeed };
                case 4: return new[] { Benefit_lvl4_GreatHalls, Benefit_lvl4_TidySettlements };
                case 5: return new[] { Benefit_lvl5_RecreationFocus, Benefit_lvl5_PristineClothing };
                //case 6: return new[] {  };

            }

            return new string[] { };
        }

        public static bool IsBenefitActiveAt(int settlementLevel, string benefit) 
        {
            for (int i=0;i <= settlementLevel;i++)
            {
                var isContained = GetBenefitListOfLevel(i).Contains(benefit);
                if (isContained) 
                    return true;
            }
            return false;
        }

        public static float CalculateChanceForImmigrants(int settlementLevel)
        {
            return settlementLevel * 0.075f;
        }


        private static TraderKindDef getTargetTraderKindForStellementTrader()
        {
            var playerTechLevel = Faction.OfPlayer.def.techLevel;
            TraderKindDef traderKindToUse = null;
            if (traderKindToUse == null && playerTechLevel >= TechLevel.Industrial)
            {
                traderKindToUse = DefDatabase<TraderKindDef>.GetNamed("Orbital_BulkGoods", false);
            }
            if (traderKindToUse == null)
            {
                traderKindToUse = DefDatabase<TraderKindDef>.GetNamed("Caravan_Outlander_BulkGoods", false);
            }
            if (traderKindToUse == null)
            {
                traderKindToUse = DefDatabase<TraderKindDef>.GetNamed("Caravan_Neolithic_BulkGoods", false);
            }
            if (traderKindToUse == null)
            {
                Log.WarningOnce("found no suitable bulk goods TraderKindDef - falling back to a random trader");
                traderKindToUse = DefDatabase<TraderKindDef>.GetRandom();
            }
            if (traderKindToUse == null)
            {
                Log.Error("current game contains no traders; that will cause significant issues down the road.");
            }
            return traderKindToUse;
        }

        public static TradeShip_SettledIn SettlementTrader_GetOrCreate(Map map)
        {
            if (map == null)
            {
                Log.Error("GetOrCreateSettlementTraderOnMap() was called with null map");
                return null;
            }
            // create the trade ship of our settlement, if it doesn't exist yet.
            var traderKindToUse = getTargetTraderKindForStellementTrader();
            TradeShip_SettledIn existingTradeShip = map?.passingShipManager?.passingShips?.FirstOrDefault(ship => { return ship is TradeShip_SettledIn; }) as TradeShip_SettledIn;
            if (existingTradeShip != null && existingTradeShip.TraderKind != traderKindToUse) // tech level has changed and we need a different trader type
            {
                map.passingShipManager.RemoveShip(existingTradeShip);
                existingTradeShip = null;
            }
            if (existingTradeShip == null) // we don't have any trader yet? We create it
            {
                existingTradeShip = new TradeShip_SettledIn(traderKindToUse, null);
                existingTradeShip.name = "DanielRenner.SettledIn.SettlementTraderName".Translate();
                map.passingShipManager.AddShip(existingTradeShip);
                existingTradeShip.GenerateThings();
            } 
            return existingTradeShip;
        }

        public static void SettlementTrader_RefreshStock(Map map)
        {
            var tradeShip = SettlementTrader_GetOrCreate(map);
            if (tradeShip != null)
            {
                tradeShip.GenerateThings();
            }
        }
    }
}
