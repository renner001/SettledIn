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
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class Patch_Frame_CompleteConstruction
    {
        public static void Postfix(Frame __instance, ref Pawn worker)
        {
            Log.DebugOnce("patch Patch_Frame_CompleteConstruction.Postfix() is getting called...");

            Map currentMap = Find.CurrentMap;
            if (currentMap != null && currentMap.IsPlayerHome)
            {
                var settlementScoreManager = currentMap.GetComponent<MapComponent_SettlementResources>();
                if (settlementScoreManager != null) 
                {
                    // get interesting property work amount
                    var itemDefBuilt = __instance.def.entityDefToBuild as BuildableDef;
                    var work = 0f;
                    if (itemDefBuilt != null)
                    {
                        work = StatDefOf.WorkToBuild.Worker.GetValueAbstract(itemDefBuilt);
                    }

                    settlementScoreManager.Notify_Construction(work);
                }
            }

        }
    }

}
