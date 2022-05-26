using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class CompAssignableToPawn_ProductionBuilding : CompAssignableToPawn
    {
        public CompAssignableToPawn_ProductionBuilding()
        {
            Log.DebugOnce("CompAssignableToPawn_ProductionBuilding.CompAssignableToPawn_ProductionBuilding() at least coms are gettign created...");
        }

        public override void ForceAddPawn(Pawn pawn)
        {
            Log.DebugOnce("CompAssignableToPawn_ProductionBuilding.ForceAddPawn() trying to add pawn " + pawn);
            if (assignedPawns.Contains(pawn))
            {
                return;
            }
            while (assignedPawns.Count > 0)
            {
                ForceRemovePawn(assignedPawns[0]);
            }
            base.ForceAddPawn(pawn);
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            Log.DebugOnce("CompAssignableToPawn_ProductionBuilding.TryAssignPawn() trying to assign pawn " + pawn);
            if (assignedPawns.Contains(pawn))
            {
                return;
            }
            int maxTryCount = 10;
            while (assignedPawns.Count > 0 && --maxTryCount > 0)
            {
                TryUnassignPawn(assignedPawns[0]);
            }
            base.TryAssignPawn(pawn);
        }
    }
}
