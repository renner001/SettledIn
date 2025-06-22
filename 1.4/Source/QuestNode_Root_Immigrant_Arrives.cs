using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    internal class QuestNode_Root_Immigrant_Arrives : QuestNode_Root_WandererJoin_WalkIn
    {

        private string signalAccept;
        private string signalReject;

        protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
        {
            this.signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
            this.signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
            quest.Signal(this.signalAccept, delegate
            {
                quest.SetFaction(Gen.YieldSingle<Pawn>(pawn), Faction.OfPlayer, null);
                quest.PawnsArrive(Gen.YieldSingle<Pawn>(pawn), null, map.Parent, null, false, null, null, null, null, null, false, false, true);
                quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, false);
            }, null, QuestPart.SignalListenMode.OngoingOnly);
            quest.Signal(this.signalReject, delegate
            {
                quest.GiveDiedOrDownedThoughts(pawn, PawnDiedOrDownedThoughtsKind.DeniedJoining, null);
                quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, false);
            }, null, QuestPart.SignalListenMode.OngoingOnly);
        }

        public override void SendLetter(Quest quest, Pawn pawn)
        {
            TaggedString questLabel = "SettledIn.ImmigrantMessage".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true); // "LetterLabelWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            TaggedString questDescription = "SettledIn.ImmigrationDescription".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref questDescription, ref questLabel, pawn);
            /*if (pawn.DevelopmentalStage.Juvenile())
            {
                string text = (pawn.ageTracker.AgeBiologicalYears * 3600000).ToStringTicksToPeriod(true, false, true, true, false);
                questDescription += "\n\n" + "RefugeePodCrash_Child".Translate(pawn.Named("PAWN"), text.Named("AGE"));
            }*/
            ChoiceLetter_AcceptJoiner choiceLetter_AcceptJoiner = (ChoiceLetter_AcceptJoiner)LetterMaker.MakeLetter(questLabel, questDescription, LetterDefOf.AcceptJoiner, null, quest);
            choiceLetter_AcceptJoiner.signalAccept = this.signalAccept;
            choiceLetter_AcceptJoiner.signalReject = this.signalReject;
            choiceLetter_AcceptJoiner.quest = quest;
            choiceLetter_AcceptJoiner.StartTimeout(60000);
            Find.LetterStack.ReceiveLetter(choiceLetter_AcceptJoiner, null);
        }

    }
}
