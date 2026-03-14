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
    public class JobDriver_RecreationConstruction : JobDriver
    {
        protected Building_FrameRecreationConstruction Marker =>
            job.targetA.Thing as Building_FrameRecreationConstruction;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            var work = new Toil
            {
                tickAction = () =>
                {
                    if (Marker == null || Marker.Destroyed)
                    {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }

                    pawn.needs.joy.GainJoy(0.0008f, DefOfs_SettledIn.Productive);
                    Marker.DoRecreationWork(pawn);
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 1200
            };

            work.WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A);

            yield return work;
        }
    }
}
