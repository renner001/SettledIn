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
    public class Comp_AdjustableFacility : CompFacility
    {
        private float currentRange;
        public float CurrentRange
        {
            get { return currentRange; }
            set { currentRange = value; Notify_ThingChanged(); }
        }

        public new CompProperties_AdjustableFacility Props => (CompProperties_AdjustableFacility)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                CurrentRange = Props.maxDistance;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentRange, "currentRange", Props.maxDistance);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
                yield return g;

            yield return new Command_Action
            {
                defaultLabel = $"Set range: {CurrentRange:F1}",
                defaultDesc = "Adjust the maximum link range for this facility.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/RangeIcon", true), // Replace with your icon
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_Slider(
                        "Adjust Link Range",
                        (val) => CurrentRange = val,
                        Props.maxDistanceFrom, // min
                        Props.maxDistance, // max
                        CurrentRange // start
                    ));
                }
            };
        }
    }
}
