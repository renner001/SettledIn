using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static DanielRenner.SettledIn.TradeConnectivityUtility;

namespace DanielRenner.SettledIn
{
    public static class TradeConnectivityUtility
    {
        public class SettlementTradeInfo
        {
            public Settlement settlement;
            public float distance;
            public float goodwill;
            public float distanceFactor;
            public float goodwillFactor;
            public float finalScore;

            public override string ToString()
            {
                string name = settlement?.Label ?? "Unknown";
                return $"{name}: {finalScore:P0} (Dist {distance:0}, Rep {goodwill:+0;-0;0})";
            }
        }

        /// <summary>
        /// Calculates a trade efficiency factor (0.6–1.4) based on the five best settlements
        /// around the player colony, taking distance and reputation into account.
        /// Returns both the multiplier and the top settlements used.
        /// </summary>
        public static float CalculateTradeFactor(Map map, out List<SettlementTradeInfo> topSettlements)
        {
            topSettlements = new List<SettlementTradeInfo>();

            if (map == null || map.Tile < 0)
                return 0.4f;

            int playerTile = map.Tile;
            var settlements = Find.WorldObjects.Settlements;
            if (settlements == null)
                return 0.4f;

            Log.DebugOnce($"CalculateTradeFactor: found {settlements.Count} settlements");
            List<SettlementTradeInfo> scored = new List<SettlementTradeInfo>();

            foreach (var s in settlements)
            {
                if (s.Faction == null || s.Faction == Faction.OfPlayer)
                    continue;

                int distance = Find.WorldGrid.TraversalDistanceBetween(playerTile, s.Tile, passImpassable: false, int.MaxValue);
                if (distance < 0) continue;
                if (distance >= int.MaxValue) continue;

                float goodwill = Faction.OfPlayer.GoodwillWith(s.Faction);
                float distanceFactor = Mathf.Clamp01(1f - (distance / 200f)); // full at 0, 0 at 200+
                float goodwillFactor = Mathf.InverseLerp(-100f, 100f, goodwill); // -100..100 → 0..1

                float final = 0.4f + (distanceFactor * 0.6f * goodwillFactor);

                scored.Add(new SettlementTradeInfo
                {
                    settlement = s,
                    distance = distance,
                    goodwill = goodwill,
                    distanceFactor = distanceFactor,
                    goodwillFactor = goodwillFactor,
                    finalScore = final
                });
            }

            if (scored.Count == 0)
                return 0.4f; // isolated fallback

            topSettlements = scored.OrderByDescending(s => s.finalScore).Take(5).ToList();
            float avg = topSettlements.Average(s => s.finalScore);

            return Mathf.Lerp(0.4f, 1.4f, avg);
        }
    }

    public class Building_TradeDepot : Building
    {
        public ThingDef InputDef
        {
            get
            {
                var comp = this.GetComp<Comp_TradingDepotRefuelable>();
                return comp != null ? comp.CurrentFuelDef : null;
            }
            set
            {
                var comp = this.GetComp<Comp_TradingDepotRefuelable>();
                if (comp != null && comp.CurrentFuelDef != value)
                {
                    if (comp.CanEjectFuel())
                        comp.EjectFuel();
                    comp.CurrentFuelDef = value;
                    comp.UpdateFuelFilter();
                }
            }
        }

        private ThingDef outputDef;

        // fuelCount -> inputBuffer --tick-rate-> outputBuffer -> outputCount 
        private float inputBuffer = 0f; // stores consumed item spares for burning
        private float outputBuffer = 0f; // carries over fractional trade value
        private float tradeTargetSilverPerDay = 100f; // Player-set target
        private float currentTradeCap = 0f;            // Dynamic max based on conditions
        private float tradeFactorCached = -1f; // cached world tile trade effects
        private List<SettlementTradeInfo> tradeEffects = null;

        private float EffectiveTradeRate => Mathf.Min(tradeTargetSilverPerDay, currentTradeCap);

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos()) yield return g;
            var inputDef = InputDef;
            yield return new Command_Action
            {
                defaultLabel = inputDef == null ? "Select items to sell" : $"Input: {inputDef.label}",
                icon = inputDef == null ? TexButton.Minus : inputDef.uiIcon,
                action = SelectInput
            };

            yield return new Command_Action
            {
                defaultLabel = outputDef == null ? "Select items to buy" : $"Output: {outputDef.LabelCap}",
                icon = outputDef == null ? TexButton.Minus : outputDef.uiIcon,
                action = SelectOutput
            };

