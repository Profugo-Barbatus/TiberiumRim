using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    class IncidentWorker_IonStorm : IncidentWorker_MakeMapCondition
    {

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int count = map.listerThings.ThingsOfDef(ThingDef.Named("TiberiumGreen")).Count;
            int count2 = map.listerThings.ThingsOfDef(ThingDef.Named("TiberiumBlue")).Count;
            int count3 = map.listerThings.ThingsOfDef(ThingDef.Named("TiberiumRed")).Count;

            /*
            int countD = map.listerThings.ThingsOfDef(ThingDef.Named("TiberiumGreenDesert")).Count;
            int count2D = map.listerThings.ThingsOfDef(ThingDef.Named("TiberiumBlueDesert")).Count;
            int count3D = map.listerThings.ThingsOfDef(ThingDef.Named("TiberiumRedDesert")).Count;
            */

            Log.Message("Trying to execute the Ion Storm");
            if (count > 400 && count2 > 200 && count3 > 50)
            {
                Log.Message("Executing Ion Storm with " + count + " green crystals;- " + count2 + " blue crystals;- " + count3 + " red crystals.");
                int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
                MapCondition cond = MapConditionMaker.MakeCondition(this.def.mapCondition, duration, 1);
                MapCondition cond2 = MapConditionMaker.MakeCondition(MapConditionDefOf.SolarFlare, duration, 1);
                Log.Message("Ion STorm Ticks: N/A" + " | Weather Ticks " + 90000);
                map.mapConditionManager.RegisterCondition(cond);
                map.mapConditionManager.RegisterCondition(cond2);
                map.weatherManager.TransitionTo(WeatherDef.Named("IonStormWeather"));
                base.SendStandardLetter();
                return true;
            }
            Log.Message("Can't execute Ion Storm with " + count + " green crystals;- " + count2 + " blue crystals;- " + count3 + " red crystals.");
            return false;
        }

    }
}
