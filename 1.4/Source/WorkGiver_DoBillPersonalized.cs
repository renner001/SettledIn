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
    // removed from code and replaced by direct injection into patch of workgiver dobill
    class WorkGiver_DoBillPersonalized : WorkGiver_DoBill
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var assignableComp = t.TryGetComp<CompAssignableToPawn>();

            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && assignableComp.AssignedPawnsForReading.Contains(pawn))
            {
                return base.HasJobOnThing(pawn, t, forced);
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            var assignableComp = thing.TryGetComp<CompAssignableToPawn>();
            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && assignableComp.AssignedPawnsForReading.Contains(pawn))
            {
                var jobOnThing = base.JobOnThing(pawn, thing, forced);
                if (jobOnThing != null)
                {
                    Log.Debug("started new job=" + jobOnThing + " for pawn=" + pawn + " on thing=" + thing);
                }

                return jobOnThing;
            }
            return null;
        }
    }
}
