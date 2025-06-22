using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class StatPart_WorkTablePersonalized : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && Applies(req.Thing))
            {
                val *= 1.2f;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && Applies(req.Thing))
            {
                return "Personalization bonus: x120%";
            }
            return null;
        }

        public static bool Applies(Thing th)
        {
            var assignablePawnComp = th.TryGetComp<CompAssignableToPawn_ProductionBuilding>();
            if (assignablePawnComp != null && assignablePawnComp.AssignedPawns != null && assignablePawnComp.AssignedPawns.Count() > 0)
            {
                var settlementResources = th.Map != null ? th.Map.GetComponent<MapComponent_SettlementResources>() : null;
                if (settlementResources != null && settlementResources.SettlementLevel > 3)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
