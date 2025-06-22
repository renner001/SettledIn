using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

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
            var settlementScoreManager = Current.Game?.GetComponent<GameComponent_SettlementScoreManager>();
            Map currentMap = Find.CurrentMap;
            if (currentMap != null && currentMap.IsPlayerHome && settlementScoreManager.cachedStatistics.ContainsKey(currentMap))
            {
                settlementScoreManager.UpdateCache(currentMap);
                statistics = settlementScoreManager.cachedStatistics[currentMap];
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

        public override void DoWindowContents(Rect canvas)
        {
            
            // setup the font and don't expect it to be right
            Text.Font = GameFont.Small;

            // draw summary
            var summary = canvas.TopPartPixels(100);
            var rest = canvas.BottomPartPixels(canvas.height - summary.height);

            var summaryScoreInPointsIcon = summary.LeftPartPixels(200);
            var summaryRest = summary.RightPartPixels(summary.width - 200);
            var summaryFollowsSignLeft = summaryRest.LeftPartPixels(50);
            summaryRest = summaryRest.RightPartPixels(summaryRest.width - 50);
            var summaryScoreInTypesIcon = summaryRest.RightPartPixels(200);
            summaryRest = summaryRest.LeftPartPixels(summaryRest.width - 200);
            var summaryFollowsSignRight = summaryRest.RightPartPixels(50);
            summaryRest = summaryRest.LeftPartPixels(summaryRest.width - 50);
            var summaryCenterText = summaryRest;

            // drawing the summaryIcon area
            drawImage(summaryScoreInPointsIcon, Textures_SettledIn.ScoreIcon, 1.0f);
            try
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(summaryScoreInPointsIcon, Math.Floor(statistics.totalPointsByScore).ToString());
            }
            finally
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            drawImage(summaryScoreInTypesIcon, Textures_SettledIn.ScoreIcon, 1.0f);
            try
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(summaryScoreInTypesIcon, Math.Floor(statistics.totalPointsByRoomTypes).ToString() + "\n" + statistics.achievedRoomTypes + " of " + statistics.validRoomTypes);
            }
            finally
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }


            // drawing the summaryText area
            var traderQuantity = (int)Math.Floor((SettlementScoreUtility.GetTraderQuantityMultiplierFromSettlementScore(statistics.totalPointsByScore + statistics.totalPointsByRoomTypes) - 1) * 100);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(summaryFollowsSignLeft, ">"); // replace by real icon
            Widgets.Label(summaryFollowsSignRight, "<"); // replace by real icon
            //Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(summaryCenterText, "+ " + traderQuantity + "% trader inventory");
            Text.Anchor = TextAnchor.UpperLeft;

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
                drawImage(iconRect, roomTypeStatictic.texture, 0.9f);
                var textRect = row.RightPartPixels(row.width - iconRect.width);
                textRect.width -= entryHeight;

                var label = roomTypeStatictic.def?.label + "(" + roomTypeStatictic.def.defName + ")";
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
                    var oldColor = GUI.color;
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

                    Widgets.Label(roomSubEntry, RoomNameUtility.GetRoomRoleLabel(room.roomRef) + ": " + room.score + " points");
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
