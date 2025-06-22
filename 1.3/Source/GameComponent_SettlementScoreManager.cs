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
    /// all statistics for a single map
    /// </summary>
    public class MapStatistics
    {
        public float totalPointsByScore { get; private set; }
        public float totalPointsByRoomTypes { get; private set; }
        public int validRoomTypes = 0;
        public int achievedRoomTypes = 0;
        public List<RoomRoleStatictics> roomRoles = new List<RoomRoleStatictics>();
        public void Update()
        {
            totalPointsByScore = roomRoles.Sum(roomRole => { return roomRole.GenerateTotalPoints(); });
            validRoomTypes = roomRoles.Count(roomRole => { return roomRole.validForRoomRolePoints; });
            achievedRoomTypes = roomRoles.Count(roomRole => { return roomRole.validForRoomRolePoints && roomRole.rooms.Count > 0; });
            totalPointsByRoomTypes = SettlementScoreUtility.GenerateRoomTypeScoreFromFulfillment(achievedRoomTypes, validRoomTypes);
        }

        public override string ToString()
        {
            return "points=" + totalPointsByScore + ", roomRoles=[" + String.Join(";", roomRoles) + "]";
        }
    }

    /// <summary>
    /// statistics for an entire room role, consisting of multiple rooms
    /// </summary>
    public class RoomRoleStatictics
    {
        public string defName;

        public string overrideLabel = null;
        public string overrideDescription = null;
        public bool validForRoomRolePoints = true;

        public Texture2D texture;
        public RoomRoleDef def;

        public List<RoomStatistics> rooms = new List<RoomStatistics>();

        public float GenerateTotalPoints()
        {
            return rooms.Sum(room => { return room.score; });
        }

        public override string ToString()
        {
            return "{ defName=" + defName + ", graphics=" + texture.name + ", points =" + GenerateTotalPoints() + ", " + rooms.Count + " rooms: [" + String.Join(";", rooms) + "] }";
        }
    }

    /// <summary>
    /// statistics for a single room
    /// </summary>
    public class RoomStatistics
    {
        public Room roomRef;
        public LookTargets location;
        public float score = 0;

        public override string ToString()
        {
            return RoomNameUtility.GetRoomRoleLabel(roomRef) + ": " + score + " points";
        }
    }

    class GameComponent_SettlementScoreManager : GameComponent
    {
        public GameComponent_SettlementScoreManager(Game game)
        {
            Log.Debug("GameComponent_SettlementScoreManager created");
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            UpdateCache();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            UpdateCache();
        }

        public override void GameComponentTick()
        {
            //base.GameComponentTick();
            // once a day update all the caches
            if (Find.TickManager.TicksGame % 62500 == 0) // 2500 -> one in-game hour -> every day being 2500*24 = 62.500 /*GenTicks.TickRareInterval*/
            {
                UpdateCache();
            }
        }

        public Dictionary<Map, MapStatistics> cachedStatistics = new Dictionary<Map, MapStatistics>();

        public void UpdateCache()
        {
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(): Regenerating complete cache..");
            cachedStatistics.Clear(); 
            var allHomeMaps = Find.Maps.Where(map => { return map.IsPlayerHome; }).ToArray();
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(): " + allHomeMaps.Length + " home maps found " + String.Join(", ", allHomeMaps.Select(homeMap => { return homeMap.ToString(); })));
            foreach (Map homeMap in allHomeMaps)
            {
                UpdateCache(homeMap);
            }
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(): Cche complete: " + String.Join(", ", cachedStatistics));
        }

        public void UpdateCache(Map map)
        {
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(map): Regenerating cache for " + map);
            if (!cachedStatistics.ContainsKey(map))
            {
                cachedStatistics.Add(map, new MapStatistics());
            } else
            {
                cachedStatistics[map].roomRoles.Clear();
            }
            var mapStatistic = cachedStatistics[map];

            var allRooms = map?.regionGrid?.allRooms;
            if (allRooms == null) // just use an empty list if we get nothing else...
            {
                allRooms = new List<Room>();
            }
            allRooms = allRooms.Where(room => { return room.Role != null && !SettlementScoreUtility.SkippedRoomTypes.Contains(room.Role.defName) && !room.Fogged && room.ProperRoom; }).ToList();

            var allRoomRoles = DefDatabase<RoomRoleDef>.AllDefs.ToList();

            //mapStatistic.roomRoles.Add(initStatistic(DefOfs_SettledIn.Museum.defName));
            // fill missing fields and add generic items
            foreach (RoomRoleDef def in allRoomRoles)
            {
                if (SettlementScoreUtility.SkippedRoomTypes.Contains(def.defName))
                {
                    continue;
                }
                var existingEntry = mapStatistic.roomRoles.FirstOrDefault(entry => { return entry.defName == def.defName; });
                if (existingEntry == null)
                {
                    existingEntry = initStatistic(def.defName);
                    mapStatistic.roomRoles.Add(existingEntry);
                }
                var roomsOfThisRole = allRooms.Where(room => { return room.Role == def && !room.Fogged && room.ProperRoom; });


                existingEntry.rooms.AddRange(roomsOfThisRole.Select(room => {
                    LookTargets location = null;
                    if (room.CellCount > 0)
                    {
                        location = new LookTargets(room.Cells.First(), Find.CurrentMap);
                    }
                    return new RoomStatistics()
                    {
                        roomRef = room,
                        location = location,
                        score = SettlementScoreUtility.GenerateRoomScore(room)
                    };
                }));
                existingEntry.def = def;
            }
            // update the points now after populating the whole list
            mapStatistic.Update();
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(map): Cache for map=" + map + ", cache=" + mapStatistic);
        }

        private RoomRoleStatictics initStatistic(string defName)
        {
            var texture = Textures_SettledIn.GenericBuilding;
            if (SettlementScoreUtility.OverrideTextures.ContainsKey(defName))
            {
                texture = SettlementScoreUtility.OverrideTextures[defName];
            }
            var newStatistic = new RoomRoleStatictics()
            {
                defName = defName,
                texture = texture
            };
            if (SettlementScoreUtility.InvalidRoomTypesForRoomTypeScore.Contains(defName) || SettlementScoreUtility.SkippedRoomTypes.Contains(defName))
            {
                newStatistic.validForRoomRolePoints = false;
            }
            if (SettlementScoreUtility.OverrideLabels.ContainsKey(defName))
            {
                newStatistic.overrideLabel = SettlementScoreUtility.OverrideLabels[defName];
            }
            if (SettlementScoreUtility.OverrideDescriptions.ContainsKey(defName))
            {
                newStatistic.overrideDescription = SettlementScoreUtility.OverrideDescriptions[defName];
            }
            return newStatistic;
        }
    }
}