            yield return new Command_Action
            {
                defaultLabel = $"Trade Rate: {EffectiveTradeRate:F0}/{tradeTargetSilverPerDay:F0} silver/day",
                defaultDesc = "Set how much silver value is traded per day.\n" +
                  "The rate may be capped by wealth, reputation, or distance.",
                icon = Textures_SettledIn.Benefit_lvl2_TraderStock,
                action = () =>
                {
                    float newCap = currentTradeCap; // updated dynamically
                    Find.WindowStack.Add(new Dialog_Slider(
                        "Set Trade Value per Day",
                        val => tradeTargetSilverPerDay = val,
                        0f,
                        Mathf.Max(50f, newCap * 2f), // allow overshoot
                        tradeTargetSilverPerDay
                    ));
                }
            };
        }

        private IEnumerable<ThingDef> getDefsOnMap(Map map)
        {
            if (map == null) yield break;

            // Gather all haulable / refuelable items currently available in colony storage or on ground
            var allThings = map.listerThings.AllThings;

            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            foreach (var thing in allThings)
            {
                if (thing.def.category != ThingCategory.Item)
                    continue;

                // Skip minified or quest items, etc.
                if (thing.IsForbidden(Faction.OfPlayer)) continue;
                if (thing.IsInAnyStorage() || thing.PositionHeld.IsValid)
                {
                    if (seen.Add(thing.def))
                        yield return thing.def;
                }
            }
        }

        private void UpdateTradeCap()
        {
            var map = Map;
            if (map == null)
                return;

            var comp = map.GetComponent<MapComponent_SettlementResources>();
            if (comp == null)
                return;

            var totalScore = comp.cachedStatistics.totalPointsByRoomTypes + comp.cachedStatistics.totalPointsByScore;

            //float repFactor = GetFactionReputationFactor();
            //float distFactor = GetDistanceFactor();

            currentTradeCap = Mathf.Max(50f, totalScore / 1000f);
        }

        private void SelectInput()
        {
            Find.WindowStack.Add(new Dialog_SelectThingDef(def => { InputDef = def; }, "Select Input Item", getDefsOnMap(Map)));
        }

        private IEnumerable<ThingDef> CollectAllValidOutputThingDefs()
        {
            // Show *all* tangible items that make sense to trade for
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.category != ThingCategory.Item)
                    continue;
                if (def.BaseMarketValue <= 0)
                    continue;

                yield return def;
            }
        }

        private IEnumerable<ThingDef> CollectBulkGoodsTraderOutputDefs()
        {
            var map = this.Map;
            if (map == null)
                yield break;

            var trader = SettlementLevelUtility.SettlementTrader_GetOrCreate(map); 
            if (trader == null)
                yield break;

            var traderKind = trader.TraderKind;

            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            IEnumerable<ThingDef> allThings = CollectAllValidOutputThingDefs();

            foreach (StockGenerator generator in traderKind.stockGenerators)
            {
                foreach (ThingDef def in allThings)
                {
                    if (def == null)
                        continue;

                    if (def.category != ThingCategory.Item)
                        continue;

                    if (def.BaseMarketValue <= 0f)
                        continue;

                    var tradability = generator.TradeabilityFor(def);
                    if (tradability == Tradeability.All || tradability == Tradeability.Buyable)
                    {
                        if (seen.Add(def))
                            yield return def;
                    }
                }
            }
        }

        private void SelectOutput()
        {
            // todo: only generate this once per game load if it is too expensive on calculation side:
            var validOutputThings = CollectBulkGoodsTraderOutputDefs();
            Find.WindowStack.Add(new Dialog_SelectThingDef(def => outputDef = def, "Select Output Item", validOutputThings));
        }

        public override void TickRare()
        {
            Log.DebugOnce("at least Building_TradingDepot TickRare() is getting called..");
            if (tradeFactorCached < 0f || Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
            {
                Log.Debug($"Building_TradingDepot: refreshing CalculateTradeFactor for {this}..");
                tradeFactorCached = TradeConnectivityUtility.CalculateTradeFactor(Map, out tradeEffects);
            }

            if (InputDef == null || outputDef == null || !Spawned) 
                return;

            UpdateTradeCap();

            float tradeSilverPerDay = EffectiveTradeRate;
            if (tradeSilverPerDay <= 0f) 
                return;

            var refuelableComp = GetComp<CompRefuelable>();
            if (refuelableComp == null)
                return;

            float tradeSilverThisTick = tradeFactorCached * tradeSilverPerDay / (GenDate.TicksPerDay / 250);

            float inValue = InputDef.BaseMarketValue;
            if (inValue <= 0f)
                return;

            float outValue = outputDef.BaseMarketValue;
            if (outValue <= 0f)
                return;

            float itemsRequiredFromFuel = Mathf.Ceil((tradeSilverThisTick - inputBuffer) / inValue);
            var itemsAvailableAsFuel = refuelableComp.Fuel;
            var consumedAmount = Mathf.Min(itemsAvailableAsFuel, itemsRequiredFromFuel);
            // consume!
            if (consumedAmount > 0f)
            {
                refuelableComp.ConsumeFuel(consumedAmount);
                inputBuffer += consumedAmount * inValue;
            }

            // convert to output buffer
            var shiftAmountBetweenBuffers = Mathf.Min(inputBuffer, tradeSilverThisTick);
            inputBuffer -= shiftAmountBetweenBuffers;
            outputBuffer += shiftAmountBetweenBuffers;

            // spawn new items if enough buffer is available
            var outputItemCount = Mathf.Floor(outputBuffer / outValue);
            if (outputItemCount > 0f)
            {
                Thing outThing = ThingMaker.MakeThing(outputDef);
                outThing.stackCount = (int)outputItemCount;
                GenPlace.TryPlaceThing(outThing, InteractionCell, Map, ThingPlaceMode.Near);
                outputBuffer -= outputItemCount * outValue;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.AppendLine($"Capacity for trading around {currentTradeCap:0} silver a day.");
            sb.AppendLine($"Trade Efficiency: {tradeFactorCached:0.00}x");
            if (tradeEffects != null && tradeEffects.Count > 0)
            {
                sb.AppendLine("Connected Settlements:");
                foreach (var s in tradeEffects)
                {
                    sb.AppendLine($"  {s.settlement.LabelShort.CapitalizeFirst()}");
                    sb.AppendLine($"    Distance: {s.distance:0} | Rep: {s.goodwill:+0;-0;0} | Score: {s.finalScore:P0}");
                }
            }
            else
            {
                sb.AppendLine("No established trade routes available.");
            }

            return sb.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            // Save/load definitions
            Scribe_Defs.Look(ref outputDef, "outputDef");

            // Save/load progress and other variables
            Scribe_Values.Look(ref inputBuffer, "inputBuffer", 0f);
            Scribe_Values.Look(ref outputBuffer, "outputBuffer", 0f);
            Scribe_Values.Look(ref tradeTargetSilverPerDay, "tradeTargetSilverPerDay", 100f);
        }
    }


}
