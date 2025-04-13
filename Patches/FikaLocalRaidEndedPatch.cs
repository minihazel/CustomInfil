using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Fika.Core.Coop.GameMode;
using hazelify.UnlockedEntries.Data;
using UnlockedEntries;
using UnityEngine;
using Comfort.Common;
using Fika.Core.Coop.Players;

namespace hazelify.UnlockedEntries.Patches
{
    public class FikaLocalRaidEndedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CoopGame), nameof(CoopGame.Extract));
        }

        [PatchPrefix]
        private void PatchPrefix(ref CoopPlayer __instance)
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
            string pId = existingPlayerData.ProfileId;

            if (player == null)
            {
                Plugin.logIssue("FikaLocalRaidEndedPatch -> Player is null", false);
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

                if (!currentLoc.ToLower().StartsWith("factory4"))
                {
                    Plugin.playerManager.SetPlayerData(pId, currentLoc, currentPlayerPosition, currentPlayerRotation);
                }
                else
                {
                    Plugin.playerManager.SetPlayerData(pId, "factory4_day", currentPlayerPosition, currentPlayerRotation);
                    Plugin.playerManager.SetPlayerData(pId, "factory4_night", currentPlayerPosition, currentPlayerRotation);
                }

                Plugin.logIssue(successMessage, false);
            }
        }
    }
}
