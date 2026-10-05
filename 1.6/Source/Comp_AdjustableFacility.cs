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
    /// <summary>
    /// Supports 3 things:
    /// - it is a facility, therefore supports linking up with other buildings
    /// - a variable range that can be configured
    /// - turns disabled if the facility needs a specific room type
    /// </summary>
    public class Comp_AdjustableFacility : CompFacility
    {
        private float currentRange;
        public float CurrentRange
        {
            get { return currentRange; }
            set { currentRange = value; Notify_ThingChanged(); }
        }

        public new CompProperties_AdjustableFacility Props => (CompProperties_AdjustableFacility)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                CurrentRange = Props.maxDistance;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentRange, "currentRange", Props.maxDistance);
        }

        public override void CompTick()
        {
            // Only run the logic every 200 ticks
            if (parent.IsHashIntervalTick(200))
            {
                CompTickRare();
            }
        }

        public override void CompTickRare()
        {
            Log.DebugOnce("at least Comp_AdjustableFacility.CompTickRare() is getting called..");
            if (!parent.Spawned || !parent.Map.IsPlayerHome)
                return;
            
            HediffDef districtHediff = Props.appliedHediff;
            if (districtHediff == null)
                return;

            if (!CanBeActive) 
                return;

            // Get the radius from adjustable property
            float radiusSq = CurrentRange * CurrentRange;

            // Get all pawns on the map - sadly there is no performance increase feasible for me
            var allPawns = parent.Map.mapPawns.AllHumanlikeSpawned;

            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn pawn = allPawns[i];

                // DistanceSquared is significantly faster than Distance
                if (pawn.Position.DistanceToSquared(parent.Position) <= radiusSq)
                {
                    // Apply or Refresh the Hediff
                    // Giving it a short duration (e.g. 10 seconds) means it 
                    // disappears automatically if they walk out of range.
                    

                    Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(districtHediff);
                    if (existing == null)
                    {
                        pawn.health.AddHediff(districtHediff);
                    }
                    else
                    {
                        // Reset age so it doesn't expire while in range
                        existing.ageTicks = 0;
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
                yield return g;

            yield return new Command_Action
            {
                defaultLabel = $"Set range: {CurrentRange:F1}",
                defaultDesc = "Adjust the maximum link range for this facility.",
                icon = ContentFinder<Texture2D>.Get("ChangeRange", true),
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_Slider(
                        "Adjust Link Range",
                        (val) => CurrentRange = val,
                        Props.maxDistanceFrom, // min
                        Props.maxDistance, // max
                        CurrentRange // start
                    ));
                }
            };
        }

        public override bool CanBeActive
        {
            get
            {
                Log.DebugOnce("at least Comp_AdjustableFacility.CanBeActive is gettign called..");
                /* no, we do not inherit from the normal facility - who knows what Ludeon will change next?
                // Check if the parent (power) logic says it can be active
                if (!base.CanBeActive) 
                    return false;
                */

                // Check if the custom room role requirement exists
                Room room = this.parent.GetRoom();
                var props = this.parent.def.GetModExtension<AdjacencyProperties>();
                if (props != null && props.requiredRoomRole != null)
                {
                    Log.DebugOnce($"checking Comp_AdjustableFacility of {this.parent} with AdjacencyProperties CanBeActive(): Expected rooom role {props.requiredRoomRole} and real room role {room?.Role}");
                    // If the room changed and no longer matches the role, return false
                    return room != null && props.requiredRoomRole.Contains(room.Role.defName);
                }

                return true;
            }
        }

        List<RoomRoleDef> cachedRoomRoles = null;

        public override string CompInspectStringExtra()
        {
            if (!CanBeActive)
            {
                Room room = this.parent.GetRoom();
                var props = this.parent.def.GetModExtension<AdjacencyProperties>();
                var requiredRoomRole = props?.requiredRoomRole;
                if (cachedRoomRoles == null)
                {
                    cachedRoomRoles = new List<RoomRoleDef>();
                    foreach (var roomrolename in requiredRoomRole)
                    {
                        var foundDef = DefDatabase<RoomRoleDef>.GetNamed(roomrolename, false);
                        if (foundDef != null && !cachedRoomRoles.Contains(foundDef))
                            cachedRoomRoles.Add(foundDef);
                    }
                }

                return $"Inactive: must be in one of: {String.Join(", ", cachedRoomRoles.Select(role => role.label))}";
            }
            return base.CompInspectStringExtra();
        }
    }
}
