using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /*
     * Harmony patches need to be updated with new Hugslib/Harmony combined integration
     * 
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("TiberiumRim");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(MapCondition_ToxicFallout))]
    [HarmonyPatch("DoCellSteadyEffects")]
    [HarmonyPatch(new Type[] { typeof(IntVec3) })]
    class ToxicFalloutPatch
    {
        [HarmonyPrefix]
        static bool PrefixMethod(MapCondition_ToxicFallout __instance, IntVec3 c)
        {
            var map = Traverse.Create(__instance).Property("Map").GetValue<Map>();
            if (c.InBounds(map) && !c.Roofed(map))
            {
                List<Thing> thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.defName.Contains("Tiberium"))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(AutoBuildRoofAreaSetter))]
    [HarmonyPatch("TryGenerateAreaNow")]
    [HarmonyPatch(new Type[] { typeof(Room) })]
    class AutoBuildroofAreaPatch
    {
        [HarmonyPrefix]
        static bool PrefixMethod(Room room)
        {
            var things = room.AllContainedThings;
            var count = things.Count;
            foreach (Thing thing in things)
            {
                if (thing != null)
                {
                    if (thing.def.defName.Contains("Tiberium") || count <= 9)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }*/
}
