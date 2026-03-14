using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    /**
     * Base class for all room roles that use the dynamic concept introduced with this mod. Since Rimworld does not allow for accessing the def from within the workers, sadly for each RoleRoleDef 
     * also a worker class must be created. By using this base class, only a defName must be given to enable full functionality
     **/
    public abstract class RoomRoleWorker_DynamicBase : RoomRoleWorker
    {
        public RoomRoleWorker_DynamicBase(string DefName)
        {
            this.DefName = DefName;
        }

        protected string DefName = "";
        public override float GetScore(Room room)
        {
            var roomRoleDef = DefDatabase<SettledInDynamicRoomRoleDef>.GetNamed(DefName);
            Log.DebugOnce("RoomRoleWorker_DynamicBase: Known Defs: Perfect=" + String.Join(";", roomRoleDef.perfectDefs) + " Positive=" + String.Join(";", roomRoleDef.positiveDefs) + ", Negative=" + String.Join(";", roomRoleDef.negativeDefs) + ", Forbidden=" + String.Join(";", roomRoleDef.forbiddenDefs));

            var score = 0f;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                var thing = containedAndAdjacentThings[i];
                var thisThingScore = 0f;
                // in case of forbidden things -> 0
                if (roomRoleDef.forbiddenDefs.Contains(thing.def.defName))
                {
                    return 0f;
                }
                if (thing is Building_Bed)
                {
                    return 0f;
                }
                if (roomRoleDef.perfectDefs.Contains(thing.def.defName))
                {
                    thisThingScore += 20000;
                } 
                if (roomRoleDef.positiveDefs.Contains(thing.def.defName))
                {
                    thisThingScore += 1000;
                }
                if (roomRoleDef.negativeDefs.Contains(thing.def.defName))
                {
                    thisThingScore -= 2000;
                }
                specificThingScoreOverride(thing, ref thisThingScore);
                score += thisThingScore;
            }
            return score;
        }

        protected virtual void specificThingScoreOverride(Thing thing, ref float score)
        {

        }
    }
}
