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

    public class Building_TableSettlementOffice : Building
    {


        public override string GetInspectString()
        {
            var settlementResources = Map.GetComponent<MapComponent_SettlementResources>();
            var inspectStringAddition = "\nCurrent settlement organization level: " + Math.Floor((float)settlementResources.ManagementBuffer_current * 100 / settlementResources.ManagementBuffer_max) + "%";
#if DEBUG
            inspectStringAddition += "\nsettlement buffer: " + settlementResources.ManagementBuffer_current;
            inspectStringAddition += "\nmax settlement buffer: " + settlementResources.ManagementBuffer_max;
#endif
            var baseString = base.GetInspectString();
            return baseString + inspectStringAddition;
        }
 
    }
}
