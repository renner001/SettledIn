using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{
    [StaticConstructorOnStartup]
    public static class Textures_SettledIn
    {
        public static Texture2D GenericBuilding = ContentFinder<Texture2D>.Get("RoomIcons/Generic", false);
        public static Texture2D Checkmark = ContentFinder<Texture2D>.Get("RoomIcons/checked", false);

        // icons for benefits
        public static Texture2D Benefit_lvl0_SettlementCenter = ContentFinder<Texture2D>.Get("Benefit_lvl0_SettlementCenter", false);
        public static Texture2D Benefit_lvl1_Immigrants = ContentFinder<Texture2D>.Get("Benefit_lvl1_Immigrants", false);
        public static Texture2D Benefit_lvl2_TraderStock = ContentFinder<Texture2D>.Get("Benefit_lvl2_TraderStock", false);
        public static Texture2D Benefit_lvl3_ColonistFocus = ContentFinder<Texture2D>.Get("Benefit_lvl3_ColonistFocus", false);
        public static Texture2D Benefit_lvl4_RoomFocus = ContentFinder<Texture2D>.Get("Benefit_lvl4_RoomFocus", false);
        public static Texture2D Benefit_lvl4_PersonalWorkbench = ContentFinder<Texture2D>.Get("Benefit_lvl4_PersonalWorkbench", false);
        public static Texture2D Benefit_lvl5_CozyRooms = ContentFinder<Texture2D>.Get("Benefit_lvl5_CozyRooms", false);
        public static Texture2D Benefit_lvl5_ManagedSettlements = ContentFinder<Texture2D>.Get("Benefit_lvl5_ManagedSettlements", false);
        public static Texture2D Benefit_lvl6_RecreationFocus = ContentFinder<Texture2D>.Get("Benefit_lvl6_RecreationFocus", false);
        public static Texture2D Benefit_lvl6_TidySettlements = ContentFinder<Texture2D>.Get("Benefit_lvl6_TidySettlements", false);

        // icons for UI elements
        public static Texture2D UpgradeSettlementIcon = ContentFinder<Texture2D>.Get("UpgradeSettlement", false);
        public static Texture2D Unknown = ContentFinder<Texture2D>.Get("Unknown", false);
        public static Texture2D MenuIcon = ContentFinder<Texture2D>.Get("SettlementScoreMenuIcon", false);
        public static Texture2D ScoreIcon = ContentFinder<Texture2D>.Get("Score", false);
        public static Texture2D BackgroundShade = ContentFinder<Texture2D>.Get("BackgroundShade", false);

        // icons for settlement center stages
        public static Texture2D SettlementCenter0 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter0", false);
        public static Texture2D SettlementCenter1 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter1", false);
        public static Texture2D SettlementCenter2 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter2", false);
        public static Texture2D SettlementCenter3 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter3", false);
        public static Texture2D SettlementCenter3glow = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter3glow", false);
        public static Texture2D SettlementCenter4 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter4", false);
        public static Texture2D SettlementCenter4glow = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter4glow", false);
        public static Texture2D SettlementCenter5 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter5", false);
        public static Texture2D SettlementCenter5glow = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter5glow", false);
        public static Texture2D SettlementCenter6 = ContentFinder<Texture2D>.Get("Building_SettlementCenter/SettlementCenter6", false);

        // Arktech Personal Aaccess Point
        public static Texture2D Arktech_PAP_thumbnail = ContentFinder<Texture2D>.Get("Building_Arktech_PAP/Arktech_PAP", false);
        public static Texture2D Arktech_PAP = ContentFinder<Texture2D>.Get("Building_Arktech_PAP/Arktech_PAP_fullSize", false);
        public static Texture2D Arktech_PAP_glow_4 = ContentFinder<Texture2D>.Get("Building_Arktech_PAP/Arktech_PAP_glow_4", false);

    }
}
