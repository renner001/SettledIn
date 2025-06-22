using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public static class RoomNameUtility
    {
        private static Type _labelsOnFloorGetRoomLabelRef = null;

        public static string GetRoomRoleLabel(Room room)
        {
            return room.GetRoomRoleLabel();
        }
    }
}
