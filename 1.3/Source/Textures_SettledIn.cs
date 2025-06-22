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
        public static Texture2D ScoreIcon = ContentFinder<Texture2D>.Get("Score", false);
        public static Texture2D MenuIcon = ContentFinder<Texture2D>.Get("SettlementScoreMenuIcon", false);
    }
}
