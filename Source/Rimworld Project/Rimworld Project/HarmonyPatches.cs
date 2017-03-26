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
            if (c.InBounds(map))
            {
                List<Thing> thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.defName.Contains("Tiberium"))
                    {
                        Log.Message("Dont kill tiberium plz");
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
            foreach (Thing thing in room.AllContainedThings)
            {
                if (thing != null)
                {
                    if (thing.def.defName.Contains("Tiberium") || thing.def.mineable)
                    {
                        Log.Message("Found Tiberium stuff, not making room");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
