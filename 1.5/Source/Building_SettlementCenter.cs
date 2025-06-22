using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{

    public class Building_SettlementCenter : Building
    {
        public new Building_SettlementCenterDef def
        {
            get
            {
                return base.def as Building_SettlementCenterDef;
            }
        }

        public void UpgradeSettlement()
        {
            var settlementResources = Map.GetComponent<MapComponent_SettlementResources>();
            var success = settlementResources.UpgradeSettlement();
            if (!success)
            {
                // we only run the following updates on our building if the update was a success
                return;
            }
            
            // up the quality of the building by 1 until we reach legendary for every update
            var qualityComp = this.GetComp<CompQuality>();
            switch (qualityComp.Quality)
            {
                case QualityCategory.Awful:
                    qualityComp.SetQuality(QualityCategory.Poor, ArtGenerationContext.Colony);
                    break;
                case QualityCategory.Poor: 
                    qualityComp.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
                    break;
                case QualityCategory.Normal:
                    qualityComp.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Colony);
                    break;
                case QualityCategory.Masterwork:
                    qualityComp.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Colony);
                    break;
                case QualityCategory.Legendary: break;
                default: break;
            }
        }

        public override string GetInspectString()
        {
            var baseString = base.GetInspectString();
            return baseString + getInspectString();
        }
        private string getInspectString()
        {
            var settlementScoreManager = Current.Game?.GetComponent<GameComponent_SettlementScoreManager>();
            var settlementResources = Map.GetComponent<MapComponent_SettlementResources>();
            var inspectString = "\nSettlement Level: " + settlementResources.SettlementLevel;
            if (settlementResources.SettlementLevel < SettlementLevelUtility.MaxLevel)
            {
                string upgradeRequirements;
                // todo: increase performance by using the cached values
                var allegibleForUpdate = SettlementLevelUtility.CheckRequirements(settlementResources.SettlementLevel + 1, Map, out upgradeRequirements);
                if (allegibleForUpdate)
                {
                    inspectString += "\nCan be upgraded now.";
                }
                else
                {
                    inspectString += "\nRequirements for next Upgrade:\n" + upgradeRequirements;
                }
            }
            
            return inspectString;
        }

        Graphic settlementCenter0 = null;
        Graphic settlementCenter1 = null;
        Graphic settlementCenter2 = null;
        Graphic settlementCenter3 = null;
        Graphic settlementCenter3glow = null;
        Graphic settlementCenter4 = null;
        Graphic settlementCenter4glow = null;
        Graphic settlementCenter5 = null;
        Graphic settlementCenter5glow = null;
        Graphic settlementCenter6 = null;

        private void initGraphicAlternativesIfNecessary()
        {
            if (this.settlementCenter0 == null)
            {
                Log.Debug("Loading SettlementCenter additional textures from texPath*: " + def.graphicData.texPath);
                this.settlementCenter0 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "0", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
                this.settlementCenter1 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "1", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
                this.settlementCenter2 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "2", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
                this.settlementCenter3 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "3", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
                this.settlementCenter3glow = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "3glow", ShaderDatabase.Transparent, this.DefaultGraphic.drawSize, Color.white, Color.white, null);
                this.settlementCenter4 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "4", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
                this.settlementCenter4glow = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "4glow", ShaderDatabase.Transparent, this.DefaultGraphic.drawSize, Color.white, Color.white, null);
                this.settlementCenter5 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "5", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
                this.settlementCenter5glow = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "5glow", ShaderDatabase.Transparent, this.DefaultGraphic.drawSize, Color.white, Color.white, null);
                this.settlementCenter6 = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "6", this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, def.graphicData, null);
            }
        }

        public static void HandleAnimation(Thing thing, Graphic[] animation, float secondsPerAnimationCycle = 13.0f, AltitudeLayer layer = AltitudeLayer.FloorCoverings, bool doFlash = false, float flashMinAlpha = 0.1f, float flashMaxAlpha = 0.8f, float secondsPerFlash = 15.0f)
        {
            var currTick = Find.TickManager.TicksGame;
            int animationTime = (int)Math.Round(60 * secondsPerAnimationCycle);
            // calculate the animation frame:
            var animationFrameCount = (float)animation.Length;
            var nextFrame = (int)Math.Floor(animationFrameCount * 2 * Math.Abs((currTick % animationTime) - (animationTime / 2)) / animationTime);
            // in very rare cases, the frame can be exactly max.0->max+1 or 0.0->-1
            if (nextFrame >= animation.Length)
            {
                nextFrame = animation.Length - 1;
            }
            else if (nextFrame < 0)
            {
                nextFrame = 0;
            }

            // calculate flashing
            var toRender = animation[nextFrame];
            if (doFlash)
            {
                float ticksPerFlash = (float)(secondsPerFlash * 60f);
                // calculate the alpha for the next frame:
                Color newColorOne = animation[nextFrame].Color;
                newColorOne.a = flashMinAlpha + 2 * (flashMaxAlpha - flashMinAlpha) * Math.Abs((currTick % ticksPerFlash) - (ticksPerFlash / 2)) / ticksPerFlash;
                Color newColorTwo = animation[nextFrame].ColorTwo;
                newColorTwo.a = flashMinAlpha + 2 * (flashMaxAlpha - flashMinAlpha) * Math.Abs((currTick % ticksPerFlash) - (ticksPerFlash / 2)) / ticksPerFlash;
                toRender = animation[nextFrame].GetColoredVersion(animation[nextFrame].Shader, newColorOne, newColorOne);
            }

            toRender.Draw(thing.Position.ToVector3ShiftedWithAltitude(layer), thing.Rotation, thing, 0);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            var settlementResources = Map != null ? Map.GetComponent<MapComponent_SettlementResources>() : null;
            if (settlementResources != null)
            {
                var glower = this.GetComp<CompGlower>();
                glower.Props.glowRadius = 7 + 2 * settlementResources.SettlementLevel;

                if (settlementResources.SettlementLevel >= 3)
                {
                    HandleAnimation(this, new[] { settlementCenter3glow }, 30.0f, AltitudeLayer.BuildingOnTop, true, 0.1f, 0.4f, 9.0f);
                }
                if (settlementResources.SettlementLevel >= 4)
                {
                    HandleAnimation(this, new[] { settlementCenter4glow }, 30.0f, AltitudeLayer.BuildingOnTop, true, 0.2f, 0.5f, 14.0f);
                }
                if (settlementResources.SettlementLevel >= 5)
                {
                    HandleAnimation(this, new[] { settlementCenter5glow }, 30.0f, AltitudeLayer.BuildingOnTop, true, 0.4f, 0.8f, 30.0f);
                }
            }
        }

        public override Graphic Graphic
        {
            get
            {
                try
                {
                    initGraphicAlternativesIfNecessary();
                    // get the current stage of this settlement
                    //var settlementScoreManager = Current.Game?.GetComponent<GameComponent_SettlementScoreManager>();
                    var settlementResources = Map != null ? Map.GetComponent<MapComponent_SettlementResources>() : null;
                    if (settlementResources != null)
                    {
                        switch (settlementResources.SettlementLevel)
                        {
                            case 0:
                                return settlementCenter0;
                            case 1:
                                return settlementCenter1;
                            case 2:
                                return settlementCenter2;
                            case 3:
                                return settlementCenter3;
                            case 4:
                                return settlementCenter4;
                            case 5:
                                return settlementCenter5;
                            case 6:
                                return settlementCenter6;
                            default:
                                Log.ErrorOnce("failed to find right texture to render. Odd but not critical.", 23987461);
                                return base.Graphic;
                        }
                    }
                } 
                catch (Exception ex)
                {
                    Log.ErrorOnce("failed to find right texture to render. Odd but not critical. " + ex.ToString(), 23987461);
                    
                }
                return base.Graphic;
            }
        }
    }
}
