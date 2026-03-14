using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public class Comp_TradingDepotRefuelable : CompRefuelable
    {
        public ThingDef CurrentFuelDef;
        public bool replacedProp = false;
        public CompProperties_TradingDepotRefuelable originalProps;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            EnsureInstanceFilter();
            UpdateFuelFilter();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref CurrentFuelDef, "CurrentFuelDef");
            // After loading make sure filter matches again
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureInstanceFilter();
                UpdateFuelFilter();
            }
        }

        // Make sure we have an instance-local ThingFilter instead of using the shared props.fuelFilter object.
        private void EnsureInstanceFilter()
        {
            if (replacedProp)
                return;
            Log.Debug($"replacing real CompProperties_TradingDepotRefuelable by runtime duplicate on {this.parent}");
            this.originalProps = this.props as CompProperties_TradingDepotRefuelable;
            // no clone() methods? Urgh.
            var newProps = new CompProperties_TradingDepotRefuelable();
            newProps.allowRefuelIfNotEmpty = originalProps.allowRefuelIfNotEmpty;
            newProps.atomicFueling = originalProps.atomicFueling;
            newProps.autoRefuelPercent = originalProps.autoRefuelPercent;
            newProps.fuelLabel = originalProps.fuelLabel;
            newProps.canEjectFuel = originalProps.canEjectFuel;
            newProps.consumeFuelOnlyWhenPowered = originalProps.consumeFuelOnlyWhenPowered;
            newProps.consumeFuelOnlyWhenUsed = originalProps.consumeFuelOnlyWhenUsed;
            newProps.destroyOnNoFuel = originalProps.destroyOnNoFuel;
            newProps.drawFuelGaugeInMap = originalProps.drawFuelGaugeInMap;
            newProps.drawOutOfFuelOverlay = originalProps.drawOutOfFuelOverlay;
            newProps.externalTicking = originalProps.externalTicking;
            newProps.factorByDifficulty = originalProps.factorByDifficulty;
            newProps.fuelCapacity = originalProps.fuelCapacity;
            newProps.fuelConsumptionPerTickInRain = originalProps.fuelConsumptionPerTickInRain;
            newProps.fuelConsumptionRate = originalProps.fuelConsumptionRate;
            newProps.fuelGizmoLabel = originalProps.fuelGizmoLabel;
            newProps.functionsInVacuum = originalProps.functionsInVacuum;
            newProps.outOfFuelMessage = originalProps.outOfFuelMessage;
            newProps.showAllowAutoRefuelToggle = originalProps.showAllowAutoRefuelToggle;
            newProps.showFuelGizmo = originalProps.showFuelGizmo;  
            newProps.targetFuelLevelConfigurable = originalProps.targetFuelLevelConfigurable;

            newProps.fuelFilter = new ThingFilter();

            this.props = newProps;

            replacedProp = true;
           
        }

        // Set the fuel filter to only allow CurrentFuelDef (or allow all if null)
        public void UpdateFuelFilter()
        {
            Log.Debug($"updating filter for Comp_TradingDepotRefuelable to {CurrentFuelDef}");
            // Ensure filter instance exists
            EnsureInstanceFilter();

            var refuelProps = this.props as CompProperties_Refuelable;

            // Clear and set allowances
            refuelProps.fuelFilter.SetDisallowAll();
            if (CurrentFuelDef != null)
            {
                refuelProps.fuelFilter.SetAllow(CurrentFuelDef, true);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (CurrentFuelDef == null)
                return "not trading anything right now";
            return $"Trading stock of {Fuel} {CurrentFuelDef.LabelCap}";
        }

    }
}
