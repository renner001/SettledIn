using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace DanielRenner.SettledIn
{
    public class Dialog_SelectThingDef : Window
    {
        private readonly Action<ThingDef> onSelect;
        private readonly string title;
        private Vector2 scrollPos;

        private List<ThingDef> availableDefs; 
        private List<ThingDef> filteredDefs;
        private string searchQuery = "";

        public override Vector2 InitialSize => new Vector2(700f, 600f);

        public Dialog_SelectThingDef(Action<ThingDef> onSelect, string title, IEnumerable<ThingDef> defs)
        {
            this.onSelect = onSelect;
            this.title = title;
            forcePause = true;
            absorbInputAroundWindow = true;
            availableDefs = defs
                .OrderBy(d => d.label.CapitalizeFirst())
                .ToList();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            filteredDefs = availableDefs;
        }

        private void ApplySearchFilter()
        {
            if (searchQuery.NullOrEmpty())
            {
                filteredDefs = availableDefs;
            }
            else
            {
                string query = searchQuery.ToLowerInvariant();
                filteredDefs = availableDefs
                    .Where(d =>
                        (!d.label.NullOrEmpty() && d.label.ToLowerInvariant().Contains(query)) ||
                        d.defName.ToLowerInvariant().Contains(query))
                    .ToList();
            }
        }

        public override void DoWindowContents(UnityEngine.Rect inRect)
        {
            float y = 0f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, y, inRect.width, 30f), title);
            Text.Font = GameFont.Small;
            y += 35f;

            // Search box
            Rect searchRect = new Rect(0, y, inRect.width, 30f);
            string newQuery = Widgets.TextField(searchRect, searchQuery);
            if (newQuery != searchQuery)
            {
                searchQuery = newQuery;
                ApplySearchFilter();
            }
            y += 40f;

            Rect outRect = new Rect(0, y, inRect.width, inRect.height - y);
            Rect viewRect = new Rect(0, 0, outRect.width - 20f, filteredDefs.Count * 28f);

            Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);

            float curY = 0;
            // start with the "off" option
            {
                Rect row = new Rect(0, curY, viewRect.width, 28f);
                Widgets.DrawTextureFitted(new Rect(row.x, row.y, 24f, 24f), TexButton.Minus, 1f);

                string label = "None";

                if (Widgets.ButtonText(new Rect(row.x + 30f, row.y, row.width - 30f, 24f), label))
                {
                    onSelect?.Invoke(null);
                    Close();
                }

                curY += 28f;
            }
            

            foreach (var def in filteredDefs)
            {
                Rect row = new Rect(0, curY, viewRect.width, 28f);
                Widgets.ThingIcon(new Rect(row.x, row.y, 24f, 24f), def);

                string label = def.LabelCap;

                if (Widgets.ButtonText(new Rect(row.x + 30f, row.y, row.width - 30f, 24f), label))
                {
                    onSelect?.Invoke(def);
                    Close();
                }

                curY += 28f;
            }

            Widgets.EndScrollView();
        }
    }
}