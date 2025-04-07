using Comfort.Common;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Vector3 = UnityEngine.Vector3;

namespace hazelify.UnlockedEntries.Patches
{
    public class ShowIntroPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Spawn));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref LocalGame __instance)
        {
            if (__instance == null) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.RegisteredPlayers == null) return;

            var mainPlayer = __instance.GameWorld_0.MainPlayer;
            if (mainPlayer == null) return;

            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;

            Vector3 playerpos = new Vector3(x, y, z);
            mainPlayer.Teleport(playerpos, true);
        }
    }
}
