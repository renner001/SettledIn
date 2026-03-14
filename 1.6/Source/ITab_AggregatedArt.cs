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

    public class ITab_AggregatedArt : ITab_Art
    {
        private Vector2 scrollPosition;

        private static readonly Vector2 WinSize = new Vector2(460f, 450f);

        public ITab_AggregatedArt()
        {
            this.size = WinSize;
            this.labelKey = "TabArt"; // Uses vanilla "Art" label
        }

        private CompArt SelectedCompArt => base.SelThing.TryGetComp<CompArt>();

        TaleReference cachedRef;
        string cachedText;

        protected override void FillTab()
        {
            var comp = SelectedCompArt;
            if (comp == null) return;

            // Define the outer rectangle (the window boundaries)
            // We use WinSize.x and WinSize.y which we defined above
            Rect outRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(15f);

            // Prepare the text content
            string title = comp.Title;

            if (cachedRef == null || cachedRef != comp.TaleRef || cachedText == null)
            {
                cachedText = comp.GenerateImageDescription();
                cachedRef = comp.TaleRef;
            }
            string desc = cachedText;

            // Calculate how much space we need
            float textWidth = outRect.width - 20f; // Padding for the scrollbar

            Text.Font = GameFont.Medium;
            float titleHeight = Text.CalcHeight(title, textWidth);

            Text.Font = GameFont.Small;
            float descHeight = Text.CalcHeight(desc, textWidth);

            // Total height of the virtual 'canvas'
            float totalContentHeight = titleHeight + 15f + descHeight;

            // This is the 'virtual' rectangle that is actually scrolled
            Rect viewRect = new Rect(0f, 0f, textWidth, totalContentHeight);

            // Start the scroll view
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = 0f;

            // Draw the Title
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, curY, textWidth, titleHeight);
            Widgets.Label(titleRect, title);
            curY += titleHeight + 10f;

            // Draw a separator
            Widgets.DrawLineHorizontal(0f, curY - 5f, textWidth);

            // Draw the Chronicle text
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0f, curY, textWidth, descHeight);
            Widgets.Label(descRect, desc);

            Widgets.EndScrollView();
        }
    }
}
