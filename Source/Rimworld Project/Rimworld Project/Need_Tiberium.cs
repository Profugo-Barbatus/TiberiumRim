using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;


namespace TiberiumRim
{
    public class Need_Tiberium : Need
    {
        private const float ThreshDesire = 0.01f;

        private const float ThreshSatisfied = 0.3f;

        public override int GUIChangeArrow
        {
            get
            {
                return -1;
            }
        }

        public TiberiumNeedCategory CurCategory
        {
            get
            {
                if (this.CurLevel > 0.3f)
                {
                    return TiberiumNeedCategory.Statisfied;
                }
                if (this.CurLevel > 0.01f)
                {
                    return TiberiumNeedCategory.Lacking;
                }
                return TiberiumNeedCategory.Urgent;
            }
        }

        public override float CurLevel
        {
            get
            {
                return base.CurLevel;
            }
            set
            {
                TiberiumNeedCategory curCategory = this.CurCategory;
                base.CurLevel = value;
            }
        }

        private float TiberiumNeedFallPerTick
        {
            get
            {
                return this.def.fallPerDay / 60000f;
            }
        }

        public Need_Tiberium(Pawn pawn) : base(pawn)
		{
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.3f);
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = Rand.Range(0.2f, 0.5f);
        }

        public override void NeedInterval()
        {
            if(this.pawn.CarriedBy != null)
            {
                return;
            }
            var c = this.pawn.RandomAdjacentCell8Way();
            if (c.InBounds(this.pawn.Map))
            {
                var t = c.GetFirstThing(this.pawn.Map, DefDatabase<ThingDef>.GetNamed("TiberiumGreen"));
                if (t != null)
                {
                    this.CurLevel += 0.05f;
                    return;
                }
                this.CurLevel -= this.TiberiumNeedFallPerTick * 350f;
            }
        }
    }
}
