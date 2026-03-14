using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public class RitualObligationTargetWorker_SettlementCenter : RitualObligationTargetWorker_ThingDef
    {
        public RitualObligationTargetWorker_SettlementCenter()
        {
        }

        public RitualObligationTargetWorker_SettlementCenter(RitualObligationTargetFilterDef def)
            : base(def)
        {
        }

        protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
        {
            if (!base.CanUseTargetInternal(target, obligation).canUse)
            {
                return false;
            }
            Thing thing = target.Thing;
            return true;
        }

        public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
        {
            yield return DefOfs_SettledIn.SettlementCenter.label;
            yield break;
        }
    }
}
