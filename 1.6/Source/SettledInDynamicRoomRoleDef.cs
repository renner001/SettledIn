using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public  class SettledInDynamicRoomRoleDef : RoomRoleDef
    {
        public List<string> perfectDefs;
        public List<string> positiveDefs;
        public List<string> negativeDefs;
        public List<string> forbiddenDefs;
    }
}
