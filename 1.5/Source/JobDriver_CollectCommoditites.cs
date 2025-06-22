using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DanielRenner.SettledIn
{
    class JobDriver_CollectCommodities : JobDriver
	{
		private const TargetIndex CollectibleIndex = TargetIndex.A;
		public const int CollectingDuration = 80;
		public const float NeedRefillPerCommodity = 1.00f;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(CollectibleIndex).Thing, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(CollectibleIndex);

			yield return Toils_General.DoAtomic(delegate
			{
				Log.Debug("first toil of gathering commodities called");
				job.count = getItemCountToFill(pawn);
			});
			Toil reserveCollectible = Toils_Reserve.Reserve(CollectibleIndex);
			yield return reserveCollectible;
			yield return Toils_Goto.GotoThing(CollectibleIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(CollectibleIndex).FailOnSomeonePhysicallyInteracting(CollectibleIndex);
			//yield return Toils_Haul.StartCarryThing(CollectibleIndex, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			//yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveCollectible, CollectibleIndex, TargetIndex.None, takeFromValidStorage: true);
			//yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(CollectingDuration).WithProgressBarToilDelay(CollectibleIndex);
			yield return Toils_General.DoAtomic(delegate
			{
				Log.Debug("last toil of gathering commodities called");
				Thing collectible = job.GetTarget(CollectibleIndex).Thing;
				int numItemsToFullyPay = getItemCountToFill(pawn);
				int numItemsToSplitOff = Mathf.Min(numItemsToFullyPay, collectible.stackCount);
				Log.Debug("crediting " + numItemsToSplitOff + " of a total " + collectible.stackCount + " " + collectible.def);
				creditCommodity(pawn, numItemsToSplitOff); // credit the worth of the items
				collectible.SplitOff(numItemsToSplitOff).Destroy(); // destroy the items
                // spawn the silver
				var silverQuantity = Mathf.CeilToInt(numItemsToSplitOff * DefOfs_SettledIn.Commodity.BaseMarketValue / ThingDefOf.Silver.BaseMarketValue);
				var numDrops = silverQuantity / ThingDefOf.Silver.stackLimit;
				var silverStacks = new List<Thing>();
				for (int i=0;i<numDrops;i++)
				{
                    var silverDrop = ThingMaker.MakeThing(ThingDefOf.Silver);
					silverDrop.stackCount = ThingDefOf.Silver.stackLimit;
                    silverStacks.Add(silverDrop);
                }
				if (silverQuantity % ThingDefOf.Silver.stackLimit != 0)
				{
                    var silverDrop = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silverDrop.stackCount = silverQuantity % ThingDefOf.Silver.stackLimit;
                    silverStacks.Add(silverDrop);
                }
				silverStacks.ForEach(stack => { GenPlace.TryPlaceThing(stack, pawn.Position, pawn.Map, ThingPlaceMode.Near, null, null, default(Rot4)); });
            });
			yield break;
		}

		private static int getItemCountToFill(Pawn pawn)
        {
			var requiredItemCount = 0;
			var commodityNeed = pawn.needs.TryGetNeed<CommodityNeed>();
			if (commodityNeed != null)
			{
				var difference = commodityNeed.MaxLevel - commodityNeed.CurLevel;
				// take at least one up to what we need to refill
				requiredItemCount = Math.Min(1,(int)Math.Round(difference / NeedRefillPerCommodity));
				if (requiredItemCount * NeedRefillPerCommodity > difference && requiredItemCount > 1)
				{
                    requiredItemCount -= 1;
                }
            }
			return requiredItemCount;
		}

		private static void creditCommodity(Pawn pawn, int count)
        {
            var commodityNeed = pawn.needs.TryGetNeed<CommodityNeed>();
            if (commodityNeed != null)
            {
				commodityNeed.CurLevel += count * NeedRefillPerCommodity;
            }
			else
			{
                Log.Warning($"consumed commodities though there is no commodity need on pawn ${pawn}");
            }
		}
	}
}
