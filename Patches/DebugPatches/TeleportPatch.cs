using Comfort.Common;
using EFT;
using HarmonyLib;
using UnlockedEntries;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace hazelify.UnlockedEntries.Patches.DebugPatches
{
    public class TeleportPatch : ModulePatch
    {
        public GameWorld gameWorld { get; set; }
        public Player _player = null;
        public Player Player
        {
            get
            {
                if (_player == null)
                {
                    _player = gameWorld.MainPlayer;
                }
                return _player;
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref Player ___instance)
        {
            if (!Plugin.useLastExfil.Value) return;
            if (!Singleton<GameWorld>.Instantiated) return;


        }
    }
}
