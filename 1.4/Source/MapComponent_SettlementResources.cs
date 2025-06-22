using RimWorld.QuestGen;
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
    /// all statistics for a single map
    /// </summary>
    public class MapStatistics
    {
        public float totalPointsByScore;
        public int settlementLevel;
        public float totalPointsByRoomTypes;
        public int validRoomTypes = 0;
        public int achievedRoomTypes = 0;
        public List<RoomRoleStatictics> roomRoles = new List<RoomRoleStatictics>();

        public override string ToString()
        {
            return "settlementLevel= " + settlementLevel + ";points=" + totalPointsByScore + ", roomRoles=[" + String.Join(";", roomRoles) + "]";
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

    // only stores the settlement wide settings - business logic is with the accessing classes or the game component
    public  class MapComponent_SettlementResources : MapComponent
    {
        public int ManagementBuffer_current;
        public int ManagementBuffer_max;
        public int SettlementLevel;

        public MapStatistics cachedStatistics = null;

        public MapComponent_SettlementResources(Map map) : base(map)
        {
            Log.Debug("GameComponent_SettlementScoreManager created");
            SettlementLevel = 0;
            ManagementBuffer_current = 0;
            ManagementBuffer_max = 120;
        }

        public int constructionsToday = 0;
        public int constructionsYesterday = 0;
        public float constructionWorkToday = 0f;
        public float constructionWorkYesterday = 0f;
        // called to notify this manager that something has been contructed
        public void Notify_Construction(float constructedWork)
        {
            this.constructionsToday++;
            this.constructionWorkToday += constructedWork;
            refreshConstructionMoods();
        }

        private void refreshConstructionMoods()
        {
            var constructions = constructionsToday + constructionsYesterday;
            var buffIndex = 0;
            if (constructions > 0)
            {
                buffIndex = 1;
            }
            var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; });
            foreach (var pawn in validPawns)
            {
#if DEBUG
                var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.BeenConstructingLately);
                if (prevMemory == null || prevMemory.CurStageIndex != buffIndex)
                {
                    Log.Debug(pawn.Name + ": Assigning new buff due to construction activity! constructions=" + constructions + ", buffIndex=" + buffIndex);
                }
#endif
                pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.BeenConstructingLately, buffIndex));
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            var currTicks = Find.TickManager.TicksGame;

            // once a day update all the caches
            if (currTicks % 62500 == 0) // 2500 -> one in-game hour -> every day being 2500*24 = 62.500 /*GenTicks.TickRareInterval*/
            {
                UpdateCache();
            }
            // rarely (all 2500 ticks) update the max_buffer size and substract points for the settlement management
            if (currTicks % GenTicks.TickRareInterval == 0)
            {
                var countPeople = this.map.mapPawns.ColonistCount;
                ManagementBuffer_max = 120000 * countPeople;

                if (SettlementLevel > 4)
                {
                    ManagementBuffer_current -= countPeople * GenTicks.TickRareInterval;
                    if (ManagementBuffer_current < 0)
                    {
                        ManagementBuffer_current = 0;
                    }
                }
            }
            // rarely (all 2500 ticks) calculate the mood boost for the amount of assigned 
            if (currTicks % 2500 == 0)
            {
                if (SettlementLevel > 2) // starting with settlement level 3
                {
                    var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; });
                    foreach (var pawn in validPawns)
                    {
                        var foundPriorities = 0;
                        //Log.Debug("pawn");
                        List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                        for (int i = 0; i < allDefsListForReading.Count; i++)
                        {
                            WorkTypeDef workTypeDef = allDefsListForReading[i];
                            if (pawn.workSettings.GetPriority(workTypeDef) > 0)
                            {
                                foundPriorities++;
                            }
                        }
                        const int allowedWork = 16;
                        var delta = allowedWork - foundPriorities;
                        if (delta > 0)
                        {
                            var buffId = delta;
                            if (buffId >= DefOfs_SettledIn.FewAssignedPriorities.stages.Count)
                            {
                                buffId = DefOfs_SettledIn.FewAssignedPriorities.stages.Count - 1;
                            }
#if DEBUG
                            var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.FewAssignedPriorities);
                            if (prevMemory == null || prevMemory.CurStageIndex != buffId)
                            {
                                Log.Debug(pawn.Name + ": Assigning new buff due to few priorities! assignedPriorities=" + foundPriorities + ", buffId=" + buffId);
                            }
#endif

                            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefOfs_SettledIn.ManyAssignedPriorities);
                            pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.FewAssignedPriorities, buffId));
                        }
                        else if (delta < 0)
                        {
                            var debuffId = Math.Abs(delta);
                            if (debuffId >= DefOfs_SettledIn.ManyAssignedPriorities.stages.Count)
                            {
                                debuffId = DefOfs_SettledIn.ManyAssignedPriorities.stages.Count - 1;
                            }
#if DEBUG
                            var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.ManyAssignedPriorities);
                            if (prevMemory == null || prevMemory.CurStageIndex != debuffId)
                            {
                                Log.Debug(pawn.Name + ": Assigning new debuff due to too many priorities! assignedPriorities=" + foundPriorities + ", debuffId=" + debuffId);
                            }
