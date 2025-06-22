using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{
    class CompAssignableToPawn_ProductionBuilding : CompAssignableToPawn
    {
        public CompAssignableToPawn_ProductionBuilding()
        {
            Log.DebugOnce("CompAssignableToPawn_ProductionBuilding.CompAssignableToPawn_ProductionBuilding() at least coms are getting created...");
        }
        
        public override void ForceAddPawn(Pawn pawn)
        {
            Log.Debug("CompAssignableToPawn_ProductionBuilding.ForceAddPawn() trying to add pawn " + pawn);
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
            Log.Debug("CompAssignableToPawn_ProductionBuilding.TryAssignPawn() called to assign pawn " + pawn);
            if (assignedPawns.Contains(pawn))
            {
                return;
            }
            int maxTryCount = 10;
            while (assignedPawns.Count > 0 && --maxTryCount > 0)
            {
                Log.Debug("trying to unassign pawn " + assignedPawns[0]);
                TryUnassignPawn(assignedPawns[0]);
            }
            Log.Debug("trying to assign pawn " + pawn);
            base.TryAssignPawn(pawn);
            var bench = parent as IBillGiver;
            if (bench != null)
            {
                Log.Debug("checking for unfinished items to destroy");
                foreach (var bill in bench.BillStack)
                {
                    if (bill is Bill_ProductionWithUft bill_ProductionWithUft && bill_ProductionWithUft.BoundUft != null)
                    {
                        Log.Debug("refunding " + bill_ProductionWithUft.BoundUft + " for unfinished items");
                        bill_ProductionWithUft.BoundUft.Destroy(DestroyMode.Cancel); // alternatively just unbind it by assigning null?
                        Messages.Message("Onwership has changed: Unfinished bills are refunded.", MessageTypeDefOf.CautionInput);
                    }
                }
            }
            Log.Debug("successfully assigned pawn " + pawn);
        }
        
        public override void DrawGUIOverlay()
        {
            Log.DebugOnce("CompAssignableToPawn_ProductionBuilding.DrawGUIOverlay() is getting called... ");
            base.DrawGUIOverlay();
        }
        
    }
}
