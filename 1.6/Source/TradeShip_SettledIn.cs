using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public class TradeShip_SettledIn : TradeShip
    {
        public TradeShip_SettledIn(): base() { }
        public TradeShip_SettledIn(TraderKindDef def, Faction faction = null) : base(def, faction) { }

        // saving and loading the map data
        public override void ExposeData()
        {
            base.ExposeData();
        }


        // carefully remember that we also need a pacth Patch_TradeShip_GiveSoldThingToPlayer to ensure this method is actually called
        public new void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Log.DebugOnce("at least TradeShip_SettledIn.GiveSoldThingToPlayer() is getting called");
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            Pawn pawn = thing as Pawn;
            // todo: remove prisoners that have been bought
            //if (pawn != null)
            //{
            //    this.soldPrisoners.Remove(pawn);
            //}
            var settlementCenter = Map.listerThings.ThingsOfDef(DefOfs_SettledIn.SettlementCenter).FirstOrDefault();
            // place the items next to the settlement center if it can be found - else we run the default drop placing
            var placed = false;
            if (settlementCenter != null)
            {
                Log.Debug($"placing {thing} next to {settlementCenter.InteractionCell}");
                placed = GenPlace.TryPlaceThing(thing, settlementCenter.InteractionCell, this.Map, ThingPlaceMode.Near);
            }
            if (!placed)
            {
                Log.Warning($"drop-podding {thing} as fallback");
                TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(base.Map), base.Map, thing);
            } 
        }
    }
}