#endif
                            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefOfs_SettledIn.FewAssignedPriorities);
                            pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.ManyAssignedPriorities, debuffId));
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefOfs_SettledIn.ManyAssignedPriorities);
                            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefOfs_SettledIn.FewAssignedPriorities);
                        }
                    }
                }
            }

            // trigger immigration quests
            if (currTicks % 62500 == 0) // daily calculate a chance for immigrants
            {
                if (SettlementLevel > 0 && Rand.Chance(SettlementLevelUtility.CalculateChanceForImmigrants(SettlementLevel))) // only active starting with settlement level 1
                {
                    Log.Debug("new immigrant arrives!");
                    Slate slate = new Slate();
                    slate.Set<Map>("map", this.map, false);
                    slate.Set<PawnGenerationRequest>("overridePawnGenParams", new PawnGenerationRequest(PawnKindDefOf.Villager, null, PawnGenerationContext.NonPlayer, -1, true, false, false, true, false, 20f, false, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, false), false);
                    QuestUtility.GenerateQuestAndMakeAvailable(DefOfs_SettledIn.ImmigrantArrives, slate);
                }
            }

            // recalculate mood for recreation schedules
            if ((currTicks + 157) % 62500 == 0) // daily calculate the happiness due to pawn schedules, but unalign it with the default modulo for spreading the calculation time each frame
            {
                if (SettlementLevel > 0) // only active starting with settlement level 1
                {
                    Log.DebugOnce("hook for calculating mood based on schedule found");
                    var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; });
                    foreach (var pawn in validPawns)
                    {
                        var recreationHours = 0;
                        foreach (var scheduled in pawn.timetable.times)
                        {
                            if (scheduled == TimeAssignmentDefOf.Joy)
                            {
                                recreationHours++;
                            }
                        }
                        var buffIndex = Math.Min(DefOfs_SettledIn.RecreationSchedule.stages.Count-1, recreationHours);
#if DEBUG
                        var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.RecreationSchedule);
                        if (prevMemory == null || prevMemory.CurStageIndex != buffIndex)
                        {
                            Log.Debug(pawn.Name + ": Assigning new buff for recreation schedules! recreationHours=" + recreationHours + ", buffIndex=" + buffIndex);
                        }
