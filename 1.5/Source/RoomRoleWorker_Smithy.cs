using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class RoomRoleWorker_Smithy : RoomRoleWorker_DynamicBase
    {
        public RoomRoleWorker_Smithy() : base(DefOfs_SettledIn.Smithy.defName)
        {
        }

        protected override void specificThingScoreOverride(Thing thing, ref float score)
        {
            if (thing.def.defName.Contains("Smithy"))
            {
                score = 20000;
            }
        }
    }
}
