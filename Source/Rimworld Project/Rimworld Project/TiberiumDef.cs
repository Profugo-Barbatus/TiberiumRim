﻿using System;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class TiberiumDef : ThingDef
    {
        public TerrainDef corruptsInto;
        public bool isExplosive = false;
        public int buildingDamage;
        public int entityDamage;
        public List<ThingDef> friendlyTo = new List<ThingDef>();
    }
}
