using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{

    //Extended Plant Class. Tiberium Grows, is Harvested, and Spreads like a plant, so branching off of this will be the easiest technique.
    public class TiberiumCrystal : Plant
    {
        //Tiberium is a Crystal, so it doesn't freeze to death
        private const float minGrowthTemperature = -300;


        public TerrainDef corruptsTo = null;
        //Tiberium never rests
        protected override bool Resting
        {
            get
            {
                return false;
            }
        }

        //Tiberium should always grow at 100% capability
        public override float GrowthRate
        {
            get
            {
                return 1;
            }
        }

        //Internal list of anything living that decides to get too close to the crystals.
        private List<Pawn> touchingPawns = new List<Pawn>();

        public static new float MinGrowthTemperature
        {
            get
            {
                return minGrowthTemperature;
            }
        }

        public object cachedLabelMouseover { get; private set; }
        public int DyingDamagePerTick { get; private set; }


        //Overriding the TickLong to respect the fact that Tiberium doesn't properly behave like a plant.
        public override void TickLong()
        {
            /* Tiberium Cannot become Leafless. Lets save the effort on the check.
            CheckTemperatureMakeLeafless();
            */
            if (Destroyed)
                return;

            //Previously, checked if it was growing season. Tiberium doesn't have a restricted growth season.
            if (true)
            {
                bool hasLight = HasEnoughLightToGrow;

                //Record light starvation
                if (!hasLight)
                    unlitTicks += GenTicks.TickLongInterval;
                else
                    unlitTicks = 0;

                //Grow
                float prevGrowth = growthInt;
                bool wasMature = LifeStage == PlantLifeStage.Mature;
                growthInt += GrowthPerTick * GenTicks.TickLongInterval;

                if (growthInt > 1f)
                    growthInt = 1f;

                //Regenerate layers
                if ((!wasMature && LifeStage == PlantLifeStage.Mature)
                    || (int)(prevGrowth * 10f) != (int)(growthInt * 10f))
                {
                    if (CurrentlyCultivated())
                        Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
                }

                if (def.plant.LimitedLifespan)
                {
                    //Age
                    ageInt += GenTicks.TickLongInterval;

                    //Dying
                    if (Dying)
                    {
                        var map = Map;
                        bool isCrop = IsCrop; // before applying damage!

                        int dyingDamAmount = Mathf.CeilToInt(DyingDamagePerTick * GenTicks.TickLongInterval);
                        TakeDamage(new DamageInfo(DamageDefOf.Rotting, dyingDamAmount));

                        if (Destroyed)
                        {
                            if (isCrop && def.plant.Harvestable && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + def.defName, 240f))
                                Messages.Message("MessagePlantDiedOfRot".Translate(Label).CapitalizeFirst(), new TargetInfo(Position, map), MessageSound.Negative);

                            return;
                        }
                    }
                }

                //Murder. Doing this on the longtick should give a bit of an element of randomness to crossing a tiberium field. Its not instantly infectious, anyhow.
                //Build a list of every item occupying the same space as the tiberium.
                List<Thing> thingList = base.Position.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    //Going through the list. If the item is a pawn, then it won't null out in the latter check, and will be able to be added to the list if its not already in it.
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null && !this.touchingPawns.Contains(pawn))
                    {
                        this.touchingPawns.Add(pawn);
                        //Small Tiberium Growths won't infect, to prevent micro'd pawns from getting infected by newly spawned formations
                        if (growthInt > 0.5)
                        {
                            infect(pawn);
                        }
                    }
                }

                for (int j = 0; j < this.touchingPawns.Count; j++)
                {
                    Pawn pawn2 = this.touchingPawns[j];
                    if (!pawn2.Spawned || pawn2.Position != base.Position)
                    {
                        this.touchingPawns.Remove(pawn2);
                    }
                }

                //Reproduce
                if (def.plant.reproduces && growthInt >= SeedShootMinGrowthPercent)
                {
                    if (Rand.MTBEventOccurs(def.plant.reproduceMtbDays, GenDate.TicksPerDay, GenTicks.TickLongInterval))
                    {
                        if (!GenPlant.SnowAllowsPlanting(Position, Map))
                            return;

                        //Fetch a reference to the ThingDef.
                        TiberiumDef Localdef = this.def as TiberiumDef;

                        corruptsTo = DefDatabase<TerrainDef>.GetNamed(Localdef.corruptsInto, true);

                        GenPlantReproduction.TryReproduceFrom(Position, def, SeedTargFindMode.Reproduce, Map, corruptsTo);
                    }
                }

            }

            //State has changed, label may have to as well
            //Also, we want to keep this null so we don't have useless data sitting there a long time in plants that never get looked at
            cachedLabelMouseover = null;
        }

        public void infect(Pawn p)
        {
            HediffDef tiberium = DefDatabase<HediffDef>.GetNamed("TiberiumContactPoison", true);

            if (!p.health.hediffSet.HasHediff(tiberium) || !p.RaceProps.IsMechanoid)
            {
                List<BodyPartRecord> list = new List<BodyPartRecord>();

                foreach (BodyPartRecord i in p.RaceProps.body.AllParts)
                {
                    if (i.depth == BodyPartDepth.Outside)
                    {
                        list.Add(i);
                    }
                }

                BodyPartRecord target = list.RandomElement();

                List<BodyPartGroupDef> groups = target.groups;

                List<Apparel> Clothing = p.apparel.WornApparel;

                if(Clothing.CountAllowNull() == null)
                {

                }

                float protection = 0;

                for (int j = 0; j < Clothing.Count; j++)
                {
                    List<BodyPartGroupDef> covered = Clothing[j].def.apparel.bodyPartGroups;

                    if (covered.Count > 0)
                    {
                        for (int k = 0; k < covered.Count; k++)
                        {
                            if (groups.Contains(covered[k]))
                            {
                                if(Clothing[j].def.defName.Contains("TBP"))
                                {
                                    Log.Message("Prevented an infection due to special clothing");
                                    return;
                                }
                                if (protection < Clothing[j].GetStatValue(DefDatabase<StatDef>.GetNamed("ArmorRating_Sharp")))
                                {
                                    protection = Clothing[j].GetStatValue(DefDatabase<StatDef>.GetNamed("ArmorRating_Sharp"));
                                }
                            }
                        }
                    }
                }
                
                if(Rand.Chance(protection * 1.8f) )
                {
                    Log.Message("Prevented an infection due to armor rating");
                    return;
                }

                Log.Message("Failed to Prevent an Infection with an Armor Rating of " + protection);
                p.health.AddHediff(tiberium, target, null);
            }
        }
        
    }

    //Custom Version of the Reproduction Code
    public static class GenPlantReproduction
    {
        public static Plant TryReproduceFrom(IntVec3 source, ThingDef plantDef, SeedTargFindMode mode, Map map, TerrainDef setTerrain)
        {
            IntVec3 dest;
            if (!TryFindReproductionDestination(source, plantDef, mode, map, out dest))
                return null;

            return TryReproduceInto(dest, plantDef, map, setTerrain);
        }

        public static Plant TryReproduceInto(IntVec3 dest, ThingDef plantDef, Map map, TerrainDef setTerrain)
        {
            if (!plantDef.CanEverPlantAt(dest, map))
                return null;

            if (!GenPlant.SnowAllowsPlanting(dest, map))
                return null;

            changeTerrain(dest, map, setTerrain);

            return (Plant)GenSpawn.Spawn(plantDef, dest, map);
        }

        public static void changeTerrain(IntVec3 c, Map map, TerrainDef setTerrain)
        {
            TerrainDef targetTerrain = c.GetTerrain(map);

            map.terrainGrid.SetTerrain(c, setTerrain);
        }

        public static bool TryFindReproductionDestination(IntVec3 source, ThingDef plantDef, SeedTargFindMode mode, Map map, out IntVec3 foundCell)
        {
            float radius = -1;
            if (mode == SeedTargFindMode.Reproduce)
                radius = plantDef.plant.reproduceRadius;
            else if (mode == SeedTargFindMode.MapGenCluster)
                radius = plantDef.plant.WildClusterRadiusActual;
            else if (mode == SeedTargFindMode.MapEdge)
                radius = 40;

            //Now this code originally dealt with a sanity check. But we don't really need that.
            //What we want to do is try and kill/replace some plants instead. So lets cannibalize it, and use it to our own nefarious deeds.
            var rect = CellRect.CenteredOn(source, Mathf.RoundToInt(radius));
            rect.ClipInsideMap(map);
            //Basically this'll just go around all the tiles in the radius we've picked.
            for (int z = rect.minZ; z <= rect.maxZ; z++)
            {
                for (int x = rect.minX; x <= rect.maxX; x++)
                {
                    var c = new IntVec3(x, 0, z);
                    var p = c.GetPlant(map);
                    //If we do find a plant here, lets mess around a bit.
                    if (p != null)
                    {
                        //If the plant is different from us, lets do something.
                        //This does mean tiberium fields of different crystal types will engage in competition. 
                        if (p.def != plantDef)
                        {
                            //Kill the plant. Later, we'll consider a piece of tiberium infected plantlife in its place.
                            p.Destroy(DestroyMode.Kill);
                        }
                    }
                }
            }

            //From here on in, we are going to make Tiberium, but ignore *all* the sanity checking we might otherwise want

            /*float numDesiredPlants = totalFertility * map.Biome.plantDensity;
            bool full = numFoundPlants > numDesiredPlants;
            bool overloaded = numFoundPlants > numDesiredPlants * 1.25f;

            if (overloaded)
            {
                foundCell = IntVec3.Invalid;
                return false;
            }

            //Determine num desired plants of my def
            var curBiome = map.Biome;
            float totalCommonality = curBiome.AllWildPlants.Sum(pd => curBiome.CommonalityOfPlant(pd));
            float minProportion = curBiome.CommonalityOfPlant(plantDef) / totalCommonality;
            float maxProportion = (curBiome.CommonalityOfPlant(plantDef) * plantDef.plant.wildCommonalityMaxFraction) / totalCommonality;

            //Too many plants of my type nearby - don't reproduce
            float maxDesiredPlantsMyDef = numDesiredPlants * maxProportion;
            if (numFoundPlantsMyDef > maxDesiredPlantsMyDef)
            {
                foundCell = IntVec3.Invalid;
                return false;
            }

            //Too many plants nearby for the biome/total fertility - don't reproduce
            //UNLESS there are way too few of my kind of plant
            float minDesiredPlantsMyDef = numDesiredPlants * minProportion;
            bool desperateForPlantsMyDef = numFoundPlantsMyDef < minDesiredPlantsMyDef * 0.5f;
            if (full && !desperateForPlantsMyDef)
            {
                foundCell = IntVec3.Invalid;
                return false;
            }*/

            //We need to plant something
            //Try find a cell to plant into
            Predicate<IntVec3> destValidator = c =>
            {
                if (!plantDef.CanEverPlantAt(c, map))
                    return false;

                if (!GenPlant.SnowAllowsPlanting(c, map))
                    return false;

                if (!source.InHorDistOf(c, radius))
                    return false;

                if (!GenSight.LineOfSight(source, c, map, skipFirstCell: true))
                    return false;

                return true;
            };
            return CellFinder.TryFindRandomCellNear(source,
                                                map,
                                                Mathf.CeilToInt(radius),
                                                destValidator,
                                                out foundCell);
        }
    }
}
