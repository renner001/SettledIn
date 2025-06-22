using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class StatPart_MoveSpeedSettlement : StatPart
    {
		public override void TransformValue(StatRequest req, ref float val)
		{
            if (req.HasThing && Applies(req.Thing))
			{
                val *= 1.5f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				return "\nSettlement Management Bonus: x150%";
			}
			return null;
		}

		public static bool Applies(Thing th)
		{
            var map = th?.Map;
            var comp = map == null ? null : map.GetComponent<MapComponent_SettlementResources>();
			if (comp != null && comp.SettlementLevel > 4) // only startign at level 5 the bonus applies
			{
                return comp.ManagementBuffer_current > 0;
            }
			return false;
		}
	}
}
