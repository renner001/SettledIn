using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace DanielRenner.SettledIn
{
    public enum BubbleType { Mood=0, Recreation=1, Sleep=2, Comfort=3, Buffer=4 }

    public class Bubble : IExposable
    {
        public Vector3 Position = new Vector3(0f, Altitudes.AltitudeFor(AltitudeLayer.MapDataOverlay), 0f);
        public Vector3 Velocity = new Vector3(0f,0f,0.3f);
        //public float Size = 0.5f;
        public int spawnTick = Find.TickManager.TicksGame;
        //public int lastUpdateTick = Find.TickManager.TicksGame;
        public int despawnTick = Find.TickManager.TicksGame + Rand.Range(60000, 120000);
        public BubbleType Type = BubbleType.Mood;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Position, "Position", new Vector3(0f, Altitudes.AltitudeFor(AltitudeLayer.MapDataOverlay), 0f));
            Scribe_Values.Look(ref Velocity, "Velocity", new Vector3(0f, 0f, 0.3f));
            Scribe_Values.Look(ref spawnTick, "spawnTick", Find.TickManager.TicksGame);
            Scribe_Values.Look(ref despawnTick, "despawnTick", Find.TickManager.TicksGame + Rand.Range(60000, 120000)); // between 1 and 2 days
            Scribe_Values.Look(ref Type, "Type", BubbleType.Mood);
        }
    }

    [StaticConstructorOnStartup]
    class CompClickBaiter : ThingComp
    {
        public CompClickBaiter()
        {
            Log.DebugOnce("CompClickBaiter.CompClickBaiter() at least comps are getting created...");
        }

        static Material Bubble_Sleep = null;
        static Material Bubble_Recreation = null;
        static Material Bubble_Mood = null;
        static Material Bubble_Comfort = null;
        static Material Bubble_Buffer = null;

        static CompClickBaiter()
        {
            Bubble_Sleep = new Material(ShaderDatabase.Transparent);
            Bubble_Sleep.mainTexture = Textures_SettledIn.Bubble_Sleep;

            Bubble_Recreation = new Material(ShaderDatabase.Transparent);
            Bubble_Recreation.mainTexture = Textures_SettledIn.Bubble_Recreation;

            Bubble_Mood = new Material(ShaderDatabase.Transparent);
            Bubble_Mood.mainTexture = Textures_SettledIn.Bubble_Mood;

            Bubble_Comfort = new Material(ShaderDatabase.Transparent);
            Bubble_Comfort.mainTexture = Textures_SettledIn.Bubble_Comfort;

            Bubble_Buffer = new Material(ShaderDatabase.Transparent);
            Bubble_Buffer.mainTexture = Textures_SettledIn.Bubble_Buffer;
        }

        static Vector3 FloatSize = new Vector3(6f, 0f, 6f);
        const float FloatDampeningFramePercent = 0.20f;
        private Vector3 floatAnchor;
        //private Graphic bubbleGraphic = null;

        private void updateFloatAnchor()
        {
            floatAnchor = parent.TrueCenter();
            floatAnchor.y = Altitudes.AltitudeFor(AltitudeLayer.MapDataOverlay);
            // we go up a bit to float atop the buildings
            floatAnchor.z += 2f;
            // we go half the floating size further back
            floatAnchor -= FloatSize / 2;

            // make sure don't go beyond the map render boundries
            floatAnchor.x = Math.Max(floatAnchor.x, 0f);
            floatAnchor.x = Math.Min(parent.Map.Size.x - FloatSize.x, floatAnchor.x);
            floatAnchor.z = Math.Max(floatAnchor.z, 0f);
            floatAnchor.z = Math.Min(parent.Map.Size.z - FloatSize.z, floatAnchor.z);
        }

        public List<Bubble> bubbles = new List<Bubble>();

        public void SpawnBubble(BubbleType type)
        {
            var newBubble = new Bubble();
            newBubble.Position = floatAnchor + FloatSize / 2f;
            newBubble.Position.z += 2f;
            newBubble.Type = type;
            Log.Debug($"spawning bubble at {newBubble.Position}");
            bubbles.Add(newBubble);
        }

        void popBubbleEffect(Bubble bubble)
        {
            Log.Debug($"popping bubble at {bubble.Position}");
            /*
            Color puffEffectColor = new Color(0.8f, 0.8f, 0.8f);
            switch (bubble.Type)
            {
                case BubbleType.Mood:
                    puffEffectColor = new Color(0.0f, 0.8f, 0.8f); // cyan
                    break;
                case BubbleType.Comfort:
                    puffEffectColor = new Color(0.8f, 0.8f, 0.0f); // yellow
                    break;
                case BubbleType.Recreation:
                    puffEffectColor = new Color(0.0f, 0.8f, 0.0f); // green
                    break;
                case BubbleType.Sleep:
                    puffEffectColor = new Color(0.0f, 0.0f, 0.8f); // blue
                    break;
            }

            FleckMaker.Static(new IntVec3(bubble.Position), parent.Map, FleckDefOf.PsycastAreaEffect);
            FleckMaker.ThrowDustPuffThick(
                new Vector3(bubble.Position.x, AltitudeLayer.MoteOverhead.AltitudeFor(), bubble.Position.z) + Vector3Utility.RandomHorizontalOffset(1.5f),
                parent.Map,
                Rand.Range(2.5f, 5f),
                puffEffectColor
                );*/

            FleckMaker.Static(new IntVec3(bubble.Position), parent.Map, FleckDefOf.PsycastAreaEffect);

            //DefOfs_SettledIn.BubblePopping.PlayOneShot(new TargetInfo(new IntVec3(bubble.Position), parent.Map));
            DefOfs_SettledIn.BubblePopping.PlayOneShotOnCamera();
        }

        const float BubbleValuePercent = 0.15f;
        const float BubbleValueManagementBufferPercent = 0.04f;
        protected virtual void Notify_BubbleBurst(Bubble bubble)
        {
            var colonistsWithNeeds = parent.Map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist && pawn.needs != null; }).ToArray();
            if (colonistsWithNeeds == null)
                return;
            switch (bubble.Type)
            {
                case BubbleType.Sleep:
                    var colonistsCanBeCredited = colonistsWithNeeds.Where(pawn => { return pawn.needs.rest != null && pawn.needs.rest.CurLevelPercentage < 1.0f; }).ToArray();
                    if (colonistsCanBeCredited == null || colonistsCanBeCredited.Length < 1)
                        return;
                    var targetColonist = colonistsCanBeCredited[Rand.Range(0, colonistsCanBeCredited.Length)];
                    targetColonist.needs.rest.CurLevelPercentage += BubbleValuePercent;
                    Messages.Message($"{targetColonist} feels he can go a little longer. For the settlement!", MessageTypeDefOf.PositiveEvent);
                    break;
                case BubbleType.Comfort:
                    colonistsCanBeCredited = colonistsWithNeeds.Where(pawn => { return pawn.needs.comfort != null && pawn.needs.comfort.CurLevelPercentage < 1.0f; }).ToArray();
                    if (colonistsCanBeCredited == null || colonistsCanBeCredited.Length < 1)
                        return;
                    targetColonist = colonistsCanBeCredited[Rand.Range(0, colonistsCanBeCredited.Length)];
                    targetColonist.needs.comfort.CurLevelPercentage += BubbleValuePercent;
                    Messages.Message($"{targetColonist} pushes through the need for comfort. For the settlement!", MessageTypeDefOf.PositiveEvent);
                    break;
                case BubbleType.Mood:
                    colonistsCanBeCredited = colonistsWithNeeds.Where(pawn => { return pawn.needs.mood != null && pawn.needs.mood.CurLevelPercentage < 1.0f; }).ToArray();
                    if (colonistsCanBeCredited == null || colonistsCanBeCredited.Length < 1)
                        return;
                    targetColonist = colonistsCanBeCredited[Rand.Range(0, colonistsCanBeCredited.Length)];
                    targetColonist.needs.mood.CurLevelPercentage += BubbleValuePercent;
                    Messages.Message($"{targetColonist} gets a sudden push in mood. For the settlement!", MessageTypeDefOf.PositiveEvent);
                    break;
                case BubbleType.Recreation:
                    colonistsCanBeCredited = colonistsWithNeeds.Where(pawn => { return pawn.needs.joy != null && pawn.needs.joy.CurLevelPercentage < 1.0f; }).ToArray();
                    if (colonistsCanBeCredited == null || colonistsCanBeCredited.Length < 1)
                        return;
                    targetColonist = colonistsCanBeCredited[Rand.Range(0, colonistsCanBeCredited.Length)];
                    targetColonist.needs.joy.CurLevelPercentage += BubbleValuePercent;
                    Messages.Message($"{targetColonist} pushes through the need for recreation. For the settlement!", MessageTypeDefOf.PositiveEvent);
                    break;
                case BubbleType.Buffer:
                    var resources = parent.Map.GetComponent<MapComponent_SettlementResources>();
                    if (resources != null)
                    {
                        var incrementBufferBy = (int)(BubbleValueManagementBufferPercent * resources.ManagementBuffer_max);
                        resources.ManagementBuffer_current = Mathf.Clamp(resources.ManagementBuffer_current + incrementBufferBy, resources.ManagementBuffer_current, resources.ManagementBuffer_max);
                        Messages.Message("People have organized themselves. For the settlement!", MessageTypeDefOf.PositiveEvent);
                    }
                    break;
            }
        }

        const float MaxVelocity = 0.15f;
        const float MaxVelocityChange = 0.03f;
        void moveBubble(Bubble bubble)
        {
            // update velocity
            var velocityChangeX = 2 * (Rand.Value - 0.5f) * MaxVelocityChange;
            var dampeningPercentX = (bubble.Position.x - floatAnchor.x) / FloatSize.x;
            if (dampeningPercentX < FloatDampeningFramePercent)
            {
                velocityChangeX = Math.Abs(velocityChangeX);
            }
            if (dampeningPercentX > 1f - FloatDampeningFramePercent)
            {
                velocityChangeX = -1 * Math.Abs(velocityChangeX);
            }
            bubble.Velocity.x += velocityChangeX;
            bubble.Velocity.x = Math.Max(bubble.Velocity.x, -1*MaxVelocity);
            bubble.Velocity.x = Math.Min(MaxVelocity, bubble.Velocity.x);

            var velocityChangeZ = 2 * (Rand.Value - 0.5f) * MaxVelocityChange;
            var dampeningPercentZ = (bubble.Position.z - floatAnchor.z) / FloatSize.z;
            if (dampeningPercentZ < FloatDampeningFramePercent)
            {
                velocityChangeZ = Math.Abs(velocityChangeZ);
            }
            if (dampeningPercentZ > 1f - FloatDampeningFramePercent)
            {
                velocityChangeZ = -1* Math.Abs(velocityChangeZ);
            }
            bubble.Velocity.z += velocityChangeZ;
            bubble.Velocity.z = Math.Max(bubble.Velocity.z, -1 * MaxVelocity);
            bubble.Velocity.z = Math.Min(MaxVelocity, bubble.Velocity.z);

            // update position
            bubble.Position.x += bubble.Velocity.x;
            bubble.Position.z += bubble.Velocity.z;
            // we dont clamp - the existing calculation restrictions should keep the bubble in the window
        }

        const int mouseCaptureDistanceInWorldTiles = 1;

        /*

        private bool isMouseOver(Bubble bubble)
        {
            var mousePos = UI.MousePositionOnUI;
            var uiRect = GetTextureRect(bubble.Position, mouseCaptureDistanceInWorldTiles);
            var inRangeX = mousePos.x > uiRect.position.x && mousePos.x - uiRect.position.x < uiRect.width;
            var inRangeY = mousePos.y > uiRect.position.y && mousePos.y - uiRect.position.y < uiRect.height;
            var inRange = inRangeX && inRangeY;
            return inRange;
        }

        /// <summary>
        /// returns the detection rectangle around a world position in ui space
        /// </summary>
        /// <param name="position">center place</param>
        /// <param name="worldSize">tiles distance to detect hovering</param>
        /// <returns></returns>
        private Rect GetTextureRect(Vector3 position, float worldSize)
        {
            // 1. Get the center in UI Space (Top-Left 0,0 + UI Scale handled)
            Vector2 centerUI = position.MapToUIPosition();

            // 2. Get the edge in UI Space to determine the current scaled pixel size
            Vector2 edgeUI = (position + new Vector3(worldSize, worldSize, worldSize)).MapToUIPosition();
            float radiusUiX = Math.Abs(edgeUI.x - centerUI.x);
            float radiusUiY = Math.Abs(edgeUI.y - centerUI.y);

            Rect detectionRectOnUI = new Rect(
                centerUI.x - radiusUiX,
                centerUI.y - radiusUiY,
                radiusUiX * 2,
                radiusUiY * 2
            );

            //Log.Debug($"item ui position is {centerUI}. zoom scaling is {radiusUiX} based on calculated ui reference point {edgeUI}. That gives a detection rectangle of {detectionRectOnUI} while the mouse was at {UI.MousePositionOnUI}");
            // 3. Return the rect centered on the bubble
            return detectionRectOnUI;
        }

        
        public override void PostDraw()
        {
            Log.DebugOnce("at least CompClickBaiter.PostDraw() is gettign called..");
            base.PostDraw();

            //Vector3 buildingDrawPos = parent.DrawPos;
            //var drawSize = new Vector2(parent.DrawSize.x, parent.DrawSize.y);

            foreach (var bubble in bubbles.ToArray())
            {
                if (!Find.TickManager.Paused && isMouseOver(bubble))
                {
                    Notify_BubbleBurst(bubble);
                    bubbles.Remove(bubble);
                    popBubbleEffect(bubble);
                }
                else
                {
                    Material material = null;
                    switch (bubble.Type)
                    {
                        case BubbleType.Comfort:
                            material = Bubble_Comfort;
                            break;
                        case BubbleType.Mood:
                            material = Bubble_Mood;
                            break;
                        case BubbleType.Recreation:
                            material = Bubble_Recreation;
                            break;
                        case BubbleType.Sleep:
                            material = Bubble_Sleep;
                            break;
                        case BubbleType.Buffer:
                            material = Bubble_Buffer;
                            break;
                    }

                    // Create a scaling matrix to control the size of the image
                    Matrix4x4 matrix = Matrix4x4.TRS(bubble.Position, Quaternion.identity, new Vector3(1.5f, 1, 1.5f));

                    // Draw the mesh with the specified material and matrix
                    Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
                }

            }
        }

        public override void CompTick()
        {
            Log.DebugOnce("at least CompClickBaiter.CompTick() is getting called..");
            var ticks = Find.TickManager.TicksGame;

            // every 30 minutes, there is a chance of 12.5% to spawn a bubble -> averaging at 6 bubbles + 6 buffer bubbles a day
            if (ticks % 1250 == 0 && Rand.Chance(0.125f))
            {
                updateFloatAnchor();
                BubbleType nextType = (BubbleType)Rand.Range(0, 5);
                SpawnBubble(nextType);
                var spawnExtraBuffer = Rand.Chance(0.5f);
                if (spawnExtraBuffer)
                {
                    SpawnBubble(BubbleType.Buffer);
                }
            }

            // despawn bubbles
            var toDelete = bubbles.Where(bubble => { return bubble.despawnTick < ticks; }).ToArray();
            foreach (var bubble in toDelete)
            {
                bubbles.Remove(bubble);
                //popBubbleEffect(bubble); // no effect when not popped by hand?
            }

            if (ticks % 4 == 0) // 15 fps
            {
                updateFloatAnchor();
                // draw bubbles
                foreach (var bubble in bubbles)
                {
                    moveBubble(bubble);
                }
            }
        }
        */
        

        // proposal below:
        
        // Helper to keep PostDraw clean
        private Material GetBubbleMaterial(BubbleType type)
        {
            switch (type)
            {
                case BubbleType.Comfort: return Bubble_Comfort;
                case BubbleType.Mood: return Bubble_Mood;
                case BubbleType.Recreation: return Bubble_Recreation;
                case BubbleType.Sleep: return Bubble_Sleep;
                case BubbleType.Buffer: return Bubble_Buffer;
                default: return null;
            }
        }

        public override void PostDraw()
        {
            Log.DebugOnce("at least CompClickBaiter.PostDraw() is gettign called..");

            for (int i = 0; i < bubbles.Count; i++)
            {
                Bubble bubble = bubbles[i];
                Material material = GetBubbleMaterial(bubble.Type);

                if (material != null)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(bubble.Position, Quaternion.identity, new Vector3(1.5f, 1f, 1.5f));
                    Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
                }
            }
        }

        public override void CompTick()
        {
            CompTickIntervalInternal(1);
        }

        /// <summary>
        /// We will not use the CompTickInterval method as that makes the bubbles float too hacky. But since we already refactor the method to use the 
        /// interval parameter - we keep it for now.
        /// </summary>
        public void CompTickIntervalInternal(int interval)
        {
            updateFloatAnchor();
            // 1. Spawning Logic (Every ~1250 ticks)
            // We check if the current tick falls within the 'interval' window
            if (Find.TickManager.TicksGame % 1250 < interval && Rand.Chance(0.125f) )
            {
                SpawnBubble((BubbleType)Rand.Range(0, 5));
                if (Rand.Chance(0.5f)) SpawnBubble(BubbleType.Buffer);
            }

            // 2. Movement and Despawning
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                Bubble b = bubbles[i];

                // Despawn check
                if (b.despawnTick < Find.TickManager.TicksGame)
                {
                    bubbles.RemoveAt(i);
                    continue;
                }

                // Movement (Scaled by interval)
                // This ensures bubbles move the same distance even if the tick rate changes; one step every 4 ticks is a fine speed in usual cases
                // this is not exact science, as strange tick invervals might execute too often, e.g. a step every tick = max
                var steps = Math.Max(interval/4, 1);
                for (int step = 0; step < steps; step++)
                {
                    moveBubble(b);
                }
            }

            // 3. catch bubbles on mouse over
            if (bubbles.Count == 0)
                return;
            if (Find.TickManager.Paused) // we only catch when unpaused
                return;
            if (Find.WindowStack.MouseObscuredNow) // we only catch when not behind a window
                return;

            //Screen Culling: Skip if the building isn't on screen
            CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            viewRect = viewRect.ExpandedBy(2);
            if (!viewRect.Contains(parent.Position))
                return;

            Vector3 mouseWorldPos = UI.MouseMapPosition();
            const float worldRadiusSq = mouseCaptureDistanceInWorldTiles * mouseCaptureDistanceInWorldTiles;

            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                // 3. Simple 2D distance check in world space (X and Z)
                // We ignore Y because bubbles float at different altitudes
                float dx = mouseWorldPos.x - bubbles[i].Position.x;
                float dz = mouseWorldPos.z - bubbles[i].Position.z;
                float distSq = (dx * dx) + (dz * dz);

                if (distSq < worldRadiusSq)
                {
                    Notify_BubbleBurst(bubbles[i]);
                    popBubbleEffect(bubbles[i]);
                    bubbles.RemoveAt(i);
                }
            }

            /*
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                if (isMouseOver(bubbles[i]))
                {
                    Notify_BubbleBurst(bubbles[i]);
                    popBubbleEffect(bubbles[i]);
                    bubbles.RemoveAt(i);
                }
            }*/
        }
        
        // end of proposal

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref bubbles, "bubbles", LookMode.Deep);

            // Ensure the list is not null after loading
            if (Scribe.mode == LoadSaveMode.LoadingVars && bubbles == null)
            {
                bubbles = new List<Bubble>();
            }
        }
    }
}
