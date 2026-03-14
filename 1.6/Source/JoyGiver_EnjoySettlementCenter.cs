using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using System.Diagnostics;

namespace DanielRenner.SettledIn
{
    public class JoyGiver_EnjoySettlementCenter : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Log.DebugOnce("at least JoyGiver_EnjoySettlementCenter.TryGiveJob() is getting called");
            foreach (var artBuilding in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Art))
            {
                if (artBuilding is Building_SettlementCenter && artBuilding.Faction == Faction.OfPlayer)
                {
                    Log.DebugOnce("JoyGiver_EnjoySettlementCenter.TryGiveJob(): settlement center can be found");
                    if (pawn.CanReserveAndReach(artBuilding.Position, PathEndMode.Touch, Danger.None, this.def.jobDef.joyMaxParticipants))
                    {
                        Log.Debug("JoyGiver_EnjoySettlementCenter.TryGiveJob(): settlement center can be reserved");
                        return JobMaker.MakeJob(this.def.jobDef, artBuilding);
                    }
                    else
                    {
                        Log.DebugOnce("JoyGiver_EnjoySettlementCenter.TryGiveJob(): could not reserve building=" + artBuilding.ToString() + ", pawn=" + pawn.ToString());
                    }
                }
            }
            return null;
        }

    }
}
