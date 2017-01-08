using System;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class TiberiumDef : ThingDef
    {
        public TerrainDef corruptsInto;
        public int buildingDamage;
        public List<ThingDef> friendlyTo = new List<ThingDef>();
    }
}
