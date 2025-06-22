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
    class CompUpgradeSettlement : ThingComp
    {
        public CompUpgradeSettlement()
        {
            Log.DebugOnce("CompUpgradeSettlement.CompUpgradeSettlement() at least comps are getting created...");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (this.parent.Faction == Faction.OfPlayer)
            {
                bool allegibleForUpdate = false;
                var settlementResources = parent.Map.GetComponent<MapComponent_SettlementResources>();
                if (settlementResources != null && settlementResources.SettlementLevel < SettlementLevelUtility.MaxLevel)
                {
                    string upgradeRequirements;
                    // todo: increase performance by using the cached values
                    allegibleForUpdate = SettlementLevelUtility.CheckRequirements(settlementResources.SettlementLevel + 1, parent.Map, out upgradeRequirements);
                }
                yield return new Command_Action
                {
                    //hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = Textures_SettledIn.UpgradeSettlementIcon,
                    disabled = !allegibleForUpdate,
                    disabledReason = "DanielRenner.SettledIn.UpgradeDeactivated".Translate(),
                    defaultLabel = "DanielRenner.SettledIn.UpgradeSettlementLabel".Translate(),
                    defaultDesc = "DanielRenner.SettledIn.UpgradeSettlementDescription".Translate(),
                    action = delegate
                    {
                        Log.Debug("CompUpgradeSettlement was clicked. Triggering settlement upgrade now");
                        // get settlement manager and trigger the upgrade
                        (this.parent as Building_SettlementCenter).UpgradeSettlement();
                    }
                };
                if (DebugSettings.ShowDevGizmos)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Upgrade Settlement",
                        action = delegate
                        {
                            Log.Debug("CompUpgradeSettlement was clicked. Triggering settlement downgrade now");
                            // get settlement manager and trigger the downgrade
                            (this.parent as Building_SettlementCenter).UpgradeSettlement();
                        }
                    };
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Downgrade Settlement",
                        action = delegate
                        {
                            Log.Debug("CompUpgradeSettlement was clicked. Triggering settlement downgrade now");
                            // get settlement manager and trigger the downgrade
                            settlementResources.DowngradeSettlement();
                        }
                    };
                }
                }
            yield break;
        }

        public override void DrawGUIOverlay()
        {
            Log.DebugOnce("CompUpgradeSettlement.DrawGUIOverlay() is getting called... ");
            base.DrawGUIOverlay();
        }
        
    }
}
