using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    
    [HarmonyPatch(typeof(GameCondition_ToxicFallout), "DoCellSteadyEffects")]
    public static class ToxicFalloutPatch
    {
        [HarmonyPrefix]
        static bool PrefixMethod(GameCondition_ToxicFallout __instance, IntVec3 c)
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
            var things = room.ContainedAndAdjacentThings;
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
    }
}
