using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    class Building_Veinhole : Building
    {
        List<Thing> plantsChecked = new List<Thing>();

        //Once a Veinhole dies, all the Veins die with it, for convience and the sake of the players' PCs it just does it instantly. If this starts to cause lags on bigger fields of Veins this will just be left out
        //until a better method is found.
        public static void destroyVeins(List<IntVec3> checkCoords)
        {
            checkCoords.ForEach(i =>
            {
                if (i.InBounds(Find.VisibleMap))
                {
                    Thing thing = i.GetFirstThing(Find.VisibleMap, ThingDef.Named("TiberiumVein"));
                    if (!thing.DestroyedOrNull())
                    {
                            thing.Destroy(DestroyMode.Vanish);
                            IntVec3[] cells = GenAdj.AdjacentCells;
                            destroyVeins(new List<IntVec3>() { i + cells[0], i + cells[1], i + cells[2], i + cells[3], i + cells[4], i + cells[5], i + cells[6], i + cells[7] });
                    }
                }
            });
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            destroyVeins(GenAdjFast.AdjacentCells8Way(this));
            base.Destroy(mode);
        }
    }
}
