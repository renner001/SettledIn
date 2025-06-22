using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DanielRenner.SettledIn
{
    class JobDriver_EnjoySettlementCenter : JobDriver
    {
        private const TargetIndex SettlementCenterBuildingIndex = TargetIndex.A;
        private List<string> reportStrings;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Building_SettlementCenter settlementCenterBuilding = job.GetTarget(SettlementCenterBuildingIndex).Thing as Building_SettlementCenter;
            return pawn.Reserve(settlementCenterBuilding, job, this.job.def.joyMaxParticipants, 0);
        }

        public override string GetReport()
        {
            if (reportStrings == null || reportStrings.Count < 1)
            {
                reportStrings = new List<string>();
                reportStrings.Add("DanielRenner.SettledIn.SettlementCenterReportChiseling".Translate());
                reportStrings.Add("DanielRenner.SettledIn.SettlementCenterReportStanding".Translate());
                reportStrings.Add("DanielRenner.SettledIn.SettlementCenterReportLookingAt".Translate());
                reportStrings.Shuffle();
            }
            if (this.pawn.CurJob != this.job || this.pawn.Position != this.job.GetTarget(TargetIndex.A).Cell)
            {
                return base.GetReport();
            }
            if (pawn.IsHashIntervalTick(1500))
            {
                reportStrings.Shuffle();
            }
            return reportStrings[0];
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(SettlementCenterBuildingIndex);
#if DEBUG
            yield return Toils_General.DoAtomic(delegate
            {
                Log.Debug("first toil of enjoying the settlement center");
            });
#endif
            yield return Toils_Goto.GotoThing(SettlementCenterBuildingIndex, PathEndMode.Touch);
            Toil stareAtSettlementCenter = ToilMaker.MakeToil("MakeNewToils");
            stareAtSettlementCenter.tickAction = delegate
            {
                //Pawn actor = stareAtSettlementCenter.actor;
                //actor.rotationTracker.FaceCell(base.TargetA.Cell);
                pawn.GainComfortFromCellIfPossible(false);
                JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f, (Building)base.TargetThingA);
            };
            stareAtSettlementCenter.AddFinishAction(delegate
            {
                //Pawn actor = stareAtSettlementCenter.actor;
                JoyUtility.TryGainRecRoomThought(pawn);
            });
            stareAtSettlementCenter.FailOnCannotTouch(SettlementCenterBuildingIndex, PathEndMode.Touch);
            //stareAtSettlementCenter.WithEffect(EffecterDefOf.ConstructMetal, SettlementCenterBuildingIndex, null);
            /*stareAtSettlementCenter.WithProgressBar(SettlementCenterBuildingIndex, delegate
            {
                var curInterval = Find.TickManager.TicksGame % 1000;
                var percentProgress = curInterval / 1000;
                return percentProgress;
            }, false, -0.5f, false);*/
            stareAtSettlementCenter.defaultCompleteMode = ToilCompleteMode.Delay;
            stareAtSettlementCenter.defaultDuration = this.job.def.joyDuration;
            //stareAtSettlementCenter.activeSkill = () => null;
            yield return stareAtSettlementCenter;
            yield return Toils_General.Wait(2, TargetIndex.None);
#if DEBUG
            yield return Toils_General.DoAtomic(delegate
            {
                Log.Debug("last toil of enjoying the settlement center called");
            });
#endif
            yield break;
        }

        public override object[] TaleParameters()
        {
            return new object[]
            {
                this.pawn,
                base.TargetA.Thing.def
            };
        }

    }
}
