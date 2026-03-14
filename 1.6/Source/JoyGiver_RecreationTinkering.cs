using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace DanielRenner.SettledIn
{
    public class JoyGiver_RecreationTinkering : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var marker = FindClosestPersonalWorkbench(pawn);
            if (marker == null)
                return null;

            return JobMaker.MakeJob(
                DefOfs_SettledIn.RecreationTinkering,
                marker
            );
        }

        private Thing FindClosestPersonalWorkbench(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver), 
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                9999, 
                (thing) => {
                    if (!thing.HasComp<CompAssignableToPawn_ProductionBuilding>())
                        return false;
                    var comp = thing.TryGetComp<CompAssignableToPawn_ProductionBuilding>();
                    if (!comp.AssignedPawns.Contains(pawn))
                        return false;
                    var billGiver = thing as IBillGiver;
                    if (billGiver == null)
                        return false;
                    if (billGiver == null || !pawn.CanReserve(thing, 1, -1, null, false) || thing.IsBurning())
                        return false;
                    if (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, thing, false))
                        return false;
                    return true;
                }
            );
        }
    }
}
