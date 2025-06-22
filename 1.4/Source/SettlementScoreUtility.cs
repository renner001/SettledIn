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
        // key: level
        // value: score
        public static List<KeyValuePair<int, int>> DefinedScoresbyLevel = new List<KeyValuePair<int, int>>();

        // todo: delete as the level is now user controled
        public static int LevelFromScore(int score)
        {
            int returnLevel = 0;
            for (int i=0;i<DefinedScoresbyLevel.Count;i++)
            {
                if (DefinedScoresbyLevel[i].Value < score)
                {
                    returnLevel = DefinedScoresbyLevel[i].Key;
                } 
                else
                {
                    break;
                }
            }
            return returnLevel;
        }

        static SettlementScoreUtility()
        {
            /* defined scores */
            DefinedScoresbyLevel.Add(new KeyValuePair<int, int>(0, 0));
            DefinedScoresbyLevel.Add(new KeyValuePair<int, int>(1, 2500));
            DefinedScoresbyLevel.Add(new KeyValuePair<int, int>(2, 5000));
            DefinedScoresbyLevel.Add(new KeyValuePair<int, int>(9, 10000000));

            /* None */
            SkippedRoomTypes.Add("None");
            /*
            OverrideLabels.Add(RoomRoleDefOf.None.defName, "General Purpose Room");
            OverrideDescriptions.Add(RoomRoleDefOf.None.defName, "Four walls and probably a roof. One day, this may mature into a real purpose room?");
            OverrideTextures.Add(RoomRoleDefOf.None.defName, Textures_SettledIn.GenericBuilding);*/

            /* Room */
            InvalidRoomTypesForRoomTypeScore.Add("Room"); // general purpose rooms bring no bonus for having this type of room
            OverrideLabels.Add("Room", "General Purpose Room");
            OverrideDescriptions.Add("Room", "Four walls and probably a roof. One day, this may mature into a purposeful room?\nUntil then they will give no bonuses whatsoever beyond the wealth they are built out of. They do not increase the room count.");
            OverrideTextures.Add("Room", Textures_SettledIn.GenericBuilding);

            /* Bedroom */
            OverrideLabels.Add(RoomRoleDefOf.Bedroom.defName, "Bedroom");
            OverrideDescriptions.Add(RoomRoleDefOf.Bedroom.defName, "A private place to sleep for one pawn with or without spouse.\nContains a sleeping spot or bed. Having at least one of these yields a small bonus to the settlement score.");
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

            /* Guest Room */
            OverrideDescriptions.Add("GuestRoom", "A place to stay for your guests.\nContains sleeping opportunities for guests. Each room yields a small bonus to the score.");


            /* Hospital */
            OverrideDescriptions.Add(RoomRoleDefOf.Hospital.defName, "Health is a good that should never be underrated on the Rim. In the hospitals, your colonists can be assured to be well taken care off.\nContains sleeping opportunities for the sick and wounded.");

            /* Kitchen */
            OverrideDescriptions.Add("Kitchen", "The place to cook food in style.\nContains stoves and kitchen cupboards.");


            /* Laboratory */
            OverrideDescriptions.Add(RoomRoleDefOf.Laboratory.defName, "The laboratory is where the brains are put to work!\nContains research buildings.");

            /* Prison Barracks */
            OverrideDescriptions.Add(RoomRoleDefOf.PrisonBarracks.defName, "A place to sleep for the prisoners.");

            /* Prison Cell */
            OverrideDescriptions.Add(RoomRoleDefOf.PrisonCell.defName, "Some prisoners are lucky to get their own, private place. Aren't we nice?");

            /* RecRoom */
            OverrideLabels.Add(RoomRoleDefOf.RecRoom.defName, "Recreation Room");
            OverrideDescriptions.Add(RoomRoleDefOf.RecRoom.defName, "A place to spend your measly short free time and enjoy tv, games or simply contemplating in silence or with some good music from the boom box. Contains recreation buildings of all kind.");
            //OverrideTextures.Add(RoomRoleDefOf.RecRoom.defName, Textures_SettledIn.GenericBuilding);

            /* Storeroom */
            // tbd: where is the storeroom coming from? Mod or ?
            OverrideDescriptions.Add("Storeroom", "Making it on the Rim means acquiring stuff. And that stuff has to go somewhere, doesn't it?\nContains storage solutions like shelfs and very little other.");


            // todo: add all the rest


        }

        public static List<string> SkippedRoomTypes = new List<string>(new string[] { "None", "BDsDisplayMuseumRoom" /* Disdplay Shelf mod; will be covered by our Museum room type instead */, /*"Bedroom", "Room", "Tomb", "Barn"*/ });
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
            score *= 0.01f;
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
            // guest rooms from Hospitality mod have a flat 100 points bonus
            else if (room.Role.defName == "GuestRoom")
            {
                return 100f;
            }
            
            else if(room.Role == DefOfs_SettledIn.SettlementOffice)
            {
                return 500f;
            }
            /*else if (room.Role == RoomRoleDefOf.DiningRoom)
            {

            }*/

            return 250;

            //var roomRoleScore = room.Role.Worker.GetScore(room);
            //return roomRoleScore;
        }

        public static float GenerateRoomTypeScoreFromFulfillment(int achievedRooms, int totalRooms)
        {
            return achievedRooms * 200f;
        }

        public static float GetTraderQuantityMultiplierFromSettlementScore(float settlementScore)
        {
            // 1% more stock per 10 settlement points
            return 1.0f + (settlementScore / 20) / 100;
        }

    }
}
