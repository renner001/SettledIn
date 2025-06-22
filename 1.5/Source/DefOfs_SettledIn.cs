using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    [DefOf]
    public static class DefOfs_SettledIn
    {
        public static MainButtonDef MainTabSettlementScores;

        public static ThingDef SettlementCenter;
        public static ThingDef TableSettlementOffice;
        public static ThingDef Commodity;

        public static JobDef ManageSettlement;
        public static JobDef CollectCommodities;

        public static ThoughtDef FewAssignedPriorities;
        public static ThoughtDef ManyAssignedPriorities;
        public static ThoughtDef RecreationSchedule;
        public static ThoughtDef WorkedOnPersonalWorkbench;
        public static ThoughtDef WorkedOnPublicWorkbench;
        public static ThoughtDef BeenConstructingLately;
        public static ThoughtDef LongCommutes;
        public static ThoughtDef UnlikedNeightbour;
        public static ThoughtDef UnlikedPrisonNeightbour;
        public static ThoughtDef OwnsNoPersonalWorkbench;
        public static ThoughtDef WantCommodities;

        public static RoomRoleDef Museum;
        public static RoomRoleDef DrugLab;
        public static RoomRoleDef Hydroponics;
        public static RoomRoleDef SettlementOffice;
        public static RoomRoleDef TailorShop;
        public static RoomRoleDef Smithy;
        public static RoomRoleDef StoneworkStudio;
        public static RoomRoleDef ArtworkStudio;
        public static RoomRoleDef MachiningLab;
        public static RoomRoleDef CommunityRoom;
        public static RoomRoleDef ElectricalRoom;
        public static RoomRoleDef GeneratorRoom;

        public static QuestScriptDef ImmigrantArrives;

        public static SoundDef BubblePopping;

        public static WorkTypeDef Managing { 
            get 
            {
                var ret = DefDatabase<WorkTypeDef>.AllDefs.FirstOrDefault(workTypeDef => { return workTypeDef.defName == "Managing"; });
                if (ret == null)
                {
                    Log.Error("found no managing work type!");
                }                
                return ret;
            } 
        }

    }
}
