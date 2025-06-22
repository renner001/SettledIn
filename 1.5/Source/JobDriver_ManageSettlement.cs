using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DanielRenner.SettledIn
{
    class JobDriver_ManageSettlement : JobDriver
    {
        private const TargetIndex TableIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Building_TableSettlementOffice table = job.GetTarget(TableIndex).Thing as Building_TableSettlementOffice;
            return pawn.Reserve(table, job, 1, -1, null, errorOnFailed) 
                && (!table.def.hasInteractionCell || pawn.ReserveSittableOrSpot(table.InteractionCell, this.job, errorOnFailed));
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TableIndex);
#if DEBUG
            yield return Toils_General.DoAtomic(delegate
            {
                Log.Debug("first toil of managing the settlement called");
            });
#endif
            yield return Toils_Goto.GotoThing(TableIndex, PathEndMode.InteractionCell);
            Toil manageSettlement = ToilMaker.MakeToil("MakeNewToils");
            manageSettlement.tickAction = delegate
            {
                Pawn actor = manageSettlement.actor;
                var officeTable = this.TargetThingA;
                var settlementResources = officeTable.Map.GetComponent<MapComponent_SettlementResources>();
                settlementResources.ManagementBuffer_current += 66;
                actor.skills.Learn(SkillDefOf.Intellectual, 0.1f, false);
                if (actor.needs.joy != null)
                    actor.needs.joy.CurLevelPercentage -= 0.00002f;
                //float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true, -1);
                //num *= this.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);
                //Find.ResearchManager.ResearchPerformed(num, actor);
                //actor.skills.Learn(SkillDefOf.Intellectual, 0.1f, false);
                actor.GainComfortFromCellIfPossible(true);
            };
            // stop if the buffer is full
            manageSettlement.FailOn(() =>
            {
                var officeTable = this.TargetThingA;
                var settlementResources = officeTable.Map.GetComponent<MapComponent_SettlementResources>();
                return settlementResources.ManagementBuffer_max <= settlementResources.ManagementBuffer_current;
            });
            // stop if the joy falls below 10%
            manageSettlement.FailOn(() =>
            {
                Pawn actor = manageSettlement.actor;
                if (actor.needs.joy != null)
                    return actor.needs.joy.CurLevelPercentage <= 0.1f;
                return false;
            });
            //manageSettlement.FailOn(() => this.Project == null);
            //manageSettlement.FailOn(() => !this.Project.CanBeResearchedAt(this.ResearchBench, false));
            manageSettlement.FailOnCannotTouch(TableIndex, PathEndMode.InteractionCell);
            manageSettlement.WithEffect(EffecterDefOf.Research, TableIndex, null);
            manageSettlement.WithProgressBar(TableIndex, delegate
            {
                var curInterval = Find.TickManager.TicksGame % 1000;
                var percentProgress = (float)curInterval / 1000;
                return percentProgress;
            }, false, -0.5f, false);
            manageSettlement.defaultCompleteMode = ToilCompleteMode.Delay;
            manageSettlement.defaultDuration = 4000;
            manageSettlement.activeSkill = () => SkillDefOf.Intellectual;
            yield return manageSettlement;
            yield return Toils_General.Wait(2, TargetIndex.None);
#if DEBUG
            yield return Toils_General.DoAtomic(delegate
            {
                Log.Debug("last toil of managing the settlement called");
            });
#endif
            yield break;
        }

    }
}
