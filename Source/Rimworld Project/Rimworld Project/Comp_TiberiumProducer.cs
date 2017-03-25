using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    class Comp_TiberiumProducer : ThingComp
    {
        private CompProperties_TiberiumProducer Props
        {
            get
            {
                return (CompProperties_TiberiumProducer)this.props;
            }
        }

        public override void CompTickRare()
        {
            DestroyWalls();
            base.CompTickRare();
        }

        public void DestroyWalls()
        {
            var c = this.parent.RandomAdjacentCell8Way();
            if (c.InBounds(this.parent.Map))
            {
                var p = c.GetFirstBuilding(this.parent.Map);

                if (p != null)
                {
                    int amt = 150;

                    DamageInfo damage = new DamageInfo(DamageDefOf.Mining, amt);

                    if (!p.def.defName.Contains("TBNS"))
                    {
                        p.TakeDamage(damage);
                    }
                }
            }
        }
    }

    class CompProperties_TiberiumProducer : CompProperties
    {
        public CompProperties_TiberiumProducer()
        {
            this.compClass = typeof(Comp_TiberiumProducer);
        }
    }
}
