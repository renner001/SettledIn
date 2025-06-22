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
        public static string GetRoomRoleLabel(Room room)
        {
            Type environmentStatsDrawerType = typeof(EnvironmentStatsDrawer);
            MethodInfo methodType = environmentStatsDrawerType.GetMethod("GetRoomRoleLabel", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod);
            if (methodType != null && room != null)
            {
                var label = methodType.Invoke(null, new[] { room }) as string;
                return label;
            }
            return "err: failed to load name";
        }
    }
}
