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
    public class JobGiver_CollectCommodities : ThinkNode_JobGiver
    {

        private static ThingFilter collectibles;
        private static ThingFilter Collectibles
        {
            get
            {
                if (collectibles == null)
                {
                    var commodityDef = DefOfs_SettledIn.Commodity;
                    collectibles = new ThingFilter();
                    collectibles.SetAllow(commodityDef, true);
                }
                return collectibles;
            }
        }


        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.DebugOnce("at least JobGiver_CollectCommodities.TryGiveJob() is getting called...");
            var commodityNeed = pawn.needs.TryGetNeed<CommodityNeed>();
            var settlementManager = pawn.Map.GetComponent<MapComponent_SettlementResources>();
            Job collectCommodityJob = null;
            if (settlementManager != null && SettlementLevelUtility.IsBenefitActiveAt(settlementManager.SettlementLevel, SettlementLevelUtility.Benefit_lvl2_CommodityConsumption))
            {
                // check if it is time to collect commodities
                if (commodityNeed != null && commodityNeed.MaxLevel - commodityNeed.CurLevel > 0.9) // todo: fine tune values
                {
                    Log.DebugOnce("pawn " + pawn + " wants to collect commodities");
                    // collect commodities!
                    var nextCommodity = FindBestCommodity(pawn);
                    if (nextCommodity != null)
                    {
                        Log.Debug("pawn " + pawn + " will collect " + nextCommodity);
                        collectCommodityJob = JobMaker.MakeJob(DefOfs_SettledIn.CollectCommodities, nextCommodity);
                    }
                }
            }
            
            return collectCommodityJob;
        }

        private static Thing FindBestCommodity(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                if (!x.IsForbidden(pawn) && pawn.CanReserve(x))
                {
                    return true;
                }
                return false;
            };
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, Collectibles.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }

    }
}
