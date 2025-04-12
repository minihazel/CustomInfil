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

            string pId = existingPlayerData.ProfileId;
            foreach (Player registeredPlayer in gameWorld.RegisteredPlayers)
            {
                if (registeredPlayer.ProfileId == pId && !registeredPlayer.Profile.Nickname.ToLower().StartsWith("headless_"))
                {
                    Player player = registeredPlayer;

                    if (player == null)
                    {
                        Plugin.logIssue("LocalRaidEndedPatch -> Player is null", false);
                        return;
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
                            Plugin.playerManager.SetPlayerData(player.ProfileId, currentLoc, currentPlayerPosition, currentPlayerRotation);
                        }
                        else
                        {
                            Plugin.playerManager.SetPlayerData(player.ProfileId, "factory4_day", currentPlayerPosition, currentPlayerRotation);
                            Plugin.playerManager.SetPlayerData(player.ProfileId, "factory4_night", currentPlayerPosition, currentPlayerRotation);
                        }

                        Plugin.logIssue(successMessage, false);
                    }
                    break;
                }
            }
        }
    }

    public class postDestroyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnDestroy));
        }

        [PatchPostfix]
        private void PatchPostfix(ref Player __instance)
        {
            if (__instance == null) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            if (gameWorld.LocationId == null)
            {
                Plugin.logIssue("GameWorld location is null", false);
                return;
            }

            string currentLoc = gameWorld.LocationId.ToString();
            Player player = __instance;
            if (player == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> Player is null", false);
                return;
            }
            if (currentLoc == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> currentLoc is null", false);
                return;
            }

            var side = player.Side;
            if (side == EPlayerSide.Savage) return;

            Plugin.hasSpawned = true;
        }
    }

    public class OnPlayerExit : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), "IPhysicsTrigger.OnTriggerExit");
        }

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

    public class OnPlayerEnter : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), "IPhysicsTrigger.OnTriggerEnter");
        }

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
        }
    }
}
