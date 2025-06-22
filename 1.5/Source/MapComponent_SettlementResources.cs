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

        public bool GlobalEffects_IsPrisonFarAway = false;
        public int GlobalEffects_IsPrisonFarAway_lastTriggered = 0;
        public const int GlobalEffects_IsPrisonFarAway_Cooldown = 2500 * 24 * 5;
        public bool GlobalEffectWalkSpeedToggled = false;
        public bool GlobalEffectWalkSpeedActive = false;

        public List<Pawn> HasUnlikedNeighbours = new List<Pawn>();
        public List<Pawn> LongCommuters = new List<Pawn>();
        public List<Pawn> WantsWorkbench = new List<Pawn>();
        public List<Pawn> WantsFewerTasks = new List<Pawn>();
        public List<Pawn> WantsMoreRecreation = new List<Pawn>();

        public MapComponent_SettlementResources(Map map) : base(map)
        {
            Log.Debug("GameComponent_SettlementScoreManager created");
            SettlementLevel = 0;
            ManagementBuffer_current = 0;
            ManagementBuffer_max = 120;
        }

        #region construction effects

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
            var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; }).ToArray();
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

        #endregion

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            var currTicks = Find.TickManager.TicksGame;

            // once a day update all the caches
            if (currTicks % 62500 == 0) // 2500 -> one in-game hour -> every day being 2500*24 = 62.500 /*GenTicks.TickRareInterval*/
            {
                UpdateStatisticsCache();
            }
            // rarely (all 2500 ticks) update the max_buffer size and substract points for the walk speed buff
            if (currTicks % GenTicks.TickRareInterval == 0)
            {
                Log.DebugOnce("MapComponent_SettlementResources.MapComponentTick(): Hook for updating settlement buffer is getting called..",8758998);
                var countPeople = this.map.mapPawns.ColonistCount;
                ManagementBuffer_max = 120000 * countPeople;
                bool walkSpeedWasDeducted = false;
                if (GlobalEffectWalkSpeedToggled &&  SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl3_WalkSpeed))
                {
                    var managementBufferRequired = countPeople * GenTicks.TickRareInterval;
                    if (ManagementBuffer_current >= managementBufferRequired)
                    {
                        ManagementBuffer_current -= managementBufferRequired;
                        walkSpeedWasDeducted = true;
                    }
                }
                if (GlobalEffectWalkSpeedActive != walkSpeedWasDeducted)
                {
                    Log.Debug($"MapComponent_SettlementResources.MapComponentTick(): walk speed bonus changed from {GlobalEffectWalkSpeedActive} to {walkSpeedWasDeducted}. Refreshing MoveSpeed cache..");
                    GlobalEffectWalkSpeedActive = walkSpeedWasDeducted;
                    StatDefOf.MoveSpeed.Worker.TryClearCache();
                }
            }
            // rarely (all 2500 ticks) calculate the mood boost for the amount of assigned duties
            if (currTicks % 2500 == 0)
            {
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl2_ColonistFocus))
                {
                    Log.DebugOnce("MapComponent_SettlementResources.MapComponentTick(): Hook for updating assigned tasks based buffs is getting called..", 218468);
                    WantsFewerTasks.Clear();
                    var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; }).ToArray();
                    List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                    var totalWorkTypes = allDefsListForReading.Count;
                    var allowedWork = (int)Math.Floor(totalWorkTypes / 2f) - 2;
                    foreach (var pawn in validPawns)
                    {
                        var foundPriorities = 0;
                        //Log.Debug("pawn");
                        for (int i = 0; i < allDefsListForReading.Count; i++)
                        {
                            WorkTypeDef workTypeDef = allDefsListForReading[i];
                            if (pawn.workSettings.GetPriority(workTypeDef) > 0)
                            {
                                foundPriorities++;
                            }
                        }
                        
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
                            WantsFewerTasks.Add(pawn);
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
                var chanceForImmigrants = SettlementLevelUtility.CalculateChanceForImmigrants(SettlementLevel);
                Log.Debug("MapComponent_SettlementResources.MapComponentTick(): Calculating chance of immigrants: " + chanceForImmigrants);
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl0_Immigrants) && Rand.Chance(chanceForImmigrants)) // only active starting with settlement level 1
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
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl5_RecreationFocus)) // only active starting with settlement level 1
                {
                    Log.Debug("MapComponent_SettlementResources.MapComponentTick(): Updating recreational moods");
                    WantsMoreRecreation.Clear();
                    var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; }).ToArray();
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
                        if (recreationHours < 5) {
                            WantsMoreRecreation.Add(pawn);
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

            const float FilthPercentToCleanHourly = 0.05f;
            // remove some filth
            if ((currTicks + 73) % 2500 == 0) // hourly
            {
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl4_TidySettlements))
                {
                    var filthList = map.listerThings.GetThingsOfType<Filth>().ToArray();
                    if (filthList != null && filthList.Length > 0)
                    {
                        var numFilthToClean = Mathf.Clamp(filthList.Length * FilthPercentToCleanHourly, 2, 100);
                        Log.Debug($"thinning {numFilthToClean} of total {filthList.Length} filth");
                        for (int i = 0; i < numFilthToClean && i < filthList.Length; i++)
                        {
                            //Log.Debug($"thinning filth {filthList[i]}");
                            filthList[i].ThinFilth();
                        }
                    }
                }
            }

            const int RepairRatePerHour = 1;
            // repair worn apparels
            if ((currTicks + 46) % 2500 == 0) // hourly
            {
                Log.DebugOnce("at least hook for repairing apparel is getting called..");
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl5_PristineClothing))
                {
                    var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; }).ToArray();
                    foreach (var pawn in validPawns)
                    {
                        var wornApparel = pawn.apparel?.WornApparel;
                        if (wornApparel != null)
                        {
                            foreach (var item in wornApparel)
                            {
                                if (item.def.useHitPoints && item.HitPoints < item.MaxHitPoints)
                                {
                                    var oldHitpoints = item.HitPoints;
                                    var targetHitpoints = Mathf.Clamp(item.HitPoints + RepairRatePerHour, item.HitPoints, item.MaxHitPoints);
                                    Log.Debug($"repairing apparel: setting  hitpoints from {oldHitpoints} to {targetHitpoints} of {item} worn by {pawn}");
                                    item.HitPoints = targetHitpoints;
                                }
                            }
                        }
                    }
                }
            }

            // recalculate mood for construction activities
            if ((currTicks + 111) % 62500 == 0) // daily calculate the happiness due to construction work, but unalign it with the default modulo for spreading the calculation time each frame
            {
                Log.Debug("MapComponent_SettlementResources.MapComponentTick(): Updating construction moods");
                constructionWorkYesterday = constructionWorkToday;
                constructionWorkToday = 0;
                constructionsYesterday = constructionsToday;
                constructionsToday = 0;
                refreshConstructionMoods(); // refresh a last time before reducing buffers
            }

            // recalculate settlement trader stock
            if (currTicks % 937500 == 0) // once a quadrum (15 days) //todo: create settings for this
            {
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl2_TraderStock))
                {
                    Log.Debug("MapComponent_SettlementResources.MapComponentTick(): Restocking settlement trader");
                    SettlementLevelUtility.SettlementTrader_RefreshStock(map);
                }
                
            }

            var nextGlobalEffects_IsPrisonFarAway = true;
            // recalculate effects for settlement layouts
            if ((currTicks + 767) % 62500 == 0) // daily calculate the happiness due to construction work, but unalign it with the default modulo for spreading the calculation time each frame
            {
                if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl1_ManagedSettlements))
                {
                    Log.Debug("MapComponent_SettlementResources.MapComponentTick(): updating settlement layout effects..");
                    HasUnlikedNeighbours.Clear();
                    WantsWorkbench.Clear();
                    LongCommuters.Clear();
                    // build the list of assignable buildings
                    var validPawns = map.mapPawns.AllPawns.Where(pawn => { return pawn.IsColonist; }).ToArray();
                    var allBuildingsAssignedToPawns = map.listerBuildings.allBuildingsColonist.Where(building => { return building.HasComp<CompAssignableToPawn>(); }).ToArray();
                    var allBeds = allBuildingsAssignedToPawns.Where(building => { return building as Building_Bed != null; }).ToArray();
                    var prisonBeds = allBeds.Where(bed => { return (bed as Building_Bed).ForPrisoners; }).ToArray();
                    var allRooms = map?.regionGrid?.allRooms.ToArray();

                    // pawn related effects
                    foreach (var pawn in validPawns)
                    {
                        // distance to bedrooms of unliked pawns
                        var knownPawnsWithRelation = SocialCardUtility.PawnsForSocialInfo(pawn);
                        var unlikedPawns = knownPawnsWithRelation.Where(otherPawn => { return pawn.relations.OpinionOf(otherPawn) <= -20; }).ToArray();

                        // unliked neightbour: debuff for unliked pawn beds nearby
                        const float unlikedPawnBedDistance = 20f;
                        const float prisonBedDistance = 30f;
                        var myBed = allBeds.FirstOrDefault(bed =>
                        {
                            var assignedPawns = bed.GetAssignedPawns();
                            return assignedPawns != null && assignedPawns.Contains(pawn);
                        });
                        var unlikedBeds = allBeds.Where(bed =>
                        {
                            var assignedPawns = bed.GetAssignedPawns();
                            if (assignedPawns == null)
                                return false;
                            return assignedPawns.Any(bedAssignedPawn =>
                            {
                                return unlikedPawns.Contains(bedAssignedPawn);
                            });
                        });
                        if (prisonBeds.Any(prisonBed => { return myBed.Position.DistanceTo(prisonBed.Position) < prisonBedDistance; }))
                        {
#if DEBUG
                            var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.UnlikedPrisonNeightbour);
                            if (prevMemory == null)
                            {
                                Log.Debug(pawn.Name + ": Assigning new debuff for UnlikedPrisonNeightbour");
                            }
#endif
                            nextGlobalEffects_IsPrisonFarAway = false;
                            if (!HasUnlikedNeighbours.Contains(pawn))
                            {
                                HasUnlikedNeighbours.Add(pawn);
                            }
                            pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.UnlikedPrisonNeightbour));
                        }
                        if (unlikedBeds.Any(unlikedBed => { return myBed.Position.DistanceTo(unlikedBed.Position) < unlikedPawnBedDistance; }))
                        {
#if DEBUG
                            var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.UnlikedNeightbour);
                            if (prevMemory == null)
                            {
                                Log.Debug(pawn.Name + ": Assigning new debuff for UnlikedNeightbour");
                            }
