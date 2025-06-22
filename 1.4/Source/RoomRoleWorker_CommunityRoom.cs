using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class RoomRoleWorker_CommunityRoom : RoomRoleWorker_DynamicBase
    {
        public RoomRoleWorker_CommunityRoom() : base(DefOfs_SettledIn.CommunityRoom.defName)
        {
        }

        public override float GetScore(Room room)
        {
            var recScore = RoomRoleDefOf.RecRoom.Worker.GetScore(room);
            var diningScore = RoomRoleDefOf.DiningRoom.Worker.GetScore(room);

            var summary = recScore + diningScore;
            // if we have a room that contains both relatively equally... so 1 to 4 -> then we have a community room
            if (recScore > 0.2f * summary && recScore < 0.8f * summary)
            {
                return summary;
            }
            return 0f;

        }
    }
}
