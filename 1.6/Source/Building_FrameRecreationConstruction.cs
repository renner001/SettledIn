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
    public class Building_FrameRecreationConstruction : Building
    {
        public ThingDef finishedDef; // MonumentOfThePeople, etc.

        public float progress;
        public float workRequired = 20000f; // default required work if there is no target def

        public bool IsUpgrading = true;

        public float ProgressPercent => Mathf.Clamp01(progress / workRequired);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            Log.Debug("at least Building_FrameRecreationConstruction.SpawnSetup is getting called");
            base.SpawnSetup(map, respawningAfterLoad);

            // infer finished def automatically
            if (finishedDef == null)
            {
                Log.Debug($"Building_FrameRecreationConstruction.SpawnSetup: loading target def for {def}");
                var defName = def.defName.Replace("_unfinished", "");
                if (defName != def.defName)
                {
                    finishedDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                }
                Log.Debug($"Building_FrameRecreationConstruction.SpawnSetup: loaded {finishedDef} as target def for {def}");
                if (finishedDef != null)
                {
                    workRequired = finishedDef.GetStatValueAbstract(
                        StatDefOf.WorkToBuild,
                        Stuff
                    );
                }
            }
        }

        
        public virtual void DoRecreationWork(Pawn pawn)
        {
            if (!IsUpgrading)
                return;
            float speed = 1f;
            progress += speed;
            if (progress >= workRequired)
            {
                Complete();
            } 
        }

        protected void BeginUpgrading()
        {
            progress = 0;
            IsUpgrading = true;
        }

        protected virtual void Complete()
        {
            IsUpgrading = false;
            progress = -1f;
            if (Destroyed)
                return;
            Log.Debug($"Building_FrameRecreationConstruction.Complete: finished construction of {this}");
            var rotation = Rotation;
            var position = Position;
            var map = Map;
            Destroy(DestroyMode.Vanish); // must be this way around or spawning the new item automatically replaces it as it shared the same coordinates
            var finished = ThingMaker.MakeThing(finishedDef);
            GenSpawn.Spawn(finished, position, map, rotation);
        }

        public override string GetInspectString()
        {
            var otherString = base.GetInspectString();
            var sb = new StringBuilder(otherString);
            if (IsUpgrading)
            {
                sb.AppendLine();
                sb.Append($"Progress: {ProgressPercent:P0}");
            }
#if DEBUG
            sb.AppendLine();
            sb.Append($"progress: {progress} workRequired: {workRequired}");
#endif
            return sb.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref IsUpgrading, "IsUpgrading");
        }

        // some stolen code from the RimWorld.Frame logic to render a ongoing construction:
        private static Material UnderfieldMat;

        private static readonly Color recreationColor = new Color(0.56f, 0.28f, 0.7f, 0.5f);

        private Material cachedCornerMat = null;
        private Material CornerMat
        {
            get
            {
                if (this.cachedCornerMat == null)
                {
                    Texture2D CornerTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Corner", true);
                    this.cachedCornerMat = MaterialPool.MatFrom(CornerTex, ShaderDatabase.MetaOverlay, recreationColor);
                }
                return this.cachedCornerMat;
            }
        }

        private Material cachedTileMat = null;
        private Material TileMat
        {
            get
            {
                if (this.cachedTileMat == null)
                {
                    Texture2D TileTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Tile", true);
                    this.cachedTileMat = MaterialPool.MatFrom(TileTex, ShaderDatabase.MetaOverlay, recreationColor);
                }
                return this.cachedTileMat;
            }
        }

        protected void DrawProgressOverlay(Vector3 drawLoc, bool flip = false)
        {
            if (ProgressPercent < 0f || ProgressPercent >= 1f)
                return;

            Vector3 vector = drawLoc;
            ThingDef buildDef = this.finishedDef;
            Vector2 vector2 = new Vector2((float)this.def.size.x, (float)this.def.size.z);
            vector2.x *= 1.15f;
            vector2.y *= 1.15f;
            Vector3 vector3 = new Vector3(vector2.x, 1f, vector2.y);
            Matrix4x4 matrix4x = default(Matrix4x4);
            matrix4x.SetTRS(vector, base.Rotation.AsQuat, vector3);
            if (UnderfieldMat == null)
            {
                UnderfieldMat = MaterialPool.MatFrom("Things/Building/BuildingFrame/Underfield", ShaderDatabase.Transparent);
            }
            Graphics.DrawMesh(MeshPool.plane10, matrix4x, UnderfieldMat, 0);
            int num = 4;
            for (int i = 0; i < num; i++)
            {
                float num2 = (float)Mathf.Min(base.RotatedSize.x, base.RotatedSize.z) * 0.38f;
                IntVec3 intVec = default(IntVec3);
                if (i == 0)
                {
                    intVec = new IntVec3(-1, 0, -1);
                }
                else if (i == 1)
                {
                    intVec = new IntVec3(-1, 0, 1);
                }
                else if (i == 2)
                {
                    intVec = new IntVec3(1, 0, 1);
                }
                else if (i == 3)
                {
                    intVec = new IntVec3(1, 0, -1);
                }
                Vector3 vector4 = default(Vector3);
                vector4.x = (float)intVec.x * ((float)base.RotatedSize.x / 2f - num2 / 2f);
                vector4.z = (float)intVec.z * ((float)base.RotatedSize.z / 2f - num2 / 2f);
                Vector3 vector5 = new Vector3(num2, 1f, num2);
                Matrix4x4 matrix4x2 = default(Matrix4x4);
                matrix4x2.SetTRS(vector + Vector3.up * 0.03f + vector4, new Rot4(i).AsQuat, vector5);
                Graphics.DrawMesh(MeshPool.plane10, matrix4x2, this.CornerMat, 0);
            }
            int num3 = Mathf.CeilToInt((ProgressPercent - 0f) / 1f * (float)base.RotatedSize.x * (float)base.RotatedSize.z * 4f);
            IntVec2 intVec2 = base.RotatedSize * 2;
            for (int j = 0; j < num3; j++)
            {
                IntVec2 intVec3 = default(IntVec2);
                intVec3.z = j / intVec2.x;
                intVec3.x = j - intVec3.z * intVec2.x;
                Vector3 vector6 = new Vector3((float)intVec3.x * 0.5f, 0f, (float)intVec3.z * 0.5f) + vector;
                vector6.x -= (float)base.RotatedSize.x * 0.5f - 0.25f;
                vector6.z -= (float)base.RotatedSize.z * 0.5f - 0.25f;
                Vector3 vector7 = new Vector3(0.5f, 1f, 0.5f);
                Matrix4x4 matrix4x3 = default(Matrix4x4);
                matrix4x3.SetTRS(vector6 + Vector3.up * 0.02f, Quaternion.identity, vector7);
                Graphics.DrawMesh(MeshPool.plane10, matrix4x3, TileMat, 0);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Log.DebugOnce($"at least Building_FrameRecreationConstruction.DrawAt is getting called");
            base.DrawAt(drawLoc, flip);
            if (finishedDef != null)
            {
                GhostDrawer.DrawGhostThing(
                    Position,
                    Rotation,
                    finishedDef,
                    finishedDef.graphic,
                    Color.white,
                    AltitudeLayer.MetaOverlays
                );
            }
            if (IsUpgrading)
                DrawProgressOverlay(drawLoc, flip);
        }
    }
}
