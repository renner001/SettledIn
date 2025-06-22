using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace DanielRenner.SettledIn
{
    public class RitualTargetFilter_SettlementCenter : RitualTargetFilter
    {
        public RitualTargetFilter_SettlementCenter()
        {
        }

        public RitualTargetFilter_SettlementCenter(RitualTargetFilterDef def)
            : base(def)
        {
        }

        public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
        {
            Pawn pawn = initiator.Thing as Pawn;
            rejectionReason = "";
            if (pawn == null)
            {
                return false;
            }
            Building_SettlementCenter building_SettlementCenter = pawn.Map.listerThings.ThingsOfDef(DefOfs_SettledIn.SettlementCenter).FirstOrDefault() as Building_SettlementCenter;
            if (building_SettlementCenter == null)
            {
                rejectionReason = "AbilityUpgradeSettlementDisabledNoSettlementCenter".Translate();
                return false;
            }
            if (!pawn.CanReserveAndReach(building_SettlementCenter, PathEndMode.InteractionCell, pawn.NormalMaxDanger(), 1, -1, null, false))
            {
                rejectionReason = "AbilityUpgradeSettlementDisabledSettlementCenterNotAccessible".Translate();
                return false;
            }
            return true;
        }

        public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
        {
            var pawn = (Pawn)initiator.Thing;
            if (pawn == null)
            {
                Log.ErrorOnce("RitualTargetFilter_SettlementCenter.BestTarget: pawn is not set", 37525239);
                return TargetInfo.Invalid;
            }
            Building_SettlementCenter building_SettlementCenter = pawn.Map.listerThings.ThingsOfDef(DefOfs_SettledIn.SettlementCenter).FirstOrDefault() as Building_SettlementCenter;
            if (building_SettlementCenter == null)
            {
                return TargetInfo.Invalid;
            }
            return new TargetInfo(building_SettlementCenter.InteractionCell, building_SettlementCenter.Map, false);
        }

        public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
        {
            yield return "AbilityUpgradeSettlementTargetDescSettlementCenter".Translate();
            yield break;
        }
    }
}
