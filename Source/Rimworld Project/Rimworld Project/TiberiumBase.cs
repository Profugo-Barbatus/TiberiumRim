using HugsLib;
using HugsLib.Settings;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    class TiberiumBase : ModBase
    {
        public static TiberiumBase Instance { get; private set; }

        public override string ModIdentifier
        {
            get
            {
                return "TiberiumRim";
            }
        }

        private TiberiumBase()
        {
            Instance = this;
        }

        public void logMessage(String message)
        {
            Logger.Message(message);
        }

        public void logError(String message)
        {
            Logger.Error(message);
        }

        public SettingHandle<bool> BuildingDamage;
        public SettingHandle<bool> TiberiumCompetes;
        public SettingHandle<bool> EntityDamage;

        public override void DefsLoaded()
        {
            BuildingDamage = Settings.GetHandle<bool>("BuildingDamage", "Tiberium_Damages_Structures".Translate(), "Tiberium_Damages_Structures_Desc".Translate(), true);
            TiberiumCompetes = Settings.GetHandle<bool>("TiberiumCompetes", "Tiberium_Competes".Translate(), "Tiberium_Competes_Desc".Translate(), false);
            EntityDamage = Settings.GetHandle<bool>("EntityDamage", "Tiberium_Damages_Items".Translate(), "Tiberium_Damages_Items_Desc".Translate(), true);
        }

        //ToxicFallout detour to save our crystals from dying.
        public override void Initialize()
        {
            MethodInfo source = typeof(MapCondition_ToxicFallout).GetMethod("DoCellSteadyEffects");
            MethodInfo destination = typeof(MapCondition_TibToxicFallout).GetMethod("_DoCellSteadyEffects");
            HugsLib.Source.Detour.DetourProvider.TryCompatibleDetour(source, destination);
            base.Initialize();
        }
    }
}
