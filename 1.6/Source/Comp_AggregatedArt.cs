using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    public class CompProperties_AggregatedArt : CompProperties_Art
    {
        public CompProperties_AggregatedArt()
        {
            // This links the properties to your specific C# component class
            this.compClass = typeof(Comp_AggregatedArt);
        }
    }

    public class ArtDataHolder : IExposable
    {
        // 1. The Direct Reference (For easy access in your code)
        public Tale tale;

        // 2. The "Ticket" (Keeps the Tale alive in the Manager)
        public TaleReference TaleRef;

        // 3. Metadata
        public Pawn Author;
        public int dateTicks; // Absolute game ticks when this was recorded

        // Default constructor required for Scribe (Loading)
        public ArtDataHolder()
        {
        }

        // Main Constructor
        public ArtDataHolder(Tale newTale, Pawn author, TaleReference useReference = null)
        {
            this.tale = newTale;
            this.Author = author;
            this.dateTicks = GenTicks.TicksAbs; // Capture current date

            if (useReference == null)
                this.TaleRef = new TaleReference(newTale);
            else
                TaleRef = useReference;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Author, "author", false);
            Scribe_Values.Look(ref this.dateTicks, "dateTicks");

            Scribe_Deep.Look(ref this.TaleRef, "taleRef");
        }

        // Cleanup Method
        // MUST be called when the parent Building/Item is destroyed
        public void Notify_LostReference()
        {
            if (this.TaleRef != null)
            {
                this.TaleRef.ReferenceDestroyed();
                this.TaleRef = null;
            }
        }

        // Helper to format the date string
        public string DateString => GenDate.DateFullStringAt(this.dateTicks, Find.WorldGrid.LongLatOf(0));

        public TaggedString GetText(RulePackDef include = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateString + /* " "  +  Author +*/ ":");
            sb.AppendLine(TaleRef.GenerateText(TextGenerationPurpose.ArtDescription, include));
            return sb.ToTaggedString();
        }
    }

    public class Comp_AggregatedArt : CompArt
    {
        private List<ArtDataHolder> recordedTales = new List<ArtDataHolder>();

        // set the title to our fixed title on any spawn - rather too often than sorry!
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            titleIntField.SetValue(this, "DanielRenner.SettledIn.ChroniclesTitle".Translate()); // sadly private in the base class...
        }

        public override TaggedString GenerateImageDescription()
        {
            Log.DebugOnce($"generating description for {this.parent} based on {recordedTales.Count} items chronicled");
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DanielRenner.SettledIn.SettlementCenterArtIntro".Translate());
            foreach (var tale in recordedTales)
            {
                sb.AppendLine(tale.GetText(this.Props.descriptionMaker));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (TaleRef == null) // we create a random tale just for compatibility with mods expectign a tale in every art
            {
                InitializeArt(ArtGenerationContext.Colony);
                // replace the title in case we selected a tale that is incapable of generatign titles...
            }

            // Ensure the history and the IDs survive save/load
            Scribe_Collections.Look(ref recordedTales, "recordedTales", LookMode.Deep);

            if (recordedTales == null)
                recordedTales = new List<ArtDataHolder>();
        }

        static FieldInfo taleRefField = typeof(CompArt).GetField("taleRef", BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo titleIntField = typeof(CompArt).GetField("titleInt", BindingFlags.Instance | BindingFlags.NonPublic); // todo: replace the ITab_Art logics to get rid of this ugly shit

        /// <summary>
        /// Scans the colony's history for a significant event that hasn't been recorded yet.
        /// </summary>
        public bool TryRecordNewHistoricalEvent()
        {
            if (recordedTales.Count > 500)
            {
                Log.Message($"skipped recording a new tale as there are already {recordedTales.Count} tales recorded in {parent}");
                return false;
            }

            // Filter for Permanent Historical events (deaths, weddings, major victories)
            // that haven't been added to this specific building yet.
            var potentialTales = Find.TaleManager.AllTalesListForReading.Where(t =>
                {
                    return t.def.usableForArt
                    && t.def.rulePack != null
                    && recordedTales.FindIndex(recTale => 
                    {
                        return recTale.tale == t;
                    }) < 0;
                }
            );

            if (potentialTales.TryRandomElement(out Tale selectedTale))
            {
                var taleRef = new TaleReference(selectedTale);
                var text = taleRef.GenerateText(TextGenerationPurpose.ArtDescription, this.Props.descriptionMaker);
                Log.Debug($"AppendTaleToChronicle: {text}");
                recordedTales.Add(new ArtDataHolder(selectedTale,null, taleRef));
                taleRefField.SetValue(this, taleRef); // change the tale we are referencing to trick a refresh in the ITab_Art
                return true;
            }
            return false;
        }

        public override string CompInspectStringExtra()
        {
            return this.recordedTales.Count +  "DanielRenner.SettledIn.ChronicleStored".Translate();
        }
    }

}
