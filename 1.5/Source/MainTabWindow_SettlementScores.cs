using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace DanielRenner.SettledIn
{
    class MainTabWindow_SettlementScores : MainTabWindow
    {
        // temporary variables
        Vector2 scrollPosition;
        //List<Room> allRooms;
        //List<RoomRoleDef> allRoomRoles;
        MapStatistics statistics;

        public MainTabWindow_SettlementScores()
        { }

        public override void PreOpen()
        {
            Log.Debug("MainTabWindow_SettlementScores.PreOpen() called");
            base.PreOpen();
            // get the room list; we do this now before updating the views cache to make sure we don't use rooms in this panel that were created after opening the panel
            Map currentMap = Find.CurrentMap;
            if (currentMap != null && currentMap.IsPlayerHome)
            {
                var settlementResources = currentMap.GetComponent<MapComponent_SettlementResources>();
                settlementResources.UpdateStatisticsCache();
                statistics = settlementResources.cachedStatistics;
            }
            else
            {
                statistics = new MapStatistics();
            }
            
            Log.Debug("MainTabWindow_SettlementScores.PreOpen() populated settings: " + String.Join(", ", statistics));
        }

        public override void PreClose()
        {
            Log.Debug("MainTabWindow_SettlementScores.PreClose() called");
            base.PreClose();
        }

        private Rect drawAndHandleLevelBenefitInSummary(string benefit, Rect benefitsArea, bool showDetails)
        {
            Dictionary<string, Texture2D> benefitIcons = new Dictionary<string, Texture2D>();
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl0_SettlementCenter, Textures_SettledIn.Benefit_lvl0_SettlementCenter);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl0_Immigrants, Textures_SettledIn.Benefit_lvl0_Immigrants);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl1_ManagedSettlements, Textures_SettledIn.Benefit_lvl1_ManagedSettlements);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl1_RoomFocus, Textures_SettledIn.Benefit_lvl1_RoomFocus);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl1_PersonalWorkbench, Textures_SettledIn.Benefit_lvl1_PersonalWorkbench);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl2_TraderStock, Textures_SettledIn.Benefit_lvl2_TraderStock);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl2_ColonistFocus, Textures_SettledIn.Benefit_lvl2_ColonistFocus);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl2_CommodityConsumption, Textures_SettledIn.Benefit_lvl2_CommodityConsumption);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl3_CozyRooms, Textures_SettledIn.Benefit_lvl3_CozyRooms);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl3_WalkSpeed, Textures_SettledIn.Benefit_lvl3_WalkSpeed);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl4_GreatHalls, Textures_SettledIn.Benefit_lvl4_GreatHalls);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl4_TidySettlements, Textures_SettledIn.Benefit_lvl4_TidySettlements);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl5_RecreationFocus, Textures_SettledIn.Benefit_lvl5_RecreationFocus);
            benefitIcons.Add(SettlementLevelUtility.Benefit_lvl5_PristineClothing, Textures_SettledIn.Benefit_lvl5_PristineClothing);

            string tipText;
            if (benefit == SettlementLevelUtility.Benefit_lvl0_Immigrants)
            {
                var chance = Math.Floor(SettlementLevelUtility.CalculateChanceForImmigrants(statistics.settlementLevel) * 100 * 100) / 100; // change chance to 2 digit percent value
                tipText = benefit.Translate(chance.Named("CHANCE"));
            }
            else if (benefit == SettlementLevelUtility.Benefit_lvl2_TraderStock)
            {
                var traderQuantity = (int)Math.Floor((SettlementScoreUtility.GetTraderQuantityMultiplierFromSettlementScore(statistics.totalPointsByScore + statistics.totalPointsByRoomTypes) - 1) * 100);
                tipText = benefit.Translate(traderQuantity.Named("QUANTITY"));
            } else
            {
                tipText = benefit.Translate();
            }

            const int benefitSize = 35;
            // achieved level benefits:
            // benefit0
            var benefitX = benefitsArea.TopPartPixels(benefitSize);
            var retBenefitsArea = benefitsArea.BottomPartPixels(benefitsArea.height - benefitSize - 5);
            drawImage(benefitX, Textures_SettledIn.BackgroundShade, 0.5f);
            if (Mouse.IsOver(benefitX))
            {
                GUI.DrawTexture(benefitX, TexUI.HighlightTex);
            }
            if (showDetails)
            {
                var icon = benefitIcons.ContainsKey(benefit) ? benefitIcons[benefit] : Textures_SettledIn.Unknown;
                if (!benefitIcons.ContainsKey(benefit))
                {
                    Log.ErrorOnce("Benefit=" + benefit + " has no icon", 4712337);
                }
                TooltipHandler.TipRegion(benefitX, new TipSignal(tipText, 0.0f));
                drawImage(benefitX, icon);
            }
            else
            {
                drawImage(benefitX, Textures_SettledIn.Unknown, 0.5f);
            }

            return retBenefitsArea;
        }

        private Rect drawAndHandleSettlementLevelInSummary(int level, Rect restOfSummary)
        {
            const float settlementPhaseIconSize = 80;

            var settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter0;
            switch (level)
            {
                case 0: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter0; break;
                case 1: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter1; break;
                case 2: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter2; break;
                case 3: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter3; break;
                case 4: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter4; break;
                case 5: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter5; break;
                case 6: settlementLevelIconToDraw = Textures_SettledIn.SettlementCenter6; break;
                default: break;
            }

            var coolNewSummarySettlementLevelX = restOfSummary.LeftPartPixels(settlementPhaseIconSize);
            var retCoolNewSummaryRest = restOfSummary.RightPartPixels(restOfSummary.width - settlementPhaseIconSize - 50);

            var iconX = coolNewSummarySettlementLevelX.TopPartPixels(settlementPhaseIconSize);
            var benefitsArea = coolNewSummarySettlementLevelX.BottomPartPixels(coolNewSummarySettlementLevelX.height - iconX.height - 10);

            // draw background and hovering in case mouse is over
            drawImage(iconX, Textures_SettledIn.BackgroundShade, 1.3f);
            if (Mouse.IsOver(iconX))
            {
                GUI.DrawTexture(iconX, TexUI.HighlightTex);
            }
            // get to know the tooltip text etc. and draw the tooltip
            string tooltip;
            var isUpgradableLevel = SettlementLevelUtility.CheckRequirements(level, Current.Game?.CurrentMap, out tooltip);
            TooltipHandler.TipRegion(iconX, new TipSignal(tooltip, 0.0f));
            // achieved levels:
            if (statistics.settlementLevel >= level) 
            {
                drawImage(iconX, settlementLevelIconToDraw, 1.0f);
                // temporarily render the glow to differenciate the levels 5 and 6
                if (level == 6)
                {
                    drawImage(iconX, Textures_SettledIn.SettlementCenter5glow, 1.0f);
                }
                drawImage(iconX, Textures_SettledIn.Checkmark, 0.5f);
            } 
            // next level if we fulfill the requirements:
            else if (statistics.settlementLevel == level - 1 && isUpgradableLevel)
            {
                drawImage(iconX, settlementLevelIconToDraw, 1.0f);
                drawImage(iconX, Textures_SettledIn.UpgradeSettlementIcon, 0.5f);
            }
            // next level if we dont fulfill the requirements OR future levels:
            else
            {
                drawImage(iconX, Textures_SettledIn.Unknown, 0.5f);
            }

            // let's add the benefits below
            var benefitsToShow = SettlementLevelUtility.GetBenefitListOfLevel(level);
            foreach(var benefit in benefitsToShow) {
                benefitsArea = drawAndHandleLevelBenefitInSummary(benefit, benefitsArea, statistics.settlementLevel >= level || (statistics.settlementLevel == level - 1 && isUpgradableLevel));
            }

            return retCoolNewSummaryRest;
        }

        public override void DoWindowContents(Rect canvas)
        {
            
            // setup the font and don't expect it to be right
            Text.Font = GameFont.Small;
            Color oldColor;

            // reserve space for summary
            var summary = canvas.TopPartPixels(100);
            var rest = canvas.BottomPartPixels(canvas.height - summary.height);
            // reserve space for coolnewsummary
            var coolNewSummary = rest.TopPartPixels(180);
            rest = rest.BottomPartPixels(rest.height - coolNewSummary.height);
            // reserve space for room breakdown
            var roomBreakdownHeader = rest.TopPartPixels(Text.LineHeight);
            rest = rest.BottomPartPixels(rest.height - Text.LineHeight);


            // draw region summary
            var skipLeftAndRight = (summary.width - 200 - 200 - 300) / 2;
            summary = summary.RightPartPixels(summary.width- skipLeftAndRight);
            summary = summary.LeftPartPixels(summary.width - skipLeftAndRight);
            var summaryScoreInPointsIcon = summary.LeftPartPixels(200);
            var summaryRest = summary.RightPartPixels(summary.width - 200);
            var summaryScoreInTypesIcon = summaryRest.RightPartPixels(200);
            summaryRest = summaryRest.LeftPartPixels(summaryRest.width - 200);
            var summaryLevel = summaryRest;

            // drawing the summaryIcon area
            drawImage(summaryLevel, Textures_SettledIn.ScoreIcon, 1.3f);
            try
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                oldColor = GUI.color;
                GUI.color = new Color(1f, 0.5f, 0.5f, 1.0f);
                Widgets.Label(summaryLevel, "Level " + statistics.settlementLevel.ToString());
                GUI.color = oldColor;
            }
            finally
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            drawImage(summaryScoreInPointsIcon, Textures_SettledIn.ScoreIcon, 1.0f);
            try
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(summaryScoreInPointsIcon, Math.Floor(statistics.totalPointsByScore + statistics.totalPointsByRoomTypes).ToString() + "\n" + "points");
            }
            finally
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            drawImage(summaryScoreInTypesIcon, Textures_SettledIn.ScoreIcon, 1.0f);
            try
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(summaryScoreInTypesIcon, statistics.achievedRoomTypes + " of " + statistics.validRoomTypes + "\nrooms");
            }
            finally
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            // region coolNewSummary
            const float settlementPhaseIconSize = 80;
            oldColor = GUI.color;
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1.0f);
            Widgets.DrawLineHorizontal(coolNewSummary.x + 10, coolNewSummary.y + settlementPhaseIconSize / 2, coolNewSummary.width - 20);
            GUI.color = oldColor;
            {
                var leftGapSummary = coolNewSummary.LeftPartPixels(40);
                var coolNewSummaryRest = coolNewSummary.RightPartPixels(coolNewSummary.width - leftGapSummary.width);

                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(0, coolNewSummaryRest);
                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(1, coolNewSummaryRest);
                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(2, coolNewSummaryRest);
                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(3, coolNewSummaryRest);
                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(4, coolNewSummaryRest);
                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(5, coolNewSummaryRest);
                coolNewSummaryRest = drawAndHandleSettlementLevelInSummary(6, coolNewSummaryRest);
            }
            /*
            // trader prices
            if (Mouse.IsOver(benefitsTraderInventoryIcon))
            {
                GUI.DrawTexture(benefitsTraderInventoryIcon, TexUI.HighlightTex);
                //Widgets.DrawHighlight(benefitsTraderInventoryIcon);
                //Widgets.DrawBox(benefitsTraderInventoryIcon, 1, null);
            }
            drawImage(benefitsTraderInventoryIcon, Textures_SettledIn.TraderInventoryIcon, 1.0f);
            var traderQuantity = (int)Math.Floor((SettlementScoreUtility.GetTraderQuantityMultiplierFromSettlementScore(statistics.totalPointsByScore + statistics.totalPointsByRoomTypes) - 1) * 100);
            TooltipHandler.TipRegion(benefitsTraderInventoryIcon, new TipSignal("+ " + traderQuantity + "% trader inventory based on total settlement score", 0.0f));
            */

            // room breakdown header
            Widgets.Label(roomBreakdownHeader, "Room Breakdown:");
            

            /*
            // draw header row
            var headerRow = rest.TopPartPixels(35);
            rest = rest.BottomPartPixels(rest.height - headerRow.height);
            headerRow.width -= 20;
            */

            // setup data for the table
            var entryHeight = 4 * Text.LineHeight; // overview of room type height
            var entryHeightRoomDetails = 1 * Text.LineHeight; // height per each room of this type
            // lets estimate the required height by number of pawns
            var allRoomsCount = statistics.roomRoles.Sum(roomRole => { return roomRole.rooms.Count; });
            var estimatedContentsHeight = entryHeight * statistics.roomRoles.Count + entryHeightRoomDetails * allRoomsCount;

            var scrollView = new Rect(0, 0, canvas.width - 20, estimatedContentsHeight);
            // lets start with the scrollable list that will contain the pawns
            Widgets.BeginScrollView(rest, ref scrollPosition, scrollView, true);

            // render the pawns in a table
            float offsetCurrRow = 0;
            int numEntry = 0;

            foreach (var roomTypeStatictic in statistics.roomRoles)
            {
                var row = new Rect(0f, offsetCurrRow, scrollView.width, entryHeight);
                var rowIncludingRooms = new Rect(0f, offsetCurrRow, scrollView.width, entryHeight + entryHeightRoomDetails * roomTypeStatictic.rooms.Count);
                offsetCurrRow += entryHeight;
                // background renderings
                if (numEntry % 2 == 1)
                {
                    Widgets.DrawAltRect(rowIncludingRooms);
                }
                numEntry += 1;
                /*
                if (Mouse.IsOver(row))
                {
                    GUI.DrawTexture(row, TexUI.HighlightTex);
                }
                */
                Widgets.DrawLineHorizontal(row.x, row.y, scrollView.width);

                var iconRect = row.LeftPartPixels(entryHeight);
                //drawImage(iconRect, roomTypeStatictic.texture, 0.9f);
                var textRect = row.RightPartPixels(row.width - iconRect.width);
                textRect.width -= entryHeight;

                var label = roomTypeStatictic.def?.label;
                label = label.Trim();
                var parts = label.Split(' ');
                label = "";
                for (int i = 0; i < parts.Length;i++) 
                {
                    label += parts[i].First().ToString().ToUpper() + String.Join("", parts[i].Skip(1)) + " ";
                }

#if DEBUG
                label = label + "(" + roomTypeStatictic.def.defName + ")";
#endif

                if (!String.IsNullOrEmpty(roomTypeStatictic.overrideLabel))
                {
                    label = roomTypeStatictic.overrideLabel;
                }
                Widgets.Label(textRect, label);
                textRect.y += Text.LineHeight;
                textRect.height -= Text.LineHeight;
                var description = roomTypeStatictic.def?.description;
                if (!String.IsNullOrEmpty(roomTypeStatictic.overrideDescription))
                {
                    description = roomTypeStatictic.overrideDescription;
                }
                Widgets.Label(textRect, description);

                var checkmarkRect = row.RightPartPixels(entryHeight);
                if (roomTypeStatictic.rooms?.Count > 0 && roomTypeStatictic.validForRoomRolePoints)
                {
                    drawImage(checkmarkRect, Textures_SettledIn.Checkmark, 0.5f);
                }

                // todo: add all rooms as small list
                foreach (var room in roomTypeStatictic.rooms)
                {
                    oldColor = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(entryHeight, offsetCurrRow, scrollView.width - iconRect.width);
                    GUI.color = oldColor;
                    var roomSubEntry = new Rect(entryHeight, offsetCurrRow, scrollView.width - iconRect.width, entryHeightRoomDetails);
                    offsetCurrRow += entryHeightRoomDetails;
                    if (Mouse.IsOver(roomSubEntry))
                    {
                        GUI.DrawTexture(roomSubEntry, TexUI.HighlightTex);
                        if (room.location != null)
                        {
                            room.location.Highlight(arrow: true, colonistBar: false);
                        }
                    }
                    if (Widgets.ButtonInvisible(roomSubEntry))
                    {
                        Log.Debug("MainTabWindow_SettlementScores: Clicked on room=" + room + " with location=" + room.location);
                        if (room.location != null && room.location.targets != null && room.location.targets.Count > 0 && CameraJumper.CanJump(room.location.targets[0]))
                        {
                            Log.Debug("MainTabWindow_SettlementScores: Jumping to target=" + room.location.targets[0]);
                            CameraJumper.TryJumpAndSelect(room.location.targets[0]);
                        }
                    }
                    string labelRoom = "err name not found";
                    try 
                    {
                        labelRoom = RoomNameUtility.GetRoomRoleLabel(room.roomRef); 
                    } 
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }
                    

                    Widgets.Label(roomSubEntry, labelRoom + ": " + Math.Floor(room.score) + " points");
                }
            }
            

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void drawImage(Rect outerRect, Texture2D icon, float scale=1.0f)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Vector2 texProportions = new Vector2(icon.width, icon.height);
                Rect texCoords = new Rect(0f, 0f, 1f, 1f);

                Rect rect = new Rect(0f, 0f, texProportions.x, texProportions.y);
                float num = ((!(rect.width / rect.height < outerRect.width / outerRect.height)) ? (outerRect.width / rect.width) : (outerRect.height / rect.height));
                num *= scale;
                rect.width *= num;
                rect.height *= num;
                rect.x = outerRect.x + outerRect.width / 2f - rect.width / 2f;
                rect.y = outerRect.y + outerRect.height / 2f - rect.height / 2f;
                GenUI.DrawTextureWithMaterial(rect, icon, null, texCoords);
            }
        }

        public static int Scrollbar(Rect drawIn, int min, int max, int steps, int settingIn, string tooltip = null)
        {
            Rect SliderOffset = drawIn.LeftPartPixels(drawIn.width - 10);
            //Widgets.Label(drawIn, setting.ToString() + " silver");
            var settingUnrounded = Widgets.HorizontalSlider(
            SliderOffset,
            settingIn, min, max, true);
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(drawIn))
                {
                    Widgets.DrawHighlight(drawIn);
                }
                TooltipHandler.TipRegion(drawIn, tooltip);
            }
            return (int)(Math.Round(settingUnrounded / (double)steps, 0) * steps);
        }
    }
}
