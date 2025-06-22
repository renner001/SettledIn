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
    /// <summary>
    /// Contains all the business logic to generate the scores of rooms and room types
    /// </summary>
    public static class SettlementScoreUtility
    {
        static SettlementScoreUtility()
        {
            /* None */
            SkippedRoomTypes.Add("None");
            /*
            OverrideLabels.Add(RoomRoleDefOf.None.defName, "General Purpose Room");
            OverrideDescriptions.Add(RoomRoleDefOf.None.defName, "Four walls and probably a roof. One day, this may mature into a real purpose room?");
            OverrideTextures.Add(RoomRoleDefOf.None.defName, Textures_SettledIn.GenericBuilding);*/

            /* Room */
            InvalidRoomTypesForRoomTypeScore.Add("Room"); // general purpose rooms bring no bonus for having this type of room
            OverrideLabels.Add("Room", "General Purpose Room");
            OverrideDescriptions.Add("Room", "Four walls and probably a roof. One day, this may mature into a real purpose room?");
            OverrideTextures.Add("Room", Textures_SettledIn.GenericBuilding);

            /* Bedroom */
            OverrideLabels.Add(RoomRoleDefOf.Bedroom.defName, "Bedroom");
            OverrideDescriptions.Add(RoomRoleDefOf.Bedroom.defName, "A private place to sleep for one pawn with or without spouse.\nContains a sleeping spot or bed and having this type of room yields a bonus to the settlement score.");
            //todo: draw new image OverrideTextures.Add(RoomRoleDefOf.Barracks.defName, Textures_SettledIn.GenericBuilding);

            /* Barracks */
            InvalidRoomTypesForRoomTypeScore.Add(RoomRoleDefOf.Barracks.defName);
            OverrideLabels.Add(RoomRoleDefOf.Barracks.defName, "Barracks");
            OverrideDescriptions.Add(RoomRoleDefOf.Barracks.defName, "A place to sleep for the masses.\nHaving any of these rooms yields no benefits.");
            //todo: draw new image OverrideTextures.Add(RoomRoleDefOf.Barracks.defName, Textures_SettledIn.GenericBuilding);

            /* Dining Room */
            OverrideLabels.Add(RoomRoleDefOf.DiningRoom.defName, "Dining Room");
            OverrideDescriptions.Add(RoomRoleDefOf.DiningRoom.defName, "A place to eat and enjoy your meals. Contains tables and chairs to sit on while enjoying the meals.");
            //OverrideTextures.Add(RoomRoleDefOf.DiningRoom.defName, Textures_SettledIn.GenericBuilding);

            /* RecRoom */
            OverrideLabels.Add(RoomRoleDefOf.RecRoom.defName, "Recreation Room");
            OverrideDescriptions.Add(RoomRoleDefOf.RecRoom.defName, "A place to spend your measly short free time and enjoy tv, games or simply contemplating in silence or with some good music from the boom box. Contains recreation buildings of all kind.");
            //OverrideTextures.Add(RoomRoleDefOf.RecRoom.defName, Textures_SettledIn.GenericBuilding);

            // todo: add all the rest
            

        }

        public static List<string> SkippedRoomTypes = new List<string>(new string[] { "None", /*"Bedroom", "Room", "Tomb", "Barn"*/ });
        public static Dictionary<string, string> OverrideLabels = new Dictionary<string, string>();
        public static Dictionary<string, string> OverrideDescriptions = new Dictionary<string, string>();
        public static Dictionary<string, Texture2D> OverrideTextures = new Dictionary<string, Texture2D>();
        public static List<string> InvalidRoomTypesForRoomTypeScore = new List<string>();

        public static float GenerateRoomScore(Room room)
        {
            var impressiveness = room.GetStat(RoomStatDefOf.Impressiveness);
            var wealth = room.GetStat(RoomStatDefOf.Wealth);
            //var space = room.GetStat(RoomStatDefOf.Space);
            

            var score = wealth;
            if (impressiveness < 30)
            {
                score *= 0.75f;
            }
            if (impressiveness > 75)
            {
                score *= 1.25f;
            }
            score += GetFlatBonusForThisRoomByRoomRole(room);
            return score;
        }

        private static float GetFlatBonusForThisRoomByRoomRole(Room room)
        {
            // barracks have no worth
            if (room.Role == RoomRoleDefOf.Barracks)
            {
                return 0f;
            }
            // same with prison barracks
            else if (room.Role == RoomRoleDefOf.PrisonBarracks)
            {
                return 0f;
            }
            // bedrooms have no bonus
            else if (room.Role == RoomRoleDefOf.Bedroom)
            {
                return 0f;
            }
            // same with prison cells
            else if (room.Role == RoomRoleDefOf.PrisonCell)
            {
                return 0f;
            }
            // empty rooms without purpose have no specific bonus
            else if (room.Role == RoomRoleDefOf.None)
            {
                return 0f;
            }
            /*else if (room.Role == RoomRoleDefOf.DiningRoom)
            {

            }*/

            var roomRoleScore = room.Role.Worker.GetScore(room);
            return roomRoleScore;
        }

        public static float GenerateRoomTypeScoreFromFulfillment(int achievedRooms, int totalRooms)
        {
            return achievedRooms * 2000f;
        }

        public static float GetTraderQuantityMultiplierFromSettlementScore(float settlementScore)
        {
            // 1% more stock per 1000 settlement points
            return 1.0f + (settlementScore / 1000) / 100;
        }
    }
}
