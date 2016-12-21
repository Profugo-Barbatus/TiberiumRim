using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiberiumRim;
using Verse;

namespace TiberiumRim
{
    class IncidentWorker_GreenAsteroid : IncidentWorker_AsteroidDrop
    {
        public override void dropRock(Map map, IntVec3 cell)
        {
            AsteroidDef Localdef = this.def as AsteroidDef;

            Building crater = (Building)GenSpawn.Spawn(Localdef.asteroidType, cell, map);
        }
    }
}
