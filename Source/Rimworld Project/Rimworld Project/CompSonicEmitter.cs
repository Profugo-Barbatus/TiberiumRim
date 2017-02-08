using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class CompSonicEmitter : ThingComp
    {
        private CompPowerTrader powerComp;
        private CompProperties_SonicEmitter def;
        private List<IntVec3> cells = new List<IntVec3>();



        public override void PostSpawnSetup()
        {

            this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
            this.def = (CompProperties_SonicEmitter)this.props;

            if(def == null)
            {
                Debug.LogError("XML property failure at" + this.ToString());
            }
            cacheCells();
        }

        public override void CompTickRare()
        {
            TiberiumBase.Instance.logMessage("Ticking");
            if (parent.HitPoints/parent.def.BaseMaxHitPoints > def.damageShutdownPercent && this.powerComp != null && this.powerComp.PowerOn)
            {
                TiberiumBase.Instance.logMessage("Checking plants");
                checkPlantLife();
            }
        }

        public void cacheCells()
        {
            var rect = CellRect.CenteredOn(this.parent.Position, Mathf.RoundToInt(def.radius));
            rect.ClipInsideMap(this.parent.Map);

            for (int z = rect.minZ; z <= rect.maxZ; z++)
            {
                for (int x = rect.minX; x <= rect.maxX; x++)
                {
                    var c = new IntVec3(x, 0, z);

                    cells.Add(c);
                }
            }
        }

        public void checkPlantLife()
        {
            foreach(IntVec3 c in cells)
            {
                Plant plant = c.GetPlant(this.parent.Map);
                if (plant != null && plant.def.defName.Contains("Tiberium") && !plant.def.defName.Contains("TiberiumPlant"))
                {
                    if (GenSight.LineOfSight(this.parent.Position, c, this.parent.Map, true))
                    {
                        TiberiumBase.Instance.logMessage("This should kill the plant");
                        plant.Destroy(DestroyMode.Vanish);
                    }
                }
            }
            
            /*
            for (int z = rect.minZ; z <= rect.maxZ; z++)
            {
                for (int x = rect.minX; x <= rect.maxX; x++)
                {
                    var c = new IntVec3(x, 0, z);
                    
                    TiberiumBase.Instance.logMessage("We Can see the Cell");
                    Plant plant = c.GetPlant(this.parent.Map);
                    if (plant != null && plant.def.defName.Contains("Tiberium"))
                    {
                        if (GenSight.LineOfSight(this.parent.Position, c, this.parent.Map, true))
                        {
                            TiberiumBase.Instance.logMessage("This should kill the plant");
                            plant.Destroy(DestroyMode.Vanish);
                        }
                    }
                    
                }
            }
            */
        }
    }

    public class CompProperties_SonicEmitter : CompProperties
    {
        public int radius;
        public float damageShutdownPercent = 0.1f;
    }

    public class PlaceWorker_TiberiumInhibitor : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            int radius = def.GetCompProperties<CompProperties_SonicEmitter>().radius;
            List<IntVec3> cells = new List<IntVec3>();

            CellRect rect = CellRect.CenteredOn(center, Mathf.RoundToInt(radius));
            rect.ClipInsideMap(this.Map);
            for (int z = rect.minZ; z <= rect.maxZ; z++)
            {
                for (int x = rect.minX; x <= rect.maxX; x++)
                {
                    var c = new IntVec3(x, 0, z);
                    if (GenSight.LineOfSight(center, c, this.Map, true))
                    {
                        cells.Add(c);
                    }
                }
            }

            GenDraw.DrawFieldEdges(cells);
        }
    }
}
