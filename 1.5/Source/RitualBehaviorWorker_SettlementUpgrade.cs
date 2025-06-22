using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;

namespace DanielRenner.SettledIn
{
    public class RitualBehaviorWorker_SettlementUpgrade : RitualBehaviorWorker_Speech //RitualBehaviorWorker
    {
        public RitualBehaviorWorker_SettlementUpgrade()
        {
        }

        public RitualBehaviorWorker_SettlementUpgrade(RitualBehaviorDef def)
            : base(def)
        {
        }

        protected override void PostExecute(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            /*
            Pawn pawn = assignments.AssignedPawns("speaker").First<Pawn>();
            Find.LetterStack.ReceiveLetter(this.def.letterTitle.Formatted(ritual.Named("RITUAL")), this.def.letterText.Formatted(pawn.Named("SPEAKER"), ritual.Named("RITUAL"), ritual.ideo.MemberNamePlural.Named("IDEOMEMBERS")) + "\n\n" + ritual.outcomeEffect.ExtraAlertParagraph(ritual), LetterDefOf.PositiveEvent, target, null, null, null, null);
            */
        }

        public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            Building_SettlementCenter targetCenter = target.HasThing ? target.Thing as Building_SettlementCenter : null;
            if (targetCenter == null)
            {
                return "SettledIn.TargetIsNoSettlementCenter".Translate();
            }
            var map = targetCenter.Map;
            // find the settlement component of the target map and check if it is currently upgradable
            // cancel if not
            var settlementResources = map.GetComponent<MapComponent_SettlementResources>();
            if (settlementResources.SettlementLevel >= SettlementLevelUtility.MaxLevel)
            {
                return "SettledIn.AlreadyMaxLevel".Translate();
            }
            string upgradeRequirements;
            var allegibleForUpdate = SettlementLevelUtility.CheckRequirements(settlementResources.SettlementLevel + 1, map, out upgradeRequirements);
            if (!allegibleForUpdate)
            {
                return "SettledIn.RequirementsNotMet".Translate() + upgradeRequirements;
            }
            // else do the basic checks of a speech
            return base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
        }
        /*
        public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            Precept_Role precept_Role = ritual.ideo.RolesListForReading.FirstOrDefault((Precept_Role r) => r.def == PreceptDefOf.IdeoRole_Leader);
            if (precept_Role == null)
            {
                return null;
            }
            if (precept_Role.ChosenPawnSingle() == null)
            {
                return "CantStartRitualRoleNotAssigned".Translate(precept_Role.LabelCap);
            }
            return base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
        }

        protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            Pawn pawn = assignments.AssignedPawns("speaker").First<Pawn>();
            return new LordJob_Joinable_Speech(target, pawn, ritual, this.def.stages, assignments, false);
        }*/
    }
}
