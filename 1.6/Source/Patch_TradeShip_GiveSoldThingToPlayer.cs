using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace DanielRenner.SettledIn
{
    /*
     * Sadly, the rimworld code does not allow overriding the trade ship method TradeShip.GiveSoldThingToPlayer()
     * So we have to override it in a patch.
     * */
    [HarmonyPatch(typeof(TradeShip), nameof(TradeShip.GiveSoldThingToPlayer))]
    public static class Patch_TradeShip_GiveSoldThingToPlayer
    {
        public static bool Prefix(TradeShip __instance, Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Log.DebugOnce("patch Patch_TradeShip_GiveSoldThingToPlayer.Prefix() is getting called...");
            if (!(__instance is TradeShip_SettledIn))
                return true;
            (__instance as TradeShip_SettledIn).GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
            return false;
        }
    }
}
