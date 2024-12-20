using CookingOverhaul.Content.Items.Hoe;
using CookingOverhaul.NetCode;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CookingOverhaul
{
	public class CookingOverhaul : Mod
	{
		public static CookingOverhaul Instance { get; private set; }

        #region Hooks
        public override void Load()
        {
            Instance = this;
        }

        public override void Unload()
        {
            Instance = null;
        }
        #endregion
    }
}
