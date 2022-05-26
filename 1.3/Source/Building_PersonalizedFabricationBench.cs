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
    class Building_PersonalizedFabricationBench : Building_WorkTable
    {

        public CompAssignableToPawn CompAssignableToPawn => GetComp<CompAssignableToPawn>();

        public override void PostMake()
        {
            base.PostMake();
            Log.Debug("Building_PersonalizedFabricationBench.PostMake(): personalized fabrication bench was built");
        }

        public override void DrawGUIOverlay()
        {
            Log.Debug("Building_PersonalizedFabricationBench.DrawGUIOverlay() called");
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom != 0) // only render on close zoom
            {
                return;
            }
            if (CompAssignableToPawn == null || CompAssignableToPawn.AssignedPawnsForReading == null)
            {
                Log.Error("Building_PersonalizedFabricationBench: missing CompAssignableToPawn!");
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
