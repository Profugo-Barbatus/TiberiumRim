using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Comp_Decrystallizer : CompTerrainPump
    {
        private CompPowerTrader powerComp;

        private int progressTicks;

        private CompProperties_Decrystallizer Props
        {
            get
            {
                return (CompProperties_Decrystallizer)this.props;
            }
        }

        private float ProgressDays
        {
            get
            {
                return (float)this.progressTicks / 60000f;
            }
        }

        private float CurrentRadius
        {
            get
            {
                return Mathf.Min(this.Props.radius, this.ProgressDays / this.Props.daysToRadius * this.Props.radius);
            }
        }

        private bool Working
        {
            get
            {
                return this.powerComp == null || this.powerComp.PowerOn;
            }
        }

        private int TicksUntilRadiusInteger
        {
            get
            {
                float num = Mathf.Ceil(this.CurrentRadius) - this.CurrentRadius;
                if (num < 1E-05f)
                {
                    num = 1f;
                }
                float num2 = this.Props.radius / this.Props.daysToRadius;
                float num3 = num / num2;
                return (int)(num3 * 60000f);
            }
        }

        public override void CompTickRare()
        {
            if (this.Working)
            {
                this.progressTicks += 250;
                int num = GenRadial.NumCellsInRadius(this.CurrentRadius);
                for (int i = 0; i < num; i++)
                {
                    this.AffectCell(this.parent.Position + GenRadial.RadialPattern[i]);
                }
            }
        }

        protected override void AffectCell(IntVec3 c)
        {
            TerrainDef terrain = c.GetTerrain(this.parent.Map);
            TerrainDef Postterrain = null;
            if (terrain.defName.Contains("Tiberium") | terrain.defName.Contains("Vein") && !terrain.defName.Contains("TiberiumWater"))
            {
                Postterrain = DefDatabase<TerrainDef>.GetNamed("DecrystallizedSoil");
                this.parent.Map.terrainGrid.SetTerrain(c, Postterrain);
            }
            if(terrain.defName.Contains("TiberiumSand"))
            {
                Postterrain = DefDatabase<TerrainDef>.GetNamed("TiberiumSandDecrystallized");
                this.parent.Map.terrainGrid.SetTerrain(c, Postterrain);
            }
            return;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (this.CurrentRadius < this.Props.radius - 0.0001f)
            {
                GenDraw.DrawRadiusRing(this.parent.Position, this.CurrentRadius);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = string.Concat(new string[]
            {
                "TimePassed".Translate().CapitalizeFirst(),
                ": ",
                this.progressTicks.ToStringTicksToPeriod(true),
                "\n",
                "CurrentRadius".Translate().CapitalizeFirst(),
                ": ",
                this.CurrentRadius.ToString("F1")
            });
            if (this.ProgressDays < this.Props.daysToRadius && this.Working)
            {
                string text2 = text;
                text = string.Concat(new string[]
                {
                    text2,
                    "\n",
                    "RadiusExpandsIn".Translate().CapitalizeFirst(),
                    ": ",
                    this.TicksUntilRadiusInteger.ToStringTicksToPeriod(true)
                });
            }
            return text;
        }

    }

    public class CompProperties_Decrystallizer : CompProperties
    {
        public float radius = 9.9f;

        public float daysToRadius = 1f;

        public CompProperties_Decrystallizer()
        {
            this.compClass = typeof(Comp_Decrystallizer);
        }
    }
}
