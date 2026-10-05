using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace DanielRenner.SettledIn
{
    public class ITab_TradeNetwork : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private static Texture2D TradeBarTex;

        public ITab_TradeNetwork()
        {
            this.size = new Vector2(500f, 450f);
            this.labelKey = "DanielRenner.SettledIn.TabTradeNetwork";
        }

        protected override void FillTab()
        {
            if (TradeBarTex == null)
                TradeBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.24f)); // A nice "Trading Green"

            Building_TradeDepot depot = base.SelThing as Building_TradeDepot;
            if (depot == null) return;

            // Use absolute coordinates for the main container
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(20f);
            const float tableTopDistanceForEfficiencyBar = 110f; // Fixed Y position after the bar section
            Rect efficiencyBar = new Rect(rect.x, rect.y, rect.width, tableTopDistanceForEfficiencyBar - rect.y);

            // ... [Efficiency Bar logic remains same, starting at rect.y] ...
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(efficiencyBar);

            // 1. Efficiency Bar Section
            Text.Font = GameFont.Medium;
            listing.Label("Global Trade Efficiency");

            Text.Font = GameFont.Small;
            float efficiency = depot.TradeFactorCached;
            // Map 0.4 - 1.4 to 0-1 for the bar
            float barPct = Mathf.InverseLerp(0.4f, 1.4f, efficiency);

            Rect barRect = listing.GetRect(28f);
            // Draw background
            Widgets.DrawBoxSolid(barRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
            // Draw fill
            Widgets.FillableBar(barRect, barPct, TradeBarTex);

            // Overlay text on the bar
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, $"{efficiency:0.00}x Efficiency");
            Text.Anchor = TextAnchor.UpperLeft;

            // Labels for the min/max ends
            Rect labelRow = listing.GetRect(18f);
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(labelRow.LeftPart(0.5f), "Minimum: 0.40x");
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(labelRow.RightPart(0.5f), "Maximum: 1.40x");
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            listing.End();         

            // Column Widths (Percentages of the available width)
            float canvasWidth = rect.width;
            float colName = canvasWidth * 0.4f;
            float colDist = canvasWidth * 0.25f;
            float colRep = canvasWidth * 0.2f;
            float colScore = canvasWidth * 0.15f;

            // --- HEADERS (Outside ScrollView) ---
            Rect headerRect = new Rect(rect.x, tableTopDistanceForEfficiencyBar, rect.width, 25f);
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(headerRect.x, headerRect.y, colName, 25f), "Settlement Name");
            Widgets.Label(new Rect(headerRect.x + colName, headerRect.y, colDist, 25f), "Distance");
            Widgets.Label(new Rect(headerRect.x + colName + colDist, headerRect.y, colRep, 25f), "Reputation");
            Widgets.Label(new Rect(headerRect.x + colName + colDist + colRep, headerRect.y, colScore, 25f), "Score");
            Widgets.DrawLineHorizontal(rect.x, headerRect.yMax, rect.width);
            GUI.color = Color.white;

            // --- THE LIST (Inside ScrollView) ---
            // outRect defines the "Window" we see through.
            Rect outRect = new Rect(rect.x, headerRect.yMax + 5f, rect.width, rect.height - headerRect.yMax);
            var effects = depot.TradeEffects; // Ensure this property is public in Building_TradingDepot!

            if (effects != null && effects.Count > 0)
            {
                // viewRect defines the total "Canvas" size. 
                // NOTE: X must be 0 here.
                Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, effects.Count * 35f);
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

                float curY = 0f;
                for (int i = 0; i < effects.Count; i++)
                {
                    var s = effects[i];
                    // RowRect x must be 0 relative to viewRect
                    Rect rowRect = new Rect(0f, curY, viewRect.width, 32f);

                    if (i % 2 == 0) Widgets.DrawLightHighlight(rowRect);

                    // Row Content
                    Text.Font = GameFont.Small;
                    // Draw name - Truncate handles long faction names
                    Widgets.Label(new Rect(5f, curY + 4f, colName - 10f, 25f), s.settlement.LabelShortCap.Truncate(colName - 10f));

                    Text.Font = GameFont.Tiny;
                    Widgets.Label(new Rect(colName, curY + 6f, colDist, 25f), $"{s.distance:0} tiles");

                    // Reputation
                    GUI.color = s.goodwill >= 50 ? Color.cyan : (s.goodwill >= 0 ? Color.green : Color.red);
                    Widgets.Label(new Rect(colName + colDist, curY + 6f, colRep, 25f), s.goodwill.ToString("F0"));
                    GUI.color = Color.white;

                    // Score
                    Widgets.Label(new Rect(colName + colDist + colRep, curY + 6f, colScore, 25f), s.finalScore.ToString("P0"));

                    curY += 35f;
                }
                Widgets.EndScrollView();
            }
            else
            {
                Text.Font = GameFont.Small;
                GUI.color = Color.gray;
                Widgets.Label(new Rect(rect.x, headerRect.yMax + 10f, rect.width, 30f), "Scanning trade frequencies...");
                GUI.color = Color.white;
            }
        }
    }
}