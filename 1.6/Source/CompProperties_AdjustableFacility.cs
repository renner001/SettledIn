using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public class CompProperties_AdjustableFacility : CompProperties_Facility
    {
        public float maxDistanceFrom = 15f;
        public HediffDef appliedHediff;

        public CompProperties_AdjustableFacility()
        {
            compClass = typeof(Comp_AdjustableFacility);
        }
    }
}