#endif
                            if (!HasUnlikedNeighbours.Contains(pawn))
                            {
                                HasUnlikedNeighbours.Add(pawn);
                            }
                            pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.UnlikedNeightbour));
                        }

                        // building related stuff on pawns
                        const float stopCalculatingCommuteAtDistance = 40f;
                        if (allBuildingsAssignedToPawns != null && pawn.needs.mood != null)
                        {
                            var assignedToThisPawn = allBuildingsAssignedToPawns.Where(building =>
                            {
                                var assignablePawn = building.GetComp<CompAssignableToPawn>();
                                if (assignablePawn != null)
                                    return assignablePawn.AssignedPawns.Contains(pawn);
                                return false;
                            }).ToArray();
                            /*
                            var assignedToThisPawnNotBeds = assignedToThisPawn.Where(building => { return !(building is Building_Bed); });
                            // check the size of all work rooms is sufficient
                            assignedToThisPawn[0]
                            var roomOfWorkTable[0].GetRoom().CellCount;
                            */

                            // no personal work table: missing assignment of pawns to work tables
                            var nonBedsAssignedToThisPawn = assignedToThisPawn.Where(building => { return !(building is Building_Bed); });
                            var validSkillTypes = new[] { SkillDefOf.Cooking, SkillDefOf.Medicine, SkillDefOf.Intellectual, SkillDefOf.Artistic, SkillDefOf.Crafting };
                            if (SettlementLevelUtility.IsBenefitActiveAt(SettlementLevel, SettlementLevelUtility.Benefit_lvl1_PersonalWorkbench))
                            {
                                var personalWorktableDemanded = false;
                                foreach (var skillType in validSkillTypes)
                                {
                                    var currSkillValue = pawn.skills?.GetSkill(skillType);
                                    var currLevel = currSkillValue != null ? currSkillValue.Level : 0;
                                    if (currLevel >= 8) // todo: fine tune the skill level
                                    {
                                        personalWorktableDemanded = true;
                                    }
                                }
                                if (personalWorktableDemanded)
                                {
                                    var bench = nonBedsAssignedToThisPawn.FirstOrDefault();
                                    Log.Debug($"pawn {pawn} demands personal work bench and has bench: {bench}");
                                    if (bench == null)
                                    {
#if DEBUG
                                        var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.OwnsNoPersonalWorkbench);
                                        if (prevMemory == null)
                                        {
                                            Log.Debug(pawn.Name + ": Assigning new debuff for OwnsNoPersonalWorkbench");
                                        }
#endif
                                        if (!WantsWorkbench.Contains(pawn))
                                        {
                                            WantsWorkbench.Add(pawn);
                                        }
                                        pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.OwnsNoPersonalWorkbench));
                                    }
                                }
                            }

                            // commute debuff: check distances for commute times debuff for every pawn
                            float totalDistances = calculateTotalDistanceBetween(assignedToThisPawn, stopCalculatingCommuteAtDistance);
                            Log.Debug($"Commute distance for pawn ${pawn} is ${totalDistances}");
                            if (totalDistances >= stopCalculatingCommuteAtDistance)
                            {
#if DEBUG
                                var prevMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefOfs_SettledIn.LongCommutes);
                                if (prevMemory == null)
                                {
                                    Log.Debug(pawn.Name + ": Assigning new debuff for commute times");
                                }
