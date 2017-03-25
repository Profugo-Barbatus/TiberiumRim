using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void CropBlighted()
        {

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

            //Fetch a reference to the ThingDef.
            TiberiumDef Localdef = this.def as TiberiumDef;

            corruptsTo = Localdef.corruptsInto;
            List<ThingDef> friendlyTo = Localdef.friendlyTo;

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

                //Reproduce
                if (def.plant.reproduces && growthInt >= SeedShootMinGrowthPercent)
                {
                    if (Rand.MTBEventOccurs(def.plant.reproduceMtbDays, GenDate.TicksPerDay, GenTicks.TickLongInterval))
                    {
                        if (!GenPlant.SnowAllowsPlanting(Position, Map))
                            return;

                        GenPlantReproduction.TryReproduceFrom(Position, def, SeedTargFindMode.Reproduce, Map, corruptsTo, friendlyTo);
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
                else if (growthInt > 0.5)
                {
                    infect(pawn2);
                }
            }
            if (TiberiumBase.Instance.BuildingDamage)
            {
                damageBuildings(Localdef.buildingDamage);
            }
            if (TiberiumBase.Instance.EntityDamage)
            {
                damageEntities(Localdef.entityDamage);
                damageChunks(Localdef.buildingDamage);
            }

            //Rare chance of monolithRise() to happen
            if (Rand.Chance(0.000001f))
            {
                if (this.def.defName.Contains("TiberiumBlue") && Rand.Chance(0.0000001f))
                {
                    MonolithRise(Map);
                }
                if (this.def.defName.Contains("TiberiumGreen"))
                {
                    MonolithRise(Map);
                }
            }

            //State has changed, label may have to as well
            //Also, we want to keep this null so we don't have useless data sitting there a long time in plants that never get looked at
            cachedLabelMouseover = null;
        }


        public virtual void infect(Pawn p)
        {
            HediffDef tiberium = DefDatabase<HediffDef>.GetNamed("TiberiumBuildupHediff", true);
            HediffDef addiction = DefDatabase<HediffDef>.GetNamed("TiberiumAddiction", true);

            if (p.RaceProps.IsMechanoid)
            {
                return;
            }

            if (p.def.defName.Contains("_TBI"))
            {
                return;
            }

            if (!p.health.hediffSet.HasHediff(addiction) && p.Position.InBounds(this.Map))
            {
                List<BodyPartRecord> list = new List<BodyPartRecord>();

                foreach (BodyPartRecord i in p.RaceProps.body.AllParts)
                {
                    if (i.depth == BodyPartDepth.Outside && !p.health.hediffSet.PartIsMissing(i))
                    {
                        list.Add(i);
                    }
                }
                bool search = true;

                BodyPartRecord target = null;

                while (search)
                {
                    //TiberiumBase.Instance.logMessage("Rerolling for target body part");
                    target = list.RandomElement();

                    if (target.height == BodyPartHeight.Bottom && Rand.Chance(0.8f))
                    {
                        //TiberiumBase.Instance.logMessage("Selected a body part with height of Bottom");
                        search = false;
                    }
                    else if (target.height == BodyPartHeight.Middle && Rand.Chance(0.5f))
                    {
                        //TiberiumBase.Instance.logMessage("Selected a body part with height of Middle");
                        search = false;
                    }
                    else if (target.height == BodyPartHeight.Top && Rand.Chance(0.2f))
                    {
                        //TiberiumBase.Instance.logMessage("Selected a body part with height of Top");
                        search = false;
                    }
                }

                List<BodyPartGroupDef> groups = target.groups;

                if (p.apparel == null)
                {
                    HealthUtility.AdjustSeverity(p, tiberium, +0.3f);
                    return;
                }

                List<Apparel> Clothing = p.apparel.WornApparel;

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
                                if (Clothing[j].def.defName.Contains("TBP"))
                                {
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

                if (Rand.Chance(protection * 1.8f))
                {
                    return;
                }
                HealthUtility.AdjustSeverity(p, tiberium, +0.1f);
            }
        }

        public void damageBuildings(int amt)
        {
            var c = this.RandomAdjacentCell8Way();
            if (c.InBounds(this.Map))
            {
                var p = c.GetFirstBuilding(this.Map);

                DamageInfo damage = new DamageInfo(DamageDefOf.Deterioration, amt);

                if (p != null)
                {
                    if (!p.def.defName.Contains("TBNS"))
                    {
                        if (Rand.Chance(0.1f))
                        {
                            corruptWall(p);
                            return;
                        }
                        p.TakeDamage(damage);
                    }
                }
                else
                {
                    //Dunno if the game explicitly treats doors different from buildings, but this'll cover just in case.
                    p = c.GetDoor(this.Map);
                    if (p != null)
                    {
                        p.TakeDamage(damage);
                    }
                }
            }
        }

        public void damageEntities(int amt)
        {
            var c = this.RandomAdjacentCell8Way();
            if (c.InBounds(this.Map))
            {
                var p = c.GetFirstItem(this.Map);

                DamageInfo damage = new DamageInfo(DamageDefOf.Deterioration, amt);

                if (p != null)
                {
                    if (p.def.IsCorpse)
                    {
                        Corpse body = (Corpse)p;
                        if (Rand.Chance(0.05f) && !body.InnerPawn.def.defName.Contains("_TBI"))
                        {
                            spawnFiendOrVisceroid(c, body.InnerPawn.def.race.body);
                            p.Destroy(DestroyMode.Vanish);
                            return;
                        }
                    }
                    p.TakeDamage(damage);
                }
            }
        }

        public void damageChunks(int amt)
        {
            var c = this.RandomAdjacentCell8Way();
            if (c.InBounds(this.Map))
            {
                var p = c.GetFirstHaulable(this.Map);

                DamageInfo damage = new DamageInfo(DamageDefOf.Deterioration, amt);

                if (p != null)
                {
                    p.TakeDamage(damage);
                }
            }
        }

        //Time to corrupt some walls
        public virtual void corruptWall(Building p)
        {
            if (p != null)
            {
                //Since we have multiple types of Tiberium, and multiple walls, we need to check which type is currently touching the wall
                ThingDef wall = null;
                switch (this.def.defName)
                {
                    case "TiberiumGreen":
                        wall = DefDatabase<ThingDef>.GetNamed("GreenTiberiumRock_TBNS", true);
                        break;

                    case "TiberiumBlue":
                        wall = DefDatabase<ThingDef>.GetNamed("BlueTiberiumRock_TBNS", true);
                        break;

                    case "TiberiumRed":
                        wall = DefDatabase<ThingDef>.GetNamed("RedTiberiumRock_TBNS", true);
                        break;

                    /* Gonna leave that out for now
                    case "TiberiumGreenDesert":
                        return;

                    case "TiberiumBlueDesert":
                        return;

                    case "TiberiumRedDesert":
                        return;
                    */

                    case "TiberiumGlacier":
                        return;
                } 

                IntVec3 loc = p.Position;

                //Only natural rocks should be infected, not constructed walls
                if (p.def.mineable && !p.def.defName.Contains("TBNS"))
                {
                    p.Destroy(DestroyMode.Vanish);
                    GenSpawn.Spawn(wall, loc, Map);
                }
            }
            return;
        }

        //A little variety mechanic, monolith now dependant on 9 blue crystals and a very rare chance, also a green harmless tiberium tower is created this way
        public void MonolithRise(Map map)
        {
            if (this.HarvestableNow && !this.def.defName.Contains("TiberiumRed") && !this.def.defName.Contains("TiberiumVein") && !this.def.defName.Contains("TiberiumGlacier") && !this.def.defName.Contains("Desert"))
            {
                bool check = false;
                foreach (IntVec3 intVec in GenAdjFast.AdjacentCells8Way(this.Position, this.Rotation, this.RotatedSize))
                {
                    if (intVec.InBounds(this.Map))
                    {
                        if (intVec.GetFirstThing(this.Map, this.def) != null && intVec.GetFirstThing(this.Map, this.def).def.plant.Harvestable)
                        {
                            check = true;
                        }
                    }
                }
                if (check)
                {
                    ThingDef tower = null;
                    switch (this.def.defName)
                    {
                        case "TiberiumGreen":
                            tower = DefDatabase<ThingDef>.GetNamed("TiberiumTower_TBNS", true);
                            break;

                        case "TiberiumBlue":
                            tower = DefDatabase<ThingDef>.GetNamed("TiberiumMonolith_TBNS", true);
                            break;
                    }
                    IntVec3 loc = this.Position;
                    GenSpawn.Spawn(tower, loc, map);
                    this.Destroy(DestroyMode.Vanish);
                    return;
                }
                return;
            }
            return;
        }

        public virtual void spawnFiendOrVisceroid(IntVec3 pos, BodyDef p)
        {
            Pawn pawn = null;
            if (Rand.Chance(0.25f))
            {
                //Unique organism based on bodytype
                PawnKindDef creature = null;

                switch (p.defName)
                {
                    case "QuadrupedAnimalWithHoovesAndHorn":
                        creature = DefDatabase<PawnKindDef>.GetNamed("TiberiumTerror_TBI", true);
                        break;

                    case "QuadrupedAnimalWithPaws":
                        creature = DefDatabase<PawnKindDef>.GetNamed("BigTiberiumFiend_TBI", true);
                        break;

                    case "BeetleLike":
                        creature = DefDatabase<PawnKindDef>.GetNamed("Tibscarab_TBI", true);
                        break;

                    case "QuadrupedAnimalWithPawsAndTail":
                        creature = DefDatabase<PawnKindDef>.GetNamed("TiberiumFiend_TBI", true);
                        break;

                    case "Snake":
                        creature = DefDatabase<PawnKindDef>.GetNamed("Crawler_TBI", true);
                        break;

                    default:
                        creature = DefDatabase<PawnKindDef>.GetNamed("Visceroid_TBI", true);
                        break;
                }

                PawnGenerationRequest request = new PawnGenerationRequest(creature);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            else
            {
                PawnKindDef Visceroid = DefDatabase<PawnKindDef>.GetNamed("Visceroid_TBI", true);
                PawnGenerationRequest request = new PawnGenerationRequest(Visceroid);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            GenSpawn.Spawn(pawn, pos, Map);
        }

        public override void Destroy(DestroyMode mode)
        {

            TiberiumDef Localdef = this.def as TiberiumDef;

            if (Localdef.isExplosive && mode == DestroyMode.Kill)
            {
                //Added a higher chance but made it a smaller radius for actual chainreactions 
                if (Rand.Chance(0.4f))
                {
                    Explode();
                }
            }
            base.Destroy(mode);
        }

        private void Explode()
        {
            GenExplosion.DoExplosion(this.Position, this.Map, 1.0f, DamageDefOf.Bomb, this);
        }
    }

    //Custom Version of the Reproduction Code
    public static class GenPlantReproduction
    {
        public static Plant TryReproduceFrom(IntVec3 source, ThingDef plantDef, SeedTargFindMode mode, Map map, TerrainDef setTerrain, List<ThingDef> friendlyTo)
        {
            IntVec3 dest;
            if (!TryFindReproductionDestination(source, plantDef, mode, map, friendlyTo, out dest))
                return null;

            return TryReproduceInto(dest, plantDef, map, setTerrain, friendlyTo);
        }

        public static Plant TryReproduceInto(IntVec3 dest, ThingDef plantDef, Map map, TerrainDef setTerrain, List<ThingDef> friendlyTo)
        {
            if (!plantDef.CanEverPlantAt(dest, map))
                return null;

            if (!GenPlant.SnowAllowsPlanting(dest, map))
                return null;

            var t = dest.GetTerrain(map);

            //Since we have glaciers we need a way to tell the game when to spawn them
            if (t.defName.Contains("Water") | t.defName.Contains("Marsh") && !t.defName.Contains("Marshy"))
            {
                if (plantDef.defName.Contains("TiberiumVein"))
                {
                    return null;
                }
                plantDef = ThingDef.Named("TiberiumGlacier");
                setTerrain = TerrainDef.Named("TiberiumWater");
            }
            else if (plantDef.defName.Contains("TiberiumGlacier"))
            {
                return null;
            }

            /* For balance purposes deserts shall not get covered in Tiberium soil
            if (t.defName.Contains("Sand") && !plantDef.defName.Contains("Desert") && Rand.Chance(0.3f))
            {
                switch (plantDef.defName)
                {
                    case "TiberiumGreen":
                        plantDef = ThingDef.Named("TiberiumGreenDesert");
                        setTerrain = TerrainDef.Named("GreenTiberiumSand");
                        break;

                    case "TiberiumBlue":
                        plantDef = ThingDef.Named("TiberiumBlueDesert");
                        setTerrain = TerrainDef.Named("BlueTiberiumSand");
                        break;

                    case "TiberiumRed":
                        plantDef = ThingDef.Named("TiberiumRedDesert");
                        setTerrain = TerrainDef.Named("RedTiberiumSand");
                        break;
                    case "TiberiumVein":
                        plantDef = ThingDef.Named("TiberiumVein");
                        setTerrain = TerrainDef.Named("RedTiberiumSand");
                        break;
                }
                changeTerrain(dest, map, setTerrain);
                return (Plant)GenSpawn.Spawn(plantDef, dest, map);
            }
            else if (t.defName.Contains("Sand"))
            {
                if (plantDef.defName.Contains("Desert"))
                {
                    changeTerrain(dest, map, setTerrain);
                    return (Plant)GenSpawn.Spawn(plantDef, dest, map);
                }
                return null;
            }
            else if (plantDef.defName.Contains("Desert"))
            {
                return null;
            }
            */

            if (Rand.Chance(0.8f))
            {
                //Log.Message("Spawn Tiberium: " + plantDef);
                changeTerrain(dest, map, setTerrain);

                return (Plant)GenSpawn.Spawn(plantDef, dest, map);
            }
            //Log.Message("You got unlucky! No tiberium spread, or is that lucky actually?");
            return null;
        }

        public static void changeTerrain(IntVec3 c, Map map, TerrainDef setTerrain)
        {
            if (c.InBounds(map))
            {
                TerrainDef targetTerrain = c.GetTerrain(map);
                map.terrainGrid.SetTerrain(c, setTerrain);
            }
        }

        public static bool TryFindReproductionDestination(IntVec3 source, ThingDef plantDef, SeedTargFindMode mode, Map map, List<ThingDef> friendlyTo, out IntVec3 foundCell)
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
                        //If Tiberium shouldn't compete, then we check against the list of Tiberium Crystal Varieties, and if the 
                        if (!TiberiumBase.Instance.TiberiumCompetes)
                        {
                            if (!friendlyTo.Contains(p.def))
                            {
                                if (Rand.Chance(0.05f))
                                {
                                    ThingDef flora = DefDatabase<ThingDef>.GetNamed("TiberiumPlant", true);
                                    IntVec3 loc = p.Position;

                                    p.Destroy(DestroyMode.Vanish);

                                    GenSpawn.Spawn(flora, loc, map);

                                }
                                else
                                {
                                    p.Destroy(DestroyMode.Vanish);
                                }
                            }
                        }
                        //Otherwise, regular behavior to avoid self-killing
                        else if (p.def != plantDef)
                        {

                            //Kill the plant. Later, we'll consider a piece of tiberium infected plantlife in its place.
                            if (Rand.Chance(0.05f))
                            {
                                ThingDef flora = DefDatabase<ThingDef>.GetNamed("TiberiumPlant", true);
                                IntVec3 loc = p.Position;

                                p.Destroy(DestroyMode.Vanish);

                                GenSpawn.Spawn(flora, loc, map);

                            }
                            else
                            {
                                p.Destroy(DestroyMode.Vanish);
                            }
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
            return CellFinder.TryFindRandomCellNear(source, map, Mathf.CeilToInt(radius), destValidator, out foundCell);
        }

    }

}

