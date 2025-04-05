using Comfort.Common;
using CustomInfil;
using EFT;
using HarmonyLib;
using hazelify.CustomInfil.Data;
using SPT.Reflection.Patching;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace hazelify.CustomInfil.Patches
{
    public class LocalRaidEndedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Class303), nameof(Class303.LocalRaidEnded));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance)
        {
            if (!Plugin.useLastExfil.Value) return;
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

            float currentPlayerX = (float)player.Position.x;
            float currentPlayerY = (float)player.Position.y;
            float currentPlayerZ = (float)player.Position.z;

            Vector3 currentPlayerPosition = new Vector3(currentPlayerX, currentPlayerY, currentPlayerZ);
            if (currentPlayerPosition == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerPosition is null", true);
                return;
            } 

            float currentPlayerRotationX = (float)player.Rotation.x;
            float currentPlayerRotationY = (float)player.Rotation.y;

            Vector2 currentPlayerRotation = new Vector2(currentPlayerRotationX, currentPlayerRotationY);
            if (currentPlayerRotation == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentPlayerRotation is null", true);
                return;
            }

            PlayerData playerData = Plugin.playerManager.GetPlayerData(player.ProfileId, gameWorld.LocationId);
            if (playerData == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> playerData is null", true);
                return;
            }

            Plugin.playerManager.SetPlayerData(player.ProfileId, currentLoc, currentPlayerPosition, currentPlayerRotation);
            string successMessage = $"Set player data of {player.ProfileId} to position {currentPlayerPosition} and rotation {currentPlayerRotation} on map " + currentLoc;
            Plugin.logIssue(successMessage, false);
        }
    }
}