#endif
                        pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.RecreationSchedule, buffIndex));
                    }
                }
            }

            // recalculate mood for recreation schedules
            if ((currTicks + 111) % 62500 == 0) // daily calculate the happiness due to construction work, but unalign it with the default modulo for spreading the calculation time each frame
            {
                Log.DebugOnce("hook for calculating mood boost from construction work found");
                constructionWorkYesterday = constructionWorkToday;
                constructionWorkToday = 0;
                constructionsYesterday = constructionsToday;
                constructionsToday = 0;
                refreshConstructionMoods(); // refresh a last time fbefore reducing buffers
            }


        }

        /**
         * Upgrades the settlement if possible. Returns whether the uprade succeeded.
         * */
        public bool UpgradeSettlement()
        {
            // checks are not required, as the upgrade button is only available if the upgrade is allowed.
            if (SettlementLevel >= SettlementLevelUtility.MaxLevel)
            {
                Log.Debug("settlement level on map " + map + " is already at max");
                return false;
            }
            /*
            if (SettlementLevel == 0)
            {
                // check the lvl1 requirements: none, as we already built a settlement center as that is the only way to trigger this method
            }
            if (SettlementLevel == 1)
            {
                // check lvl2 requirements: 
            }*/


            SettlementLevel += 1;
            Log.Debug("settlement level on map " + map + " is now " + SettlementLevel);
            return true;
        }

        public void DowngradeSettlement()
        {
            if (SettlementLevel <= 0)
            {
                Log.Debug("settlement level on map " + map + " is already at lowest");
                return;
            }
            if (SettlementLevel > 0)
            {
                SettlementLevel -= 1;
            }
            Log.Debug("settlement level on map " + map + " is now " + SettlementLevel);
        }


        public void UpdateCache()
        {
            Log.Debug("MapComponent_SettlementResources.UpdateCache(): Regenerating cache for " + map);

            cachedStatistics = new MapStatistics();

            var allRooms = map?.regionGrid?.allRooms;
            if (allRooms == null) // just use an empty list if we get nothing else...
            {
                allRooms = new List<Room>();
            }
            allRooms = allRooms.Where(room => { return room.Role != null && !SettlementScoreUtility.SkippedRoomTypes.Contains(room.Role.defName) && !room.Fogged && room.ProperRoom; }).ToList();

            var allRoomRoles = DefDatabase<RoomRoleDef>.AllDefs.ToList();

            //cachedStatistics.roomRoles.Add(initStatistic(DefOfs_SettledIn.Museum.defName));
            // fill missing fields and add generic items
            foreach (RoomRoleDef def in allRoomRoles)
            {
                if (SettlementScoreUtility.SkippedRoomTypes.Contains(def.defName))
                {
                    continue;
                }
                var existingEntry = cachedStatistics.roomRoles.FirstOrDefault(entry => { return entry.defName == def.defName; });
                if (existingEntry == null)
                {
                    existingEntry = initStatisticsForRoomDef(def.defName);
                    cachedStatistics.roomRoles.Add(existingEntry);
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
            cachedStatistics.totalPointsByScore = cachedStatistics.roomRoles.Sum(roomRole => { return roomRole.GenerateTotalPoints(); });
            cachedStatistics.validRoomTypes = cachedStatistics.roomRoles.Count(roomRole => { return roomRole.validForRoomRolePoints; });
            cachedStatistics.achievedRoomTypes = cachedStatistics.roomRoles.Count(roomRole => { return roomRole.validForRoomRolePoints && roomRole.rooms.Count > 0; });
            cachedStatistics.totalPointsByRoomTypes = SettlementScoreUtility.GenerateRoomTypeScoreFromFulfillment(cachedStatistics.achievedRoomTypes, cachedStatistics.validRoomTypes);

            //float totalPoints = totalPointsByRoomTypes + totalPointsByScore;
            cachedStatistics.settlementLevel = SettlementLevel;

            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(map): Cache for map=" + map + ", cache=" + cachedStatistics);
        }

        // initialize a new statistical data filed. Mostly take exceptions from SettlementScoreUtility to initialize the data
        private RoomRoleStatictics initStatisticsForRoomDef(string defName)
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


        // saving and loading the map data
        public override void ExposeData()
        {
            base.ExposeData();
            try
            {
                Scribe_Values.Look(ref SettlementLevel, "SettlementLevel", 0);
                Scribe_Values.Look(ref ManagementBuffer_current, "ManagementBuffer_current", 0);
                Scribe_Values.Look(ref ManagementBuffer_max, "ManagementBuffer_max", 120);
                Scribe_Values.Look(ref constructionsToday, "constructionsToday", 0);
                Scribe_Values.Look(ref constructionsYesterday, "constructionsYesterday", 0);
                Scribe_Values.Look(ref constructionWorkToday, "constructionWorkToday", 0);
                Scribe_Values.Look(ref constructionWorkYesterday, "constructionWorkYesterday", 0);
            } 
            catch (Exception ex)
            {
                Log.Warning("Failed to load settings of MapComponent_SettlementResources. This is an error the game will recover from within the next seconds. Details: " + ex);
            }
        }
    }
}
