using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;
using EntryPointSelector;

namespace hazelify.EntryPointSelector.Patches.PhysicsTriggers
{
    public class OnTriggerExitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), nameof(ExfiltrationPoint.OnTriggerExit));
        }
        // "IPhysicsTrigger.OnTriggerExit"
        [PatchPrefix]
        private static void PatchPrefix(Collider col)
        {
            if (Plugin.debug_exfildumper.Value || Plugin.debug_spawndumper.Value)
            {
                Plugin.logIssue("One or more debug options are enabled, disabling core patch modifications", false);
                Plugin.debug_exfildumper.Value = false;
                Plugin.debug_spawndumper.Value = false;
                return;
            }

            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.MainPlayer;
            if (player == null) return;
            var side = player.Side;
            if (side == EPlayerSide.Savage) return;

            Player playerByCollider = gameWorld.GetPlayerByCollider(col);

            if (Plugin.hasSpawned)
            {
                if (playerByCollider == gameWorld.MainPlayer)
                {
                    ExfiltrationControllerClass.Instance.BannedPlayers.Remove(playerByCollider.Id);
                    Plugin.hasSpawned = false;
                }
            }
        }
    }
}
