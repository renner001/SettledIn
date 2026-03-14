using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    /*
    public abstract class Thought_LayoutContextual : Thought_Memory
    {
        // The “target” that caused this thought (could be pawn, thing, or position)
        public GlobalTargetInfo targetInfo = GlobalTargetInfo.Invalid;

        public override IEnumerable<GlobalTargetInfo> GetLookTargets()
        {
            if (targetInfo.IsValid)
                yield return targetInfo;
        }

        public override string Description
        {
            get
            {
                string baseText = base.Description;
                if (targetInfo.IsValid)
                    baseText += $"\n\nCause: {GetTargetLabel()}";
                return baseText;
            }
        }

        public string GetTargetLabel()
        {
            if (!targetInfo.IsValid) return "Unknown";

            if (targetInfo.Thing is Pawn pawn)
                return pawn.NameShortColored;
            if (targetInfo.Thing is Thing thing)
                return thing.LabelCap;
            if (targetInfo.Cell.IsValid)
                return $"Location at {targetInfo.Cell}";
            return "Unknown";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref targetInfo, "targetInfo");
        }
    }
    */
}
