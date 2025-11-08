using EFT.Interactive;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using EntryPointSelector;
using Comfort.Common;

namespace hazelify.EntryPointSelector.Patches.PhysicsTriggers
{
    public class OnTriggerEnterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), "IPhysicsTrigger.OnTriggerEnter");
        }
        // "IPhysicsTrigger.OnTriggerEnter"
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
                    ExfiltrationControllerClass.Instance.BannedPlayers.Add(playerByCollider.Id);
                }
            }
            else
            {
                if (playerByCollider == gameWorld.MainPlayer)
                {
                    if (ExfiltrationControllerClass.Instance.BannedPlayers.Contains(playerByCollider.Id))
                    {
                        ExfiltrationControllerClass.Instance.BannedPlayers.Remove(playerByCollider.Id);
                    }
                }
            }
        }
    }
}
