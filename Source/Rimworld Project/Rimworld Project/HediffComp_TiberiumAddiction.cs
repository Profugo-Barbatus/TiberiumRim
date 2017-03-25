using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class HediffComp_TiberiumAddiction : HediffComp
    {
        public override void CompPostTick()
        {
            base.CompPostTick();
            if (base.Pawn.IsHashIntervalTick(500))
            {
                Adjust(this.Pawn);
            }
        }

        public void Adjust(Pawn pawn)
        {
            var c = pawn.RandomAdjacentCell8Way();
            var t = c.GetFirstThing(pawn.Map, DefDatabase<ThingDef>.GetNamed("TiberiumGreen"));
            Need N = pawn.needs.AllNeeds.Find((Need x) => x.def.defName.Contains("Need_Tiberium"));
            HediffDef Exposure = DefDatabase<HediffDef>.GetNamed("TiberiumBuildupHediff", true);

            if (N != null)
            {
                if (t != null && c.InBounds(pawn.Map))
                {
                    HealthUtility.AdjustSeverity(pawn, this.parent.def, -this.parent.Severity);
                    HealthUtility.AdjustSeverity(pawn, this.parent.def, 1 - N.CurLevelPercentage * 0.999999f);
                }
            }
            if (pawn.health.hediffSet.HasHediff(Exposure))
            {
                HealthUtility.AdjustSeverity(pawn, Exposure, -0.5f);
            }
        }

        public HediffCompProperties_TiberiumAddiction Props
        {
            get
            {
                return (HediffCompProperties_TiberiumAddiction)this.props;
            }
        }
    }

    public class HediffCompProperties_TiberiumAddiction : HediffCompProperties
    {
        public HediffCompProperties_TiberiumAddiction()
        {
            this.compClass = typeof(HediffComp_TiberiumAddiction);
        }
    }
}

