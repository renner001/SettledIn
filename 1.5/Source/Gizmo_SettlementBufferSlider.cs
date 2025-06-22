using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace DanielRenner.SettledIn
{
    public class Gizmo_SettlementBufferSlider : Gizmo_Slider
    {
        public Gizmo_SettlementBufferSlider(Map map)
        {
            this.map = map;
        }

        private Map map;

        protected override float Target
        {
            get
            {
                var resources = map?.GetComponent<MapComponent_SettlementResources>();
                if (resources != null)
                {
                    return resources.ManagementBuffer_max;
                }
                Log.Warning("Gizmo_SettlementBufferSlider.Target value cannot be generated as the MapComponent_SettlementResources could not be found.");
                return 0;
            }
            set
            {
                Log.Warning("someone tried to set the Gizmo_SettlementBufferSlider.Target value which is not supported. This simply has no effect at all");
            }
        }

        protected override bool IsDraggable 
        {
            get { return false; }
        }

        protected override float ValuePercent
        {
            get
            {
                var resources = map?.GetComponent<MapComponent_SettlementResources>();
                if (resources != null && resources.ManagementBuffer_max != 0)
                {
                    return (float)resources.ManagementBuffer_current / resources.ManagementBuffer_max;
                }
                Log.Warning("Gizmo_SettlementBufferSlider.ValuePercent value cannot be generated as the MapComponent_SettlementResources could not be found.");
                return 0f;
            }
        }

        protected override string Title
        {
            get
            {
                return "DanielRenner.SettledIn.Gizmo_SettlementBufferSliderTitle".Translate();
            }
        }

        protected override string GetTooltip()
        {
            return "DanielRenner.SettledIn.Gizmo_SettlementBufferSliderTooltip".Translate();
        }
    }
}
