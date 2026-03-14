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
    class Building_PersonalizedWorkTable : Building_WorkTable
    {

        public CompAssignableToPawn CompAssignableToPawn => GetComp<CompAssignableToPawn>();

        public override void PostMake()
        {
            base.PostMake();
            Log.Debug("Building_PersonalizedWorkTable.PostMake(): personalized fabrication bench was built");
        }

        public override void DrawGUIOverlay()
        {
            Log.DebugOnce("Building_PersonalizedWorkTable.DrawGUIOverlay() is getting called...");
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom != 0) // only render on close zoom
            {
                return;
            }
            if (CompAssignableToPawn == null || CompAssignableToPawn.AssignedPawnsForReading == null)
            {
                Log.Error("Building_PersonalizedWorkTable: missing CompAssignableToPawn!");
                return;
            }
            Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
            if (CompAssignableToPawn.AssignedPawnsForReading.Count == 0)
            {
                GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
            } 
            else
            {
                GenMapUI.DrawThingLabel(this, CompAssignableToPawn.AssignedPawnsForReading[0].LabelShort, defaultThingLabelColor);
            }
            
        }
    }
}
