using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class RoomRoleWorker_TailorShop : RoomRoleWorker_DynamicBase
    {
        public RoomRoleWorker_TailorShop() : base(DefOfs_SettledIn.TailorShop.defName)
        {
        }

        protected override void specificThingScoreOverride(Thing thing, ref float score)
        {
            if (thing.def.defName.Contains("TailoringBench"))
            {
                score = 20000;
            }
            if (thing.def.defName.Contains("Loom")) 
            {
                score = 20000;
            }
        }
    }
}
