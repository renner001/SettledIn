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
    public class JoyGiver_RecreationConstruction : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var marker = FindClosestMarker(pawn);
            if (marker == null)
                return null;

            return JobMaker.MakeJob(
                DefOfs_SettledIn.RecreationConstruction,
                marker
            );
        }

        private Building_FrameRecreationConstruction FindClosestMarker(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), 
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                9999, 
                (thing) => { return thing is Building_FrameRecreationConstruction && (thing as Building_FrameRecreationConstruction).IsUpgrading; }
            ) as Building_FrameRecreationConstruction;
        }
    }
}
