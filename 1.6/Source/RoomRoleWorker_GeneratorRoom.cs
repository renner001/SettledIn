using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class RoomRoleWorker_Generator_Room : RoomRoleWorker_DynamicBase
    {
        public RoomRoleWorker_Generator_Room() : base(DefOfs_SettledIn.GeneratorRoom.defName)
        {
        }

        protected override void specificThingScoreOverride(Thing thing, ref float score)
        {
            if (thing.def.EverTransmitsPower)
            {
                var powerComp = thing.def.GetCompProperties<CompProperties_Power>();
                if (powerComp != null && powerComp.PowerConsumption < -10)
                {
                    score = 20000;
                }
            }
        }
    }
}
