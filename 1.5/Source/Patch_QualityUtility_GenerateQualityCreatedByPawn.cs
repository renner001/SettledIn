using HarmonyLib;
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
    // patch to include the quality upgrade for rooms in a special room type
    // sadly, comps are not called for generatign the quality of item, only the pawn stats are checked, therefore we intercept the results generated and increase the quality
    // QualityCategory GenerateQualityCreatedByPawn(Pawn pawn, SkillDef relevantSkill)
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn", new[] { typeof(Pawn), typeof(SkillDef) })]
    internal class Patch_QualityUtility_GenerateQualityCreatedByPawn
    {
        static void Postfix(Pawn pawn, SkillDef relevantSkill, ref QualityCategory __result)
        {
            var roomPawnIsIn = pawn.GetRoom(RegionType.Set_All);
            var roomRole = roomPawnIsIn?.Role;
            // since these room types can only exist in rooms where there are no other production building types, we can skip checking what kind of product we are creating and
            // simply increase the quality
            if (   roomRole == DefOfs_SettledIn.ArtworkStudio 
                || roomRole == DefOfs_SettledIn.TailorShop 
                || roomRole == DefOfs_SettledIn.Smithy 
                || roomRole == DefOfs_SettledIn.StoneworkStudio 
                || roomRole == DefOfs_SettledIn.MachiningLab)
            {
                var settlementResources = pawn.Map != null ? pawn.Map.GetComponent<MapComponent_SettlementResources>() : null;
                if (settlementResources != null && SettlementLevelUtility.IsBenefitActiveAt(settlementResources.SettlementLevel, SettlementLevelUtility.Benefit_lvl1_RoomFocus))
                {
                    var randomNumber = Rand.Value;
                    // 25% chance to increase quality by 1
                    if (randomNumber < 0.25f)
                    {
                        var newQuality = (QualityCategory)Mathf.Min((int)(__result + (byte)1), 6);
                        Log.Debug("for pawn " + pawn + " in room " + roomPawnIsIn + ": increasing quality of what he makes from " + __result + " to " + newQuality);
                        __result = newQuality;
                    }
                }
            }
        }
    }
}
