using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{
    public class Dialog_Slider : Window
    {
        private readonly string title;
        private readonly System.Action<float> onValueChanged;
        private readonly float min, max;
        private float value;

        public override Vector2 InitialSize => new Vector2(300f, 100f);

        public Dialog_Slider(string title, System.Action<float> onValueChanged, float min, float max, float startValue)
        {
            this.title = title;
            this.onValueChanged = onValueChanged;
            this.min = min;
            this.max = max;
            this.value = startValue;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), title);
            Text.Anchor = TextAnchor.UpperLeft;

            value = Widgets.HorizontalSlider(new Rect(0f, 40f, inRect.width, 30f), value, min, max, false, $"{value:F1}");
            onValueChanged(value);
        }
    }
}
