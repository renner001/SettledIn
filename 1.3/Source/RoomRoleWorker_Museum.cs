using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class RoomRoleWorker_Museum : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            Log.Debug("RoomRoleWorker_Museum.GetScore() is getting called");
            return 0;
        }
    }
}
