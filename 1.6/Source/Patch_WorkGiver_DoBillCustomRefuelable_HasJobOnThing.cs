using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using Verse;
using Verse.AI;

namespace DanielRenner.SettledIn
{
    [StaticConstructorOnStartup]
    public class ModInit : Mod
    {
        static ModInit()
        {
            Verse.Log.Message("Patches for Medieval Overhaul to mod 'Settled In': loaded");
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Harmony harmony = new Harmony("DanielRenner.SettledIn.MedievalOverhaulPatches");
            harmony.PatchAll();
        }

        public ModInit(ModContentPack mcp) : base(mcp)
        {
        }
    }

    /* was removed with V1.5
    [HarmonyPatch(typeof(WorkGiver_DoBillCustomRefuelable), nameof(WorkGiver_DoBillCustomRefuelable.JobOnThing))]
    public static class Patch_WorkGiver_DoBillCustomRefuelable_JobOnThing
    {
        public static bool Prefix(WorkGiver_DoBill __instance, ref Job __result, Pawn pawn, Thing thing, bool forced)
        {
            Log.DebugOnce("patch Patch_WorkGiver_DoBillCustomRefuelable_JobOnThing.Prefix() is getting called...");
            if (forced)
            {
                return true;
            }
            if (thing == null)
            {
                return true;
            }
            var assignableComp = thing.TryGetComp<CompAssignableToPawn>();
            if (assignableComp != null && assignableComp.AssignedPawnsForReading != null && assignableComp.AssignedPawnsForReading.Count > 0 && !assignableComp.AssignedPawnsForReading.Contains(pawn))
            {
                __result = null;
                return false;
            }
            return true;
        }
    }

    */
    
}
