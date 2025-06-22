using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace DanielRenner.SettledIn
{
    public class WorkGiver_ManageSettlement : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DefOfs_SettledIn.TableSettlementOffice);

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public static bool CanManageNow(Pawn pawn, bool forced)
        {
            return !pawn.DevelopmentalStage.Juvenile() && !pawn.Downed && !pawn.Drafted && !pawn.WorkTypeIsDisabled(DefOfs_SettledIn.Managing) && pawn.workSettings.WorkIsActive(DefOfs_SettledIn.Managing) && pawn.Awake() && !pawn.IsBurning() && (forced || !PawnUtility.WillSoonHaveBasicNeed(pawn, 0.1f)) && !pawn.InMentalState && pawn.GetLord() == null;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            // skip, if we don't need to manage right now, e.g. if the management meter is full
            ;
            var ret = !CanManageNow(pawn, forced);
            return ret;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Log.Debug("checking thing=" + thing.Label + " for pawn=" + pawn.Label + " managing");
            Building_TableSettlementOffice table = thing as Building_TableSettlementOffice;
            // make sure we can actually use the table etc.
            if (table == null || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning() || thing.IsForbidden(pawn) || (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)))
            {
                return null;
            }
            // only do this job, if there is room in the buffer
            var settlementResources = thing.Map?.GetComponent<MapComponent_SettlementResources>();
            if (settlementResources == null)
            {
                return null;
            }
            if (settlementResources.SettlementLevel < 6) // only active startgin at level 6
            {
                return null;
            }
            var assignableComp = thing.TryGetComp<CompAssignableToPawn>();
            // make sure we either have a unassigned table or a table assigned for all
            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && !assignableComp.AssignedPawnsForReading.Contains(pawn) && assignableComp.AssignedPawnsForReading.Count > 0)
            {
                return null;
            }

            var howMuchRemaining = (float)(settlementResources.ManagementBuffer_max - settlementResources.ManagementBuffer_current) / settlementResources.ManagementBuffer_max;
            if (howMuchRemaining < 0.25)
            {
                return null;
            }
            // some hours (> 3) of work left?
            if (settlementResources.ManagementBuffer_max - settlementResources.ManagementBuffer_current < 10000)
            {
                return null;
            }

            // create the job
            Log.Debug("creating manage job");
            Job job = JobMaker.MakeJob(DefOfs_SettledIn.ManageSettlement, table);
            return job;
        }

    }
}
