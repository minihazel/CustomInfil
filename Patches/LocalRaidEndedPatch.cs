using Comfort.Common;
using UnlockedEntries;
using EFT;
using HarmonyLib;
using hazelify.UnlockedEntries.Data;
using SPT.Reflection.Patching;
using System.Reflection;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using EFT.UI;

namespace hazelify.UnlockedEntries.Patches
{
    public class LocalRaidEndedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnDestroy));
        }
        
        [PatchPrefix]
        private static void PatchPrefix(ref Player __instance)
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
                Plugin.logIssue("LocalRaidEndedPatch -> `existingPlayerData` is null", false);
                return;
            }

            Player player = gameWorld.MainPlayer;

            if (player == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> Player is null", false);
                player = __instance;

                if (player == null)
                {
                    Plugin.logIssue("LocalRaidEndedPatch -> Player (AllAlivePlayersList) is null", false);
                    return;
                }
            }

            if (currentLoc == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentLoc is null", false);
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
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerPosition is null", false);
                return;
            }

            float currentPlayerRotationX = player.Rotation.x;
            float currentPlayerRotationY = player.Rotation.y;

            Vector2 currentPlayerRotation = new Vector2(currentPlayerRotationX, currentPlayerRotationY);

            if (currentPlayerRotation == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerRotation is null", false);
                return;
            }


            if (player.ActiveHealthController.IsAlive)
            {
                string successMessage = $"Set player data of {player.ProfileId} to position {currentPlayerPosition} and rotation {currentPlayerRotation} on map " + currentLoc;

                if (!Plugin.playerManager.DoesPlayerDataExist(currentLoc))
                    Plugin.logIssue("LocalRaidEndedPatch -> `PlayerData` has no entry for location " + currentLoc + ", creating one", false);

                if (!currentLoc.ToLower().StartsWith("factory4"))
                {
                    Plugin.playerManager.SetPlayerData(currentLoc, currentPlayerPosition, currentPlayerRotation);
                }
                else
                {
                    Plugin.playerManager.SetPlayerData("factory4_day", currentPlayerPosition, currentPlayerRotation);
                    Plugin.playerManager.SetPlayerData("factory4_night", currentPlayerPosition, currentPlayerRotation);
                }

                Plugin.logIssue(successMessage, false);
            }

            Plugin.hasSpawned = true;
        }
    }
}
