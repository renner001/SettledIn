using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DanielRenner.SettledIn
{
    [HarmonyPatch(typeof(StatPart_WorkTableRoomRole), nameof(StatPart_WorkTableRoomRole.Applies))]
    public static class Patch_StatPart_WorkTableRoomRole
    {
        public static void Postfix(StatPart_WorkTableRoomRole __instance, ref bool __result, Thing parent)
        {
            Log.DebugOnce($"patch Patch_StatPart_WorkTableRoomRole.Postfix() is getting called...");
            // we only undo some cases where result was true
            if (__result == false)
            {
                return;
            }
            // no building properties, nothing to alter...
            var buildingProperties = parent?.def?.building;
            if (buildingProperties == null || buildingProperties.workTableRoomRole == null)
            {
                return;
            }

            // only continue if the required room is one of the room types we created sub rooms for:
            var listOfOverridenRoomRoles = new RoomRoleDef[] { RoomRoleDefOf.Workshop };
            if (!listOfOverridenRoomRoles.Contains(buildingProperties.workTableRoomRole))
            {
                return;
            }

            Room room = parent.GetRoom(RegionType.Set_All);
            // only fix if there is a room
            if (room == null)
            {
                return;
            }
            var role = room.Role;

            var listOfWorkshopAlternatives = new RoomRoleDef[] { DefOfs_SettledIn.ArtworkStudio, DefOfs_SettledIn.Smithy, DefOfs_SettledIn.StoneworkStudio, DefOfs_SettledIn.DrugLab, DefOfs_SettledIn.MachiningLab, DefOfs_SettledIn.TailorShop };
            // we have to fix a few sub room types to belong to the parental room also
            if (buildingProperties.workTableRoomRole == RoomRoleDefOf.Workshop && listOfWorkshopAlternatives.Contains(role))
            {
                __result = false;
                Log.DebugOnce($"altered StatPart_WorkTableRoomRole.Applies() for room role {role} and expected room type {buildingProperties.workTableRoomRole} to {__result}...");
            }
        }
    }

}
