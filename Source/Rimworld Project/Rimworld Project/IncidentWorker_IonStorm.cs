using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    /*
     * MakeMapCondition and MapConditions don't exist anymore. Requires reevaluation. This also seems needlessly complex for deciding *if* we will fire the event, and dependent on rares.
     * Conclusion: Reanalyze purpose of incident, balance dev cost and running cost of decisions against gameplay gains.
     */
    class IncidentWorker_IonStorm : IncidentWorker_MakeGameCondition
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
                int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
                GameCondition cond = GameConditionMaker.MakeCondition(this.def.gameCondition, duration, 1);
                GameCondition cond2 = GameConditionMaker.MakeCondition(GameConditionDefOf.SolarFlare, duration, 1);
                map.gameConditionManager.RegisterCondition(cond);
                map.gameConditionManager.RegisterCondition(cond2);
                map.weatherManager.TransitionTo(WeatherDef.Named("IonStormWeather"));
                base.SendStandardLetter();
                return true;
            }
            Log.Message("Can't execute Ion Storm with " + count + " green crystals;- " + count2 + " blue crystals;- " + count3 + " red crystals.");
            return false;
        }

    }
}
