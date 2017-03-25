using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    class HediffComp_Mutation : HediffComp
    {
        public override void CompPostTick()
        {
            base.CompPostTick();
            if (base.Pawn.IsHashIntervalTick(5000))
            {
                CheckSeverity(this.Pawn);
            }
        }

        public HediffCompProperties_TiberiumMutation Props
        {
            get
            {
                return (HediffCompProperties_TiberiumMutation)this.props;
            }
        }

        public void CheckSeverity(Pawn p)
        {
            HediffDef Infection = DefDatabase<HediffDef>.GetNamed("TiberiumContactPoison", true);
            HediffDef MutationGood = DefDatabase<HediffDef>.GetNamed("TiberiumMutationGood", true);
            HediffDef MutationBad = DefDatabase<HediffDef>.GetNamed("TiberiumMutationBad", true);
            HediffDef Addiction = DefDatabase<HediffDef>.GetNamed("TiberiumAddiction", true);
            HediffDef Exposure = DefDatabase<HediffDef>.GetNamed("TiberiumBuildupHediff", true);

            if (!p.health.hediffSet.HasHediff(Infection) && this.parent.Severity > 0.3 && Rand.Chance(0.1f))
            {

                List<BodyPartRecord> list = new List<BodyPartRecord>();

                foreach (BodyPartRecord i in p.RaceProps.body.AllParts)
                {
                    if (i.depth == BodyPartDepth.Outside && !p.health.hediffSet.PartIsMissing(i))
                    {
                        list.Add(i);
                    }
                }

                BodyPartRecord target = null;
                target = list.RandomElement();

                p.health.AddHediff(Infection, target, null);
                p.health.RemoveHediff(this.parent);
                HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
                return;
            }
            else if (!p.health.hediffSet.HasHediff(MutationGood) && this.parent.Severity > 0.8 && Rand.Chance(0.7f))
            {
                p.health.AddHediff(MutationGood);
                p.health.AddHediff(Addiction);
                p.health.RemoveHediff(this.parent);
                HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
                return;
            }
            else if(!p.health.hediffSet.HasHediff(MutationBad) && this.parent.Severity > 0.5 && Rand.Chance(0.1f))
            {
                p.health.AddHediff(MutationBad);
                p.health.AddHediff(Addiction);
                p.health.RemoveHediff(this.parent);
                HealthUtility.AdjustSeverity(p, Exposure, -1.5f);
            }

        }
    }

    public class HediffCompProperties_TiberiumMutation : HediffCompProperties
    {
        public HediffCompProperties_TiberiumMutation()
        {
            this.compClass = typeof(HediffComp_Mutation);
        }
    }
}