#endif
                                if (!LongCommuters.Contains(pawn))
                                {
                                    LongCommuters.Add(pawn);
                                }
                                pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_SettledIn.LongCommutes));
                            }
                        }
                    }
                    GlobalEffects_IsPrisonFarAway = nextGlobalEffects_IsPrisonFarAway;
                }
            }

        }

        private static float calculateTotalDistanceBetween(Building[] assignedToThisPawn, float stopCalculatingAtDistance)
        {
            var buildingPositions = assignedToThisPawn.Select(building => building.Position).ToArray();
            float totalDistances = 0f;
            if (buildingPositions.Length > 1)
            {
                for (int posOne = 0; posOne <= buildingPositions.Length - 2; posOne++)
                {
                    for (int posTwo = posOne + 1; posTwo <= buildingPositions.Length - 1; posTwo++)
                    {
                        // simple distances by sum of x and y
                        var buildingOne = buildingPositions[posOne];
                        var buildingTwo = buildingPositions[posTwo];
                        totalDistances += buildingOne.DistanceTo(buildingTwo);
                        if (totalDistances > stopCalculatingAtDistance)
                        {
                            break;
                        }
                    }
                    if (totalDistances > stopCalculatingAtDistance)
                    {
                        break;
                    }
                }
            }

            return totalDistances;
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


        public void UpdateStatisticsCache()
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
                Scribe_Values.Look(ref GlobalEffectWalkSpeedActive, "GlobalEffectWalkSpeedActive", false);
                Scribe_Values.Look(ref constructionsToday, "constructionsToday", 0);
                Scribe_Values.Look(ref constructionsYesterday, "constructionsYesterday", 0);
                Scribe_Values.Look(ref constructionWorkToday, "constructionWorkToday", 0);
                Scribe_Values.Look(ref constructionWorkYesterday, "constructionWorkYesterday", 0); 

                // Save or load the list of pawn references
                Scribe_Collections.Look(ref HasUnlikedNeighbours, "HasUnlikedNeighbours", LookMode.Reference);
                // Handle null lists after loading
                if (Scribe.mode == LoadSaveMode.PostLoadInit && HasUnlikedNeighbours == null)
                {
                    HasUnlikedNeighbours = new List<Pawn>();
                }

                // Save or load the list of pawn references
                Scribe_Collections.Look(ref LongCommuters, "LongCommuters", LookMode.Reference);
                // Handle null lists after loading
                if (Scribe.mode == LoadSaveMode.PostLoadInit && LongCommuters == null)
                {
                    LongCommuters = new List<Pawn>();
                }

                // Save or load the list of pawn references
                Scribe_Collections.Look(ref WantsWorkbench, "WantsWorkbench", LookMode.Reference);
                // Handle null lists after loading
                if (Scribe.mode == LoadSaveMode.PostLoadInit && WantsWorkbench == null)
                {
                    WantsWorkbench = new List<Pawn>();
                }

                // Save or load the list of pawn references
                Scribe_Collections.Look(ref WantsFewerTasks, "WantsFewerTasks", LookMode.Reference);
                // Handle null lists after loading
                if (Scribe.mode == LoadSaveMode.PostLoadInit && WantsFewerTasks == null)
                {
                    WantsFewerTasks = new List<Pawn>();
                }

                // Save or load the list of pawn references
                Scribe_Collections.Look(ref WantsMoreRecreation, "WantsMoreRecreation", LookMode.Reference);
                // Handle null lists after loading
                if (Scribe.mode == LoadSaveMode.PostLoadInit && WantsMoreRecreation == null)
                {
                    WantsMoreRecreation = new List<Pawn>();
                }

                Scribe_Values.Look(ref GlobalEffects_IsPrisonFarAway, "GlobalEffects_IsPrisonFarAway", false);
                Scribe_Values.Look(ref GlobalEffects_IsPrisonFarAway_lastTriggered, "GlobalEffects_IsPrisonFarAway_lastTriggered", 0);
                Scribe_Values.Look(ref GlobalEffectWalkSpeedToggled, "GlobalEffectWalkSpeedToggled", false);

            } 
            catch (Exception ex)
            {
                Log.Warning("Failed to load settings of MapComponent_SettlementResources. This is an error the game will recover from within the next seconds. Details: " + ex);
            }
        }
    }
}
