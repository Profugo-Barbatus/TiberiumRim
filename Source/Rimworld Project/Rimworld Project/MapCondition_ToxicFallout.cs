using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Source.Detour;

namespace TiberiumRim
{
    public class MapCondition_TibToxicFallout : MapCondition
    {
        //Detour to save the Toxic Fallout Mapondition issue
        public virtual void _DoCellSteadyEffects(IntVec3 c)
        {
            if (!c.Roofed(base.Map))
            {
                List<Thing> thingList = c.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
        
                    if (thing is Plant && !thing.def.defName.Contains("Tiberium"))
                    {
                        if (Rand.Value < 0.0065f)
                        {
                            thing.Destroy(DestroyMode.Kill);
                        }
                    }
                    else if (thing.def.category == ThingCategory.Item)
                    {
                        CompRottable compRottable = thing.TryGetComp<CompRottable>();
                        if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
                        {
                            compRottable.RotProgress += 3000f;
                        }
                    }
                }
            }
        }
    }
}
