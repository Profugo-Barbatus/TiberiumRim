using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    class IncidentWorker_AsteroidDrop : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int num = 0;
            int countToSpawn = 1;
            IntVec3 cell = IntVec3.Invalid;
            IntVec2 size = new IntVec2(4, 4);
            for (int i = 0; i < countToSpawn; i++)
            {
                //Makes sure asteroid doesn't attempt to land in a roofed area, or in an unseen area.
                Predicate<IntVec3> validator = delegate (IntVec3 c)
                {
                    if (c.Fogged(map))
                    {
                        return false;
                    }
                    foreach (IntVec3 current in GenAdj.CellsOccupiedBy(c, Rot4.North, size))
                    {
                        if (!current.Standable(map))
                        {
                            bool result = false;
                            return result;
                        }
                        if (map.roofGrid.Roofed(current))
                        {
                            bool result = false;
                            return result;
                        }
                    }
                    return map.reachability.CanReachColony(c);
                };
                IntVec3 intVec;
                if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(14, validator, map, out intVec))
                {
                    break;
                }

                dropRock(map, intVec);

                num++;
                cell = intVec;
            }
            if (num > 0)
            {
                if (map == Find.VisibleMap)
                {
                    Find.CameraDriver.shaker.DoShake(1f);
                }
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, new TargetInfo(cell, map, false), null);
            }
            return num > 0;
        }

        public virtual void dropRock(Map map, IntVec3 cell)
        {
            //Virtual class, to be overriden later.
        }
    }
}
