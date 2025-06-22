using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{

    public class Building_ArktechPersonalAccessPoint : Building
    {
        public new Building_ArktechPersonalAccessPointDef def
        {
            get
            {
                return base.def as Building_ArktechPersonalAccessPointDef;
            }
        }

        //Graphic arktechPapThumbnail = null;
        Graphic arktechPap = null;
        Graphic[] arkTechPapGlowAnimation;

        public static void HandleAnimation(Thing thing, Graphic[] animation, float secondsPerAnimationCycle = 13.0f, AltitudeLayer layer=AltitudeLayer.FloorCoverings, bool doFlash = false, float flashMinAlpha=0.1f, float flashMaxAlpha=0.8f, float secondsPerFlash = 15.0f)
        {
            var currTick = Find.TickManager.TicksGame;
            int animationTime = (int)Math.Round(60*secondsPerAnimationCycle);
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
            initGraphicAlternativesIfNecessary();
            HandleAnimation(this, arkTechPapGlowAnimation, 12.0f, AltitudeLayer.BuildingOnTop, true, 0.1f, 0.7f, 15.0f);
            base.DrawAt(drawLoc, flip);
        }

        private void initGraphicAlternativesIfNecessary()
        {
            if (this.arktechPap == null)
            {
                Log.Debug("Loading SettlementCenter additional textures from texPath*: " + def.graphicData.texPath);
                //this.arktechPapThumbnail = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath, this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, null);
                this.arktechPap = GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath, this.DefaultGraphic.Shader, this.DefaultGraphic.drawSize, this.DefaultGraphic.color, this.DefaultGraphic.colorTwo, null);
                arkTechPapGlowAnimation = new[] {
                    GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "_glow_2", ShaderDatabase.Transparent, this.DefaultGraphic.drawSize, Color.cyan, Color.cyan, null),
                    GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "_glow_3", ShaderDatabase.Transparent, this.DefaultGraphic.drawSize, Color.cyan, Color.cyan, null),
                    GraphicDatabase.Get(this.DefaultGraphic.GetType(), def.graphicData.texPath + "_glow_4", ShaderDatabase.Transparent, this.DefaultGraphic.drawSize, Color.cyan, Color.cyan, null),
                };

            }
        }
    }
}
