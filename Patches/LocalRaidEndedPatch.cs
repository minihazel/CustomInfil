using Comfort.Common;
using CustomInfil;
using EFT;
using EFT.Interactive;
using EFT.UI.BattleTimer;
using HarmonyLib;
using hazelify.CustomInfil.Data;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;
using System.Timers;
using UnityEngine.Pool;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace hazelify.CustomInfil.Patches
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
            Plugin.hasSpawned = true;
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

            float currentPlayerX = (float)__instance.gameObject.transform.position.x;
            float currentPlayerY = (float)__instance.gameObject.transform.position.y;
            float currentPlayerZ = (float)__instance.gameObject.transform.position.z;

            Vector3 currentPlayerPosition = new Vector3(currentPlayerX, currentPlayerY, currentPlayerZ);
            if (currentPlayerPosition == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerPosition is null", true);
                return;
            }

            float currentPlayerRotationX = (float)__instance.gameObject.transform.rotation.x;
            float currentPlayerRotationY = (float)__instance.gameObject.transform.rotation.y;

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

    public class OnPlayerEnter : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), nameof(ExfiltrationPoint.OnPlayerEnter));
        }

        [PatchPostfix]
        public static void PatchPostfix(ref Player __instance)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            if (Plugin.hasSpawned)
            {
                foreach (ExfiltrationPoint point in LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<ExfiltrationPoint>().ToArray<ExfiltrationPoint>())
                {
                    if (!(point == null))
                    {
                        point.DisableInteraction();
                    }
                }
                Plugin.hasSpawned = false;
            }
            else
            {
                foreach (ExfiltrationPoint point in LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<ExfiltrationPoint>().ToArray<ExfiltrationPoint>())
                {
                    if (!(point == null))
                    {
                        point.EnableInteraction();
                    }
                }
            }
        }
    }
}
