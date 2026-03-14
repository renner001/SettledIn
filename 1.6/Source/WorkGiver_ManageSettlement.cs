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
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllColonistBuildingsOfType<Building_TableSettlementOffice>();
        }

        public static bool CanManageNow(Pawn pawn, bool forced)
        {
            return !pawn.DevelopmentalStage.Juvenile() && !pawn.Downed && !pawn.Drafted && !pawn.WorkTypeIsDisabled(DefOfs_SettledIn.Managing) && pawn.workSettings.WorkIsActive(DefOfs_SettledIn.Managing) && pawn.Awake() && !pawn.IsBurning() && (forced || !PawnUtility.WillSoonHaveBasicNeed(pawn, 0.0f)) && !pawn.InMentalState && pawn.GetLord() == null;
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
            Building_TableSettlementOffice table = thing as Building_TableSettlementOffice;
            // make sure we can actually use the table etc.
            if (table == null || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning() || thing.IsForbidden(pawn) || (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)))
            {
                return null;
            }
            Log.DebugOnce("checking thing=" + thing.Label + " for pawn=" + pawn.Label + " managing");
            // only do this job, if there is a buffer on the map
            var settlementResources = thing.Map?.GetComponent<MapComponent_SettlementResources>();
            if (settlementResources == null)
            {
                return null;
            }
            /* always allowed to manage since it is used for other purposes from now on too
            if (settlementResources.SettlementLevel < 6) // only active startgin at level 6
            {
                return null;
            }*/
            var assignableComp = thing.TryGetComp<CompAssignableToPawn>();
            // make sure we either have a unassigned table or a table assigned for all
            if (!forced && assignableComp != null && assignableComp.AssignedPawnsForReading != null && !assignableComp.AssignedPawnsForReading.Contains(pawn) && assignableComp.AssignedPawnsForReading.Count > 0)
            {
                return null;
            }
            var howMuchRemaining = (float)(settlementResources.ManagementBuffer_max - settlementResources.ManagementBuffer_current) / settlementResources.ManagementBuffer_max;
            if (!forced && howMuchRemaining < 0.15)
            {
                return null;
            }
            // some hours (> 3) of work left?
            if (!forced && settlementResources.ManagementBuffer_max - settlementResources.ManagementBuffer_current < 10000)
            {
                return null;
            }
            // only manage if the recreation is high enough -> > 40%
            if (!forced && pawn.needs.joy != null && pawn.needs.joy.CurLevelPercentage < 0.4f)
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
