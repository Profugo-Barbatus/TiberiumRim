using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumVein : TiberiumCrystal
    {
        private const float minGrowthTemperature = 5;
        private int bledTimes = 0;


        //Veins shouldn't infect pawns directly
        public override void infect(Pawn p)
        {
            if (this.def.defName.Contains("Vein"))
            {
                hurt(p);

                //If Pawn is close to the center it gets 'attacked with poison'
                var c = this.RandomAdjacentCell8Way();
                var t = c.GetFirstBuilding(Map);
                if (t != null && c.InBounds(this.Map))
                {
                    if (t.def.defName.Contains("Veinhole_TBNS") && Rand.Chance(0.2f))
                    {
                        base.infect(p);
                    }
                }
            }
        }

        //Though they still do attack if a pawn comes too close
        public void hurt(Pawn p)
        {
            int amt = 3;
            if (p.apparel == null)
            {
                amt = amt * 3;
            }
            DamageInfo damage = new DamageInfo(DamageDefOf.Blunt, amt);

            if (Rand.Chance(0.1f) && !p.def.defName.Contains("Veinmonster") && p.Position.InBounds(this.Map))
            {
                p.TakeDamage(damage);
            }
        }

        //For aesthetics and the fact that Veins are a living being, let's make it bleed
        public void Bleed(Map map)
        {
            //Only bleed if health is lowered - Only allowing it to bleed a few times to not lag the game
            if (this.HitPoints < this.MaxHitPoints && this.bledTimes < 5)
            {
                IntVec3 pos = this.PositionHeld;
                FilthMaker.MakeFilth(pos, map, ThingDefOf.FilthBlood, 1);
                bledTimes = bledTimes + 1;
            }
        }

        //Veins also corrupt walls
        public override void corruptWall()
        {
            var c = this.RandomAdjacentCell8Way();
            var p = c.GetFirstBuilding(this.Map);
            if (p != null && c.InBounds(this.Map))
            {
                ThingDef wall = DefDatabase<ThingDef>.GetNamed("VeinTiberiumRock_TBNS", true);
                IntVec3 loc = p.Position;

                if (p.def.mineable && !p.def.defName.Contains("TBNS"))
                {
                    GenSpawn.Spawn(wall, loc, Map);
                    return;
                }
            }
            return;
        }

        //Veins don't mutate creatures
        public override void spawnFiendOrVisceroid(IntVec3 pos, BodyDef p)
        {
            return;
        }

        //Overriding ticklong for the bleeding
        public override void TickLong()
        {
            base.TickLong();
            Bleed(Map);
            spawnVeinmonster(this.Position);
        }

        public virtual void spawnVeinmonster(IntVec3 pos)
        {
            if (Rand.Chance(0.0001f))
            {
                PawnKindDef Veinmonster = DefDatabase<PawnKindDef>.GetNamed("Veinmonster_TBI", true);
                PawnGenerationRequest request = new PawnGenerationRequest(Veinmonster);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                GenSpawn.Spawn(pawn, pos, Map);
            }
        }

        //Not quite sure what to use this for. So far it would only kill Veins that are not connected to an adult Vein. This rarely happens and basically causes unnecessary checks
        /*   public void Networkcheck(Map map)
           {
               if (GenAdjFast.AdjacentCells8Way(this.Position).Any(i => i.GetFirstThing(map, ThingDef.Named("Veinhole_TBNS")) != null))
               {
                   return;
               }
                   if (GenAdjFast.AdjacentCells8Way(this.Position).Any(i =>
                   {
                       var c = i.GetFirstThing(map, this.def);
                       if (c != null)
                       {
                           TiberiumVein plant = c as TiberiumVein;
                           if (plant != null && !plant.HarvestableNow)
                           {
                               return false;
                           }
                           if (plant != null && plant.HarvestableNow)
                           {
                               return true;
                           }
                       }
                       return false;
                   }))
                   {
                       return;
                   }
                   this.Destroy(DestroyMode.Vanish);
           } */
    }
}