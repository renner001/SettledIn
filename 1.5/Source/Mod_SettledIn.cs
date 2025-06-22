using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{
    [StaticConstructorOnStartup]
    public class Mod_SettledIn : Mod
    {
        static Mod_SettledIn()
        {
            Verse.Log.Message("Mod 'Settled In': loaded");
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Harmony harmony = new Harmony("DanielRenner.SettledIn");
            harmony.PatchAll();
        }

        public Mod_SettledIn(ModContentPack mcp) : base(mcp)
        {
            LongEventHandler.ExecuteWhenFinished(() => {
                base.GetSettings<ModSettings_SettledIn>();
            });
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }


        public override string SettingsCategory()
        {
            return Translations_SettledIn.Static.SettingsPanelName;
        }


        public override void DoSettingsWindowContents(Rect rect)
        {
            // we will put the rendering code into the settings class - where it belongs...
            ModSettings_SettledIn.DoSettingsWindowContents(rect);
        }
    }
}
