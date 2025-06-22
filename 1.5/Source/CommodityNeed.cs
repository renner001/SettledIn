using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    internal class CommodityNeed : Need
    {
        public CommodityNeed(Pawn pawn)
            : base(pawn)
        {
        }


        public override void NeedInterval()
        {
            Log.DebugOnce("at least the CommodityNeed.NeedInterval() hook is getting called");
            float expectationFactor = ((ExpectationsUtility.CurrentExpectationFor(pawn).order * 10f) / 4f) + 1f;
            // expectation factor is between
            // 0: ExtremelyLow = 1
            // 1: very low = 3.5
            // 2: low = 6
            // 3: moderate = 8.5
            // 4: high = 11
            // from 0.1% per hour to 1% per hour loss based on expectations:
            this.CurLevel -= (150f/2500f) * 0.001f * expectationFactor;
        }

        public override float CurLevel { 
            get => base.CurLevel;
            set {
                // remove memories
                var prevValue = base.CurLevel;
                base.CurLevel = value;
                if (value <= 0.01f)
                {
                    // we only react if the settlement level is greater 1
                    var mapComponent = pawn?.Map?.GetComponent<MapComponent_SettlementResources>();
                    if (mapComponent == null || mapComponent.SettlementLevel < 2)
                        return;
                    pawn.needs.mood.thoughts.memories.TryGainMemory(DefOfs_SettledIn.WantCommodities, null, null);
                }
                else if (prevValue < value)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefOfs_SettledIn.WantCommodities);
                }
            } 
        }
    }
}
