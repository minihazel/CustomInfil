using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using hazelify.EntryPointSelector.Data;
using EntryPointSelector;
using UnityEngine;
using Comfort.Common;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Players;

namespace hazelify.EntryPointSelector.Patches
{
    public class FikaLocalRaidEndedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CoopGame), nameof(CoopGame.Extract));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref FikaPlayer __instance)
        {
            if (__instance == null) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            if (gameWorld.LocationId == null)
            {
                Plugin.logIssue("GameWorld location is null", false);
                return;
            }

            string currentLoc = gameWorld.LocationId.ToString().ToLower();
            PlayerData existingPlayerData = Plugin.playerManager.GetPlayerData(currentLoc);

            if (existingPlayerData == null)
            {
                Plugin.logIssue("FikaLocalRaidEndedPatch -> `existingPlayerData` is null", false);
                return;
            }

            Player player = gameWorld.MainPlayer;

            if (player == null)
            {
                Plugin.logIssue("FikaLocalRaidEndedPatch -> Player is null", false);
                player = __instance;

                if (player == null)
                {
                    Plugin.logIssue("FikaLocalRaidEndedPatch -> Player (__instance) is null", false);
                    return;
                }
                return;
            }
            if (currentLoc == null)
            {
                Plugin.logIssue("FikaLocalRaidEndedPatch -> currentLoc is null", false);
                return;
            }

            var side = player.Side;
            if (side == EPlayerSide.Savage) return;

            float currentPlayerX = player.Position.x;
            float currentPlayerY = player.Position.y;
            float currentPlayerZ = player.Position.z;

            Vector3 currentPlayerPosition = new Vector3(currentPlayerX, currentPlayerY, currentPlayerZ);
            if (currentPlayerPosition == null)
            {
                Plugin.logIssue("FikaLocalRaidEndedPatch -> currentPlayerPosition is null", false);
                return;
            }

            float currentPlayerRotationX = player.Rotation.x;
            float currentPlayerRotationY = player.Rotation.y;

            Vector2 currentPlayerRotation = new Vector2(currentPlayerRotationX, currentPlayerRotationY);

            if (currentPlayerRotation == null)
            {
                Plugin.logIssue("FikaLocalRaidEndedPatch -> currentPlayerRotation is null", false);
                return;
            }


            if (player.ActiveHealthController.IsAlive)
            {
                string successMessage = $"Set player data of {player.ProfileId} to position {currentPlayerPosition} and rotation {currentPlayerRotation} on map " + currentLoc;

                if (!Plugin.playerManager.DoesPlayerDataExist(currentLoc))
                    Plugin.logIssue("FikaLocalRaidEndedPatch -> `PlayerData` has no entry for location " + currentLoc + ", creating one", false);

                if (currentLoc.ToLower().StartsWith("factory4"))
                {
                    Plugin.playerManager.SetPlayerData("factory4_day", currentPlayerPosition, currentPlayerRotation);
                    Plugin.playerManager.SetPlayerData("factory4_night", currentPlayerPosition, currentPlayerRotation);
                }
                else if (currentLoc.ToLower().StartsWith("sandbox"))
                {
                    Plugin.playerManager.SetPlayerData("sandbox", currentPlayerPosition, currentPlayerRotation);
                    Plugin.playerManager.SetPlayerData("sandbox_high", currentPlayerPosition, currentPlayerRotation);
                }
                else
                {
                    Plugin.playerManager.SetPlayerData(currentLoc, currentPlayerPosition, currentPlayerRotation);
                }

                Plugin.logIssue(successMessage, false);
            }
        }
    }
}
