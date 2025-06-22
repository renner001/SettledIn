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

        const float MaxVelocity = 0.3f;
        const float MaxVelocityChange = 0.05f;
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

        public override void PostDraw()
        {
            Log.DebugOnce("at least CompClickBaiter.PostDraw() is gettign called..");
            base.PostDraw();
            
            //Vector3 buildingDrawPos = parent.DrawPos;
            //var drawSize = new Vector2(parent.DrawSize.x, parent.DrawSize.y);

            foreach (var bubble in bubbles.ToArray())
            {

                if (/*Event.current.type == EventType.MouseDown && Event.current.button == 0 &&*/ !Find.TickManager.Paused && Mouse.IsOver(GetTextureRect(bubble.Position, 1)))
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

        private Rect GetTextureRect(Vector3 position, float size)
        {
            // Convert the world position to screen position
            Vector3 screenPosition = Find.Camera.WorldToScreenPoint(position);
            screenPosition.y = Screen.height - screenPosition.y; // Invert Y for Unity's screen coordinates

            // Define a Rect around the screen position with the given size
            return new Rect(screenPosition.x - (size * 50), screenPosition.y - (size * 50), size * 100, size * 100);
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
