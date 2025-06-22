using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public class Alert_OptimizeSettlement : Alert
    {

        private List<Pawn> allHasUnlikedNeighbours = new List<Pawn>();
        private List<Pawn> allLongCommuters = new List<Pawn>();
        private List<Pawn> allWantsWorkbench = new List<Pawn>();
        private List<Pawn> allWantsFewerTasks = new List<Pawn>();
        private List<Pawn> allWantsMoreRecreation = new List<Pawn>();
        private List<Pawn> allCulprits = new List<Pawn>();

        private void updateLists()
        {
            allHasUnlikedNeighbours.Clear();
            allLongCommuters.Clear();
            allWantsWorkbench.Clear();
            allWantsFewerTasks.Clear();
            allWantsMoreRecreation.Clear();
            allCulprits.Clear();

            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome)
                {
                    var settlementResources = maps[i].GetComponent<MapComponent_SettlementResources>();
                    if (settlementResources != null)
                    {
                        allHasUnlikedNeighbours.AddRange(settlementResources.HasUnlikedNeighbours);
                        allLongCommuters.AddRange(settlementResources.LongCommuters);
                        allWantsWorkbench.AddRange(settlementResources.WantsWorkbench);
                        allWantsFewerTasks.AddRange(settlementResources.WantsFewerTasks);
                        allWantsMoreRecreation.AddRange(settlementResources.WantsMoreRecreation);
                    }
                }
            }

            allCulprits.AddRange(allHasUnlikedNeighbours);
            allCulprits.AddRange(allWantsWorkbench);
            allCulprits.AddRange(allLongCommuters);
            allCulprits.AddRange(allWantsFewerTasks);
            allCulprits.AddRange(allWantsMoreRecreation);
        }

        // Token: 0x0600A016 RID: 40982 RVA: 0x0035D910 File Offset: 0x0035BB10
        public override string GetLabel()
        {
            return "DanielRenner.SettledIn.SettlementAlertLabel".Translate();
        }

        // Token: 0x0600A017 RID: 40983 RVA: 0x0035D938 File Offset: 0x0035BB38
        public override TaggedString GetExplanation()
        {
            this.sb.Length = 0;
            sb.AppendLine("DanielRenner.SettledIn.SettlementAlertExplanationHead".Translate());
            if (allHasUnlikedNeighbours.Count > 0)
            {
                sb.AppendLine("DanielRenner.SettledIn.SettlementAlertExplanationHasUnlikedNeighbours".Translate());
                foreach (Pawn pawn in allHasUnlikedNeighbours)
                {
                    sb.AppendLine("  - " + pawn.NameShortColored.Resolve());
                }
            }
            if (allWantsWorkbench.Count > 0)
            {
                sb.AppendLine("DanielRenner.SettledIn.SettlementAlertExplanationWantsWorkbench".Translate());
                foreach (Pawn pawn in allWantsWorkbench)
                {
                    sb.AppendLine("  - " + pawn.NameShortColored.Resolve());
                }
            }
            if (allLongCommuters.Count > 0)
            {
                sb.AppendLine("DanielRenner.SettledIn.SettlementAlertExplanationLongCommuters".Translate());
                foreach (Pawn pawn in allLongCommuters)
                {
                    sb.AppendLine("  - " + pawn.NameShortColored.Resolve());
                }
            }
            if (allWantsFewerTasks.Count > 0)
            {
                sb.AppendLine("DanielRenner.SettledIn.SettlementAlertExplanationWantsFewerTasks".Translate());
                foreach (Pawn pawn in allWantsFewerTasks)
                {
                    sb.AppendLine("  - " + pawn.NameShortColored.Resolve());
                }
            }
            if (allWantsMoreRecreation.Count > 0)
            {
                sb.AppendLine("DanielRenner.SettledIn.SettlementAlertExplanationWantsMoreRecreation".Translate());
                foreach (Pawn pawn in allWantsMoreRecreation)
                {
                    sb.AppendLine("  - " + pawn.NameShortColored.Resolve());
                }
            }

            return sb.ToString();
        }

        // Token: 0x0600A018 RID: 40984 RVA: 0x0035D9D4 File Offset: 0x0035BBD4
        public override AlertReport GetReport()
        {
            if (GenDate.DaysPassed < 1)
            {
                return false;
            }

            updateLists();

            if (allCulprits.Count == 0) 
                return false;

            return AlertReport.CulpritsAre(allCulprits);
        }

        public const int MinDaysPassed = 1;

        private StringBuilder sb = new StringBuilder();
    }
}
