using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace DanielRenner.SettledIn
{
    public class CompProperties_BedSettlementInfo : CompProperties
    {
        public CompProperties_BedSettlementInfo()
        {
            this.compClass = typeof(Comp_BedSettlementInfo);
        }
    }

    public class Comp_BedSettlementInfo : ThingComp
    {
        public CompProperties_BedSettlementInfo Props => (CompProperties_BedSettlementInfo)props;

        public override string CompInspectStringExtra()
        {
            var bed = this.parent as Building_Bed;
            var map = parent?.Map;
            var settledInComponent = map?.GetComponent<MapComponent_SettlementResources>();
            if (bed == null || settledInComponent == null)
                return String.Empty;

            var assignedPawns = bed.GetAssignedPawns();
            if (assignedPawns == null || assignedPawns.Count() == 0)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var pawn in assignedPawns)
            {
                if (!settledInComponent.HasUnlikedNeighbours.Contains(pawn))
                    continue;

                if (!settledInComponent.UnlikedNeighborsSleepingNearby.ContainsKey(pawn))
                {
                    Log.WarningOnce($"Inconsistent lists for SettledIn MapComponent: HasUnlikedNeighbours and UnlikedNeighborsSleepingNearby are not consistent for pawn {pawn}. This should not happen - but the error has been caught and your play experience will be fine.");
                    continue;
                }

                if (sb.Length != 0)
                    sb.AppendLine(); // have new lines between different pawns
                sb.Append($"{pawn}: ");
                sb.Append("DanielRenner.SettledIn.DislikesCloseBeds".Translate());
                var neighborsTextList = String.Join(", ", settledInComponent.UnlikedNeighborsSleepingNearby[pawn]);
                sb.Append(neighborsTextList);
            }
            return sb.ToString();
        }

        // Red for social friction
        private static readonly Material HateLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.red);
        // Cyan/Blue for work commute
        private static readonly Material WorkLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.cyan);

        public override void PostDrawExtraSelectionOverlays()
        {
            Log.DebugOnce("at least Comp_BedSettlementInfo.PostDrawExtraSelectionOverlays() is being called");

            // don't draw anything if the data is corrupted or the settlement statistics have never been generated
            var bed = this.parent as Building_Bed;
            var map = parent?.Map;
            var settledInComponent = map?.GetComponent<MapComponent_SettlementResources>();
            if (bed == null || settledInComponent == null)
                return;
            var assignedPawns = bed.GetAssignedPawns();
            // if there are no pawns assigned, we have nothign to render
            if (assignedPawns == null || assignedPawns.Count() == 0)
                return;

            // go through once for rendering the hate lines
            foreach (var pawn in assignedPawns)
            {
                if (!settledInComponent.UnlikedNeighborsSleepingNearby.ContainsKey(pawn))
                    continue;

                var unlikedPawnsSleepingNearby = settledInComponent.UnlikedNeighborsSleepingNearby[pawn];
                foreach (var unlikedPawnSleepingNearby in unlikedPawnsSleepingNearby)
                {
                    if (!settledInComponent.PawnBeds.ContainsKey(unlikedPawnSleepingNearby))
                    {
                        Log.DebugOnce($"pawn {unlikedPawnSleepingNearby} has no entry in PawnBeds cache?");
                        continue;
                    }
                    var enemyBed = settledInComponent.PawnBeds[unlikedPawnSleepingNearby];
                    if (enemyBed == null)
                        continue;

                    Log.DebugOnce($"drawing enemy connector between {bed} and {enemyBed}");
                    GenDraw.DrawLineBetween(bed.TrueCenter(), enemyBed.TrueCenter(), AltitudeLayer.MetaOverlays.AltitudeFor(), HateLineMat, 0.2f);
                }
            }

            // go through again renderign the lines to assigned work benches
            foreach (var pawn in assignedPawns)
            {
                // We look for buildings where this pawn is the "Assigned" worker.
                // This covers Thrones, Meditation Spots, and benches with specific assignments.
                var workBuildings = map.listerBuildings.allBuildingsColonist
                    .Where(b => {
                        var compAssignable = b?.GetComp<CompAssignableToPawn>();
                        if (compAssignable == null)
                            return false;
                        return compAssignable.AssignedPawns.Contains(pawn);
                        }).ToList();

                foreach (var workBuilding in workBuildings)
                {
                    // Draw a dashed or colored line to show the commute path
                    GenDraw.DrawLineBetween(bed.TrueCenter(), workBuilding.TrueCenter(), AltitudeLayer.MetaOverlays.AltitudeFor(), WorkLineMat, 0.2f);
                }
            }
        }
    }
}
