using HugsLib;
using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public SettingHandle<bool> BuildingDamage;
        public SettingHandle<bool> TiberiumCompetes;

        public override void DefsLoaded()
        {
            BuildingDamage = Settings.GetHandle<bool>("BuildingDamage", "Tiberium Damages Structures", "Determines if Tiberium will damage structures.", true);
            TiberiumCompetes = Settings.GetHandle<bool>("TiberiumCompetes", "Tiberium Competes", "Determines if Tiberium Crystals of Different Varieties will Destroy each other.", false);
        }
    }
}
