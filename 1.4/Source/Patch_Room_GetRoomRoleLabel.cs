using HarmonyLib;
using HugsLib.Utils;
using LabelsOnFloor;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    [StaticConstructorOnStartup]
    public class ModInit : Mod
    {
        static ModInit()
        {
            Verse.Log.Message("Patches for LabelsOnFloor to mod 'Settled In': loaded");
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Harmony harmony = new Harmony("DanielRenner.SettledIn.LabelsOnFloorPatches");
            harmony.PatchAll();
        }

        public ModInit(ModContentPack mcp) : base(mcp)
        {
        }
    }

    
    [HarmonyPatch(typeof(Room), "GetRoomRoleLabel")]
    public static class Patch_Room_GetRoomRoleLabel
    {
        public static bool Prefix(Room __instance, ref string __result)
        {
            DanielRenner.SettledIn.Log.DebugOnce("Patch_Room_GetRoomRoleLabel.Prefix() is getting called...");
            CustomRoomLabelManager roomLabelManager = UtilityWorldObjectManager.GetUtilityWorldObject<CustomRoomLabelManager>();
            if (roomLabelManager != null && roomLabelManager.IsRoomCustomised(__instance))
            {
                var label = roomLabelManager.GetCustomLabelFor(__instance);
                __result = label;
                return false;
            }
            return true;
        }
    }
    
}
