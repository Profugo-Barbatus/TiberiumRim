using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    class AsteroidDef : IncidentDef
    {
        public ThingDef asteroidType;

        public String spawnCrystalType;

        public int spawnRadius;

        public int spawnAmount;
    }
}
