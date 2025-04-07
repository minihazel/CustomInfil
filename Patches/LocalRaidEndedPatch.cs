using Comfort.Common;
using UnlockedEntries;
using EFT;
using EFT.Interactive;
using EFT.UI.BattleTimer;
using HarmonyLib;
using hazelify.UnlockedEntries.Data;
using SPT.Reflection.Patching;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Timers;
using UnityEngine;
using UnityEngine.Pool;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
                Plugin.logIssue("GameWorld location is null", true);
                return;
            }

            string currentLoc = gameWorld.LocationId.ToString();
            Player player = gameWorld.MainPlayer;
            if (player == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> Player is null", true);
                return;
            }
            if (currentLoc == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentLoc is null", true);
                return;
            }

            float currentPlayerX = __instance.Position.x;
            float currentPlayerY = __instance.Position.y;
            float currentPlayerZ = __instance.Position.z;

            Vector3 currentPlayerPosition = new Vector3(currentPlayerX, currentPlayerY, currentPlayerZ);
            if (currentPlayerPosition == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerPosition is null", true);
                return;
            }

            float currentPlayerRotationX = __instance.Rotation.x;
            float currentPlayerRotationY = __instance.Rotation.y;

            Vector2 currentPlayerRotation = new Vector2(currentPlayerRotationX, currentPlayerRotationY);

            if (currentPlayerRotation == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerRotation is null", true);
                return;
            }

            PlayerData playerData = Plugin.playerManager.GetPlayerData(gameWorld.LocationId);
            if (playerData == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> playerData is null", true);
                return;
            }

            if (player.ActiveHealthController.IsAlive)
            {
                Plugin.playerManager.SetPlayerData(player.ProfileId, currentLoc, currentPlayerPosition, currentPlayerRotation);
                string successMessage = $"Set player data of {player.ProfileId} to position {currentPlayerPosition} and rotation {currentPlayerRotation} on map " + currentLoc;
                Plugin.logIssue(successMessage, false);
            }
        }
    }

    public class OnPlayerExit : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), "IPhysicsTrigger.OnTriggerExit");
        }

        [PatchPrefix]
        public static void PatchPrefix(Collider col)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
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
