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
    public class JobDriver_RecreationTinkering : JobDriver
    {
        protected Thing Marker =>
            job.targetA.Thing as Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        bool reservationsFailed = false;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.DoAtomic(delegate
            {
                Log.Debug("first toil of recreation tinkering the settlement called");
                if (!this.pawn.Reserve(Marker, this.job, 1, -1, null, false, false))
                {
                    reservationsFailed = true;
                }
                if (Marker != null && Marker.def.hasInteractionCell && !this.pawn.ReserveSittableOrSpot(Marker.InteractionCell, this.job, false))
                {
                    reservationsFailed = true;
                }
            }).FailOn(() => { return reservationsFailed; });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            var work = new Toil
            {
                tickAction = () =>
                {
                    pawn.needs.joy.GainJoy(0.0012f, DefOfs_SettledIn.Productive);
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 1200
            };
            work.WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A);
            work.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return work;
        }
    }
}
