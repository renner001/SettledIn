using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    internal class PlaceWorker_InSpecificRoom : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            Log.DebugOnce("at least PlaceWorker_InSpecificRoom.AllowsPlacing() is getting called...");
            if (!(def is ThingDef))
            {
                Log.WarningOnce("used PlaceWorker_InSpecificRoom on non-thing=" + def.ToString() + " - this will never restrict it's placement.");
                return true;
            }

            var adjacencyInformationInDef = def.GetModExtension<AdjacencyProperties>();
            if (adjacencyInformationInDef != null)
            {
                Room room = center.GetRoom(map);
                if (room == null || !adjacencyInformationInDef.requiredRoomRole.Contains(room.Role.defName))
                {
                    return "Wrong room type";
                }
            }
            return true;
        }

    }
}
