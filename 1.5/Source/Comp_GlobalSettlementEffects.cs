using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static UnityEngine.Random;

namespace DanielRenner.SettledIn
{
    class Comp_GlobalSettlementEffects : ThingComp
    {
        public Comp_GlobalSettlementEffects()
        {
            Log.DebugOnce("Comp_GlobalSettlementEffects.Comp_GlobalSettlementEffects() at least comps are getting created...");
        }

        Gizmo_SettlementBufferSlider gizmo_SettlementBufferSlider = null;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (this.parent.Faction == Faction.OfPlayer)
            {
                var settlementResources = parent.Map.GetComponent<MapComponent_SettlementResources>();
                if (settlementResources != null) //  && settlementResources.SettlementLevel < SettlementLevelUtility.MaxLevel
                {
                    if (gizmo_SettlementBufferSlider == null) // create the slider gizmo, if it doesnt exist yet. Necessary, as tooptips are handled badly if the object does not persist = flickering
                    {
                        gizmo_SettlementBufferSlider = new Gizmo_SettlementBufferSlider(parent.Map)
                        {
                            Disabled = false
                        };
                    }
                    yield return gizmo_SettlementBufferSlider;
                    // settlement screen
                    yield return new Command_Action
                    {
                        icon = Textures_SettledIn.MenuIcon,
                        Disabled = false, // never disabled
                        disabledReason = "",
                        defaultLabel = "DanielRenner.SettledIn.GizmoOpenSettlementTabLabel".Translate(),
                        defaultDesc = "DanielRenner.SettledIn.GizmoOpenSettlementTabDescription".Translate(),
                        action = delegate
                        {
                            Log.Debug("Comp_GlobalSettlementEffects.OpenSettlementTab was clicked.");
                            Find.MainTabsRoot.SetCurrentTab(DefOfs_SettledIn.MainTabSettlementScores, true);
                        }
                    };
                    if (SettlementLevelUtility.IsBenefitActiveAt(settlementResources.SettlementLevel, SettlementLevelUtility.Benefit_lvl2_TraderStock))
                    {
                        // settlement trader trader screen
                        yield return new Command_Action
                        {
                            icon = Textures_SettledIn.Benefit_lvl2_TraderStock,
                            Disabled = false, // never disabled
                            disabledReason = "",
                            defaultLabel = "DanielRenner.SettledIn.GizmoMagicTraderLabel".Translate(),
                            defaultDesc = "DanielRenner.SettledIn.GizmoMagicTraderDescription".Translate(),
                            action = delegate
                            {
                                Log.Debug("Comp_GlobalSettlementEffects.MagicTrader was clicked.");
                                // create the trade ship of our settlement, if it doesn't exist yet.
                                TradeShip_SettledIn existingTradeShip = SettlementLevelUtility.SettlementTrader_GetOrCreate(parent.Map);
                                // select the right negotiator
                                Pawn negotiator = SettlementGameUtilities.GetBestPlayerNegotiatorOnMap(parent.Map, existingTradeShip.TraderKind, existingTradeShip.Faction);
                                Log.Debug($"using {negotiator} as negotiator for trade with {existingTradeShip}");
                                if (negotiator == null || existingTradeShip == null)
                                    return;
                                var tradeDialog = new Dialog_Trade(negotiator, existingTradeShip, false);
                                Find.WindowStack.Add(tradeDialog);
                            }
                        }; 
                        const float SettlementBufferPercentRestockTrader = 0.25f;
                        var enoughBufferForRestockTrader = (float)settlementResources.ManagementBuffer_current / settlementResources.ManagementBuffer_max >= SettlementBufferPercentRestockTrader;
                        // settlement trader: restock command
                        yield return new Command_Action
                        {
                            icon = Textures_SettledIn.SettlementCommand_Restock, //todo: create new icon...
                            Disabled = !enoughBufferForRestockTrader,
                            disabledReason = "DanielRenner.SettledIn.GizmoNotEnoughBuffer".Translate(),
                            defaultLabel = "DanielRenner.SettledIn.GizmoMagicTraderRestockLabel".Translate(),
                            defaultDesc = "DanielRenner.SettledIn.GizmoMagicTraderRestockDescription".Translate(),
                            action = delegate
                            {
                                Log.Debug("Comp_GlobalSettlementEffects.MagicTraderRestock was clicked.");
                                // create the trade ship of our settlement, if it doesn't exist yet.
                                SettlementLevelUtility.SettlementTrader_RefreshStock(parent.Map);
                                Messages.Message("DanielRenner.SettledIn.MagicTraderRestocked".Translate(), MessageTypeDefOf.PositiveEvent);
                            }
                        };
                    }
                    // global effect: Reduce Resistance
                    if (settlementResources.SettlementLevel >= 0) // available from first level
                    {
                        var GlobalEffects_IsPrisonFarAwayOnCooldown = Find.TickManager.TicksGame > MapComponent_SettlementResources.GlobalEffects_IsPrisonFarAway_Cooldown + settlementResources.GlobalEffects_IsPrisonFarAway_lastTriggered;
                        const float SettlementBufferPercentBreakingResistance = 0.75f;
                        var enoughBufferForBreakingResistance = (float)settlementResources.ManagementBuffer_current / settlementResources.ManagementBuffer_max >= SettlementBufferPercentBreakingResistance;
                        yield return new Command_Action
                        {
                            //hotKey = KeyBindingDefOf.Command_TogglePower,
                            icon = Textures_SettledIn.SettlementCommand_PrisonFarAway,
                            Disabled = !settlementResources.GlobalEffects_IsPrisonFarAway || !enoughBufferForBreakingResistance,
                            disabledReason = !settlementResources.GlobalEffects_IsPrisonFarAway ? "DanielRenner.SettledIn.GizmoIsPrisonFarAwayDisabledReason".Translate() : "DanielRenner.SettledIn.GizmoOnCooldown".Translate(),
                            defaultLabel = "DanielRenner.SettledIn.GizmoIsPrisonFarAwayLabel".Translate(),
                            defaultDesc = "DanielRenner.SettledIn.GizmoIsPrisonFarAwayDescription".Translate(),
                            action = delegate
                            {
                                Log.Debug("Comp_GlobalSettlementEffects.ReduceResistance was clicked.");
                                DefOfs_SettledIn.BubblePopping.PlayOneShotOnCamera();
                                var prisoners = this.parent?.Map?.mapPawns?.PrisonersOfColony;
                                Pawn[] validPawns = null;
                                if (prisoners != null)
                                {
                                    validPawns = prisoners.Where(prisoner => { return prisoner.guest.Recruitable; }).ToArray();
                                }
                                if (validPawns != null && validPawns.Length > 0)
                                {
                                    var count = validPawns.Length;
                                    var randomPawn = validPawns[Rand.Range(0, count)];
                                    randomPawn.guest.resistance = 0f;
                                    Messages.Message($"{randomPawn} resistance broke. For the settlement!", MessageTypeDefOf.PositiveEvent);
                                    settlementResources.GlobalEffects_IsPrisonFarAway_lastTriggered = Find.TickManager.TicksGame;
                                    settlementResources.ManagementBuffer_current -= (int)(SettlementBufferPercentBreakingResistance * settlementResources.ManagementBuffer_max);
                                }
                            }
                        };
                        const float SettlementBufferPercentBreakingWill = 0.75f;
                        var enoughBufferForBreakingWill = (float)settlementResources.ManagementBuffer_current / settlementResources.ManagementBuffer_max >= SettlementBufferPercentBreakingWill;
                        var primaryIdeo = Faction.OfPlayer.ideos?.PrimaryIdeo;
                        yield return new Command_Action
                        {
                            //hotKey = KeyBindingDefOf.Command_TogglePower,
                            icon = primaryIdeo != null ? primaryIdeo.Icon : Textures_SettledIn.SettlementCommand_PrisonFarAway,
                            Disabled = !enoughBufferForBreakingWill || primaryIdeo == null,
                            disabledReason = "DanielRenner.SettledIn.GizmoBreakingWillDisabledReason".Translate(),
                            defaultLabel = "DanielRenner.SettledIn.GizmoBreakingWillLabel".Translate(),
                            defaultDesc = "DanielRenner.SettledIn.GizmoBreakingWillDescription".Translate(),
                            action = delegate
                            {
                                Log.Debug("Comp_GlobalSettlementEffects.ReduceWill was clicked.");
                                DefOfs_SettledIn.BubblePopping.PlayOneShotOnCamera();
                                var colonistsAndPrisoners = this.parent?.Map?.mapPawns?.FreeColonistsAndPrisoners;
                                Pawn[] validPawns = null;
                                if (colonistsAndPrisoners != null && primaryIdeo != null)
                                {
                                    validPawns = colonistsAndPrisoners.Where(pawn => { return pawn.Ideo != null && pawn.ideo.Certainty > 0 && pawn.Ideo != primaryIdeo; }).ToArray();
                                }
                                if (validPawns != null)
                                {
                                    var count = validPawns.Length;
                                    var randomPawn = validPawns[Rand.Range(0, count)];
                                    randomPawn.ideo.SetIdeo(primaryIdeo);
                                    Messages.Message($"{randomPawn} was convinced to join {primaryIdeo}. For the settlement!", MessageTypeDefOf.PositiveEvent);
                                }
                                settlementResources.GlobalEffects_IsPrisonFarAway_lastTriggered = Find.TickManager.TicksGame;
                                settlementResources.ManagementBuffer_current -= (int)(SettlementBufferPercentBreakingWill * settlementResources.ManagementBuffer_max);
                            }
                        };
                        const float SettlementBufferPercentIncreaseSkill = 0.5f;
                        const int IncreaseSkillBy = 2500;
                        var enoughBufferForIncreaseSkill = (float)settlementResources.ManagementBuffer_current / settlementResources.ManagementBuffer_max >= SettlementBufferPercentIncreaseSkill;
                        yield return new Command_Action
                        {
                            //hotKey = KeyBindingDefOf.Command_TogglePower,
                            icon = Textures_SettledIn.Benefit_lvl2_ColonistFocus,
                            Disabled = !enoughBufferForIncreaseSkill,
                            disabledReason = "DanielRenner.SettledIn.GizmoIncreaseSkillDisableReason".Translate(),
                            defaultLabel = "DanielRenner.SettledIn.GizmoIncreaseSkillLabel".Translate(),
                            defaultDesc = "DanielRenner.SettledIn.GizmoIncreaseSkillDescription".Translate(),
                            action = delegate
                            {
                                Log.Debug("Comp_GlobalSettlementEffects.IncreaseSkill was clicked.");
                                DefOfs_SettledIn.BubblePopping.PlayOneShotOnCamera();
                                var validPawns = this.parent?.Map?.mapPawns?.FreeColonists?.ToArray();
                                if (validPawns != null)
                                {
                                    var count = validPawns.Length;
                                    var randomPawn = validPawns[Rand.Range(0, count)];
                                    var allSkills = randomPawn.skills.skills.ToArray();
                                    if (allSkills != null && allSkills.Length > 0)
                                    {
                                        var randomSkill = allSkills[Rand.Range(0, allSkills.Length)];
                                        randomSkill.Learn(IncreaseSkillBy, true);
                                        Messages.Message($"{randomPawn} learned something in {randomSkill.def}. For the settlement!", MessageTypeDefOf.PositiveEvent);
                                    }
                                }
                                settlementResources.ManagementBuffer_current -= (int)(SettlementBufferPercentIncreaseSkill * settlementResources.ManagementBuffer_max);
                            }
                        };
                        const float SettlementBufferPercentCallTrader = 0.5f;
                        var enoughBufferForCallTrader = (float)settlementResources.ManagementBuffer_current / settlementResources.ManagementBuffer_max >= SettlementBufferPercentCallTrader;
                        yield return new Command_Action
                        {
                            //hotKey = KeyBindingDefOf.Command_TogglePower,
                            icon = Textures_SettledIn.Benefit_lvl2_TraderStock,
                            Disabled = !enoughBufferForCallTrader,
                            disabledReason = "DanielRenner.SettledIn.GizmoCallTraderReason".Translate(),
                            defaultLabel = "DanielRenner.SettledIn.GizmoCallTraderLabel".Translate(),
                            defaultDesc = "DanielRenner.SettledIn.GizmoCallTraderDescription".Translate(),
                            action = delegate
                            {
                                Log.Debug("Comp_GlobalSettlementEffects.CallTrader was clicked.");
                                DefOfs_SettledIn.BubblePopping.PlayOneShotOnCamera();
                                IncidentDef traderCaravanArrival = IncidentDefOf.TraderCaravanArrival;
                                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(traderCaravanArrival.category, parent.Map);
                                //incidentParms.traderKind = DefDatabase<TraderKindDef>.GetNamed("BulkGoodsTrader");
                                traderCaravanArrival.Worker.TryExecute(incidentParms);
                                settlementResources.ManagementBuffer_current -= (int)(SettlementBufferPercentCallTrader * settlementResources.ManagementBuffer_max);
                            }
                        };
                        if (SettlementLevelUtility.IsBenefitActiveAt(settlementResources.SettlementLevel, SettlementLevelUtility.Benefit_lvl3_WalkSpeed))
                        {
                            yield return new Command_Toggle
                            {
                                //hotKey = KeyBindingDefOf.Command_TogglePower,
                                icon = Textures_SettledIn.Benefit_lvl3_WalkSpeed,
                                Disabled = false,
                                disabledReason = "DanielRenner.SettledIn.GizmoWalkSpeedReason".Translate(),
                                defaultLabel = "DanielRenner.SettledIn.GizmoWalkSpeedLabel".Translate(),
                                defaultDesc = "DanielRenner.SettledIn.GizmoWalkSpeedDescription".Translate(),
                                toggleAction = () => {
                                    Log.Debug("Comp_GlobalSettlementEffects.WalkSpeed was clicked.");
                                    settlementResources.GlobalEffectWalkSpeedToggled = !settlementResources.GlobalEffectWalkSpeedToggled;
                                },
                                isActive = () => { 
                                    return settlementResources.GlobalEffectWalkSpeedToggled; 
                                }
                            };
                            
                        }
                    }
                }
                /*
                if (DebugSettings.ShowDevGizmos)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Upgrade Settlement",
                        action = delegate
                        {
                            Log.Debug("CompUpgradeSettlement was clicked. Triggering settlement downgrade now");
                            // get settlement manager and trigger the downgrade
                            (this.parent as Building_SettlementCenter).UpgradeSettlement();
                        }
                    };
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Downgrade Settlement",
                        action = delegate
                        {
                            Log.Debug("CompUpgradeSettlement was clicked. Triggering settlement downgrade now");
                            // get settlement manager and trigger the downgrade
                            settlementResources.DowngradeSettlement();
                        }
                    };
                }
                */
            }
            yield break;
        }

        public override void DrawGUIOverlay()
        {
            Log.DebugOnce("CompUpgradeSettlement.DrawGUIOverlay() is getting called... ");
            base.DrawGUIOverlay();
        }
        
    }
}
