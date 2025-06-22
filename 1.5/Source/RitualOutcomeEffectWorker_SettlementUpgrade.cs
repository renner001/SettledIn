using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace DanielRenner.SettledIn
{
    public class RitualOutcomeEffectWorker_SettlementUpgrade : RitualOutcomeEffectWorker_Speech
    {
        public RitualOutcomeEffectWorker_SettlementUpgrade()
        {
        }

        public RitualOutcomeEffectWorker_SettlementUpgrade(RitualOutcomeEffectDef def)
            : base(def)
        {
        }

        public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            Log.Debug("Upgrading settlement as result of ritual");
            // notify the settlement center to do the upgrade
            Building_SettlementCenter targetCenter = jobRitual.Map.listerThings.ThingsOfDef(DefOfs_SettledIn.SettlementCenter).FirstOrDefault() as Building_SettlementCenter;
            if (targetCenter == null)
            {
                Log.Error("failed to upgarde settlement at end of upgrade ritual, since there is no settlement center on the map");
                return;
            } 
            else
            {
                targetCenter.UpgradeSettlement();
            }
            base.Apply(progress, totalPresence, jobRitual);
        }
    }
}
