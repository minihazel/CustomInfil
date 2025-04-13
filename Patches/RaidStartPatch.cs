using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using UnlockedEntries;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Vector3 = UnityEngine.Vector3;
using static LocationSettingsClass;
using EFT.Game.Spawning;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Numerics;
using System.Collections.Generic;
using BepInEx.Configuration;
using hazelify.UnlockedEntries.Data;
using System.Globalization;

namespace hazelify.UnlockedEntries.Patches
{
    public class RaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref GameWorld __instance)
        {
            bool areCoordinatesEmpty = false;
            if (Plugin.debug_exfildumper.Value || Plugin.debug_spawndumper.Value)
            {
                Plugin.logIssue("One or more debug options are enabled, disabling core patch modifications", false);
                Plugin.debug_exfildumper.Value = false;
                Plugin.debug_spawndumper.Value = false;
                return;
            }

            if (__instance == null) return;
            Player player = __instance.MainPlayer;
            GameWorld gameWorld = Singleton<GameWorld>.Instance;

            if (player == null) return;

            if (__instance.LocationId == null)
            {
                Plugin.logIssue("GameWorld location is null", false);
                return;
            }

            string selectedExfil = null;
            string translatedInternalSelectedExfil = null;
            string currentLoc = __instance.LocationId.ToString().ToLower();
            var exfilController = __instance.ExfiltrationController;
            var side = player.Side;

            PlayerData existingPlayerData = Plugin.playerManager.GetPlayerData(currentLoc);

            if (existingPlayerData == null)
            {
                Plugin.logIssue("LocalRaidEndedPatch -> `existingPlayerData` is null, creating an entry for player " + player.Profile.Nickname.ToString(), false);

                Vector3 currentPlayerPosition = new Vector3(0, 0, 0);
                Vector2 currentPlayerRotation = new Vector2(0, 0);

                string successMessage = $"Profile Id did not exist, but entry did; saving profile Id into file for all locations";

                if (!currentLoc.StartsWith("factory4"))
                {
                    Plugin.playerManager.SetPlayerData(player.ProfileId, currentLoc, currentPlayerPosition, currentPlayerRotation);
                }
                else
                {
                    Plugin.playerManager.SetPlayerData(player.ProfileId, "factory4_day", currentPlayerPosition, currentPlayerRotation);
                    Plugin.playerManager.SetPlayerData(player.ProfileId, "factory4_night", currentPlayerPosition, currentPlayerRotation);
                }

                Plugin.logIssue(successMessage, false);
                areCoordinatesEmpty = true;
            }

            if (Plugin.useLastExfil.Value && !areCoordinatesEmpty)
            {
                if (Plugin.isLITInstalled)
                {
                    return;
                }

                float existingPosX = existingPlayerData.Position_X;
                float existingPosY = existingPlayerData.Position_Y;
                float existingPosZ = existingPlayerData.Position_Z;

                float existingRotX = existingPlayerData.Rotation_X;
                float existingRotY = existingPlayerData.Rotation_Y;

                Vector3 existingPos = new Vector3(existingPosX, existingPosY, existingPosZ);
                Vector2 existingRot = new Vector2(existingRotX, existingRotY);

                if (existingPos == null)
                {
                    Plugin.logIssue("RaidStartPatch -> PlayerData position is null", false);
                    return;
                }

                if (existingRot == null)
                {
                    Plugin.logIssue("RaidStartPatch -> PlayerData rotation is null", false);
                    return;
                }

                Plugin.hasSpawned = true;
                ExfiltrationControllerClass.Instance.BannedPlayers.Add(player.Id);

                player.Rotation = existingRot;
                player.Teleport(existingPos, true);
                Plugin.logIssue("Teleported player to last known position: " + existingPos.ToString() + " with rotation " + existingRot.ToString(), false);
            };
            if (!Plugin.chooseInfil.Value)
            {
                Plugin.logIssue("RaidStartPatch -> `Choose Infil` disabled, skipping", false);
                return;
            }

            switch (currentLoc)
            {
                case "factory4_day":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Factory_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Factory_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "factory4_night":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Factory_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Factory_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "bigmap":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Customs_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Customs_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "sandbox":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.GZ_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.GZ_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "sandbox_high":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.GZ_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.GZ_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "rezervbase":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Reserve_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Reserve_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "lighthouse":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Lighthouse_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Lighthouse_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "shoreline":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Shoreline_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Shoreline_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "woods":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Woods_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Woods_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "interchange":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Interchange_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Interchange_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "tarkovstreets":
                    switch (player.Side)
                    {
                        case EPlayerSide.Savage:
                            selectedExfil = Plugin.Streets_Exfils_Scavs.Value.ToString();
                            break;
                        default:
                            selectedExfil = Plugin.Streets_Exfils.Value.ToString();
                            break;
                    }
                    break;
                case "laboratory":
                    selectedExfil = Plugin.Labs_Exfils.Value.ToString();
                    break;
            }

            // CODE ACTUALLY STARTS HERE

            switch (player.Side)
            {
                case EPlayerSide.Savage:
                    translatedInternalSelectedExfil = ExfilLookup.GetInternalName(currentLoc + "_scav", selectedExfil).ToLower();
                    break;
                default:
                    translatedInternalSelectedExfil = ExfilLookup.GetInternalName(currentLoc, selectedExfil).ToLower();
                    break;
            }

            List<ExfiltrationPoint> points = [];
            List<SpawnpointsData> currentMap = Plugin.spawnDataDictionary[currentLoc];

            JObject closestSpawn = null;

            string detectedExfilName = null;
            Vector3 currentExfilPosition = Vector3.zero;
            Vector3 spawnpoint_vec = Vector3.zero;
            float closestDistance = float.MaxValue;

            switch (player.Side)
            {
                case EPlayerSide.Savage:
                    points.AddRange(exfilController.ExfiltrationPoints);
                    points.AddRange(exfilController.ScavExfiltrationPoints);
                    break;
                default:
                    points.AddRange(exfilController.ExfiltrationPoints);
                    points.AddRange(exfilController.SecretExfiltrationPoints);
                    break;
            }

            foreach (var foundExfil in points)
            {
                string _Name = foundExfil.Settings.Name.ToString().ToLower();
                string _Id = foundExfil.Settings.Id.ToString();
                List<string> currentMapExfils = new List<string>();

                switch (player.Side)
                {
                    case EPlayerSide.Savage:
                        currentMapExfils = Plugin.GetExfilList(currentLoc + "_scav");
                        break;
                    default:
                        currentMapExfils = Plugin.GetExfilList(currentLoc);
                        break;
                }

                if (currentMapExfils == null)
                {
                    Plugin.logIssue("[UnlockedEntries] `currentMapExfils` was null", false);
                    return;
                }

                if (currentMapExfils.Contains(_Name) && translatedInternalSelectedExfil == _Name)
                {
                    float _X = foundExfil.transform.position.x;
                    float _Y = foundExfil.transform.position.y;
                    float _Z = foundExfil.transform.position.z;

                    currentExfilPosition = new Vector3(_X, _Y, _Z);
                    detectedExfilName = _Name.ToString();
                    break;
                }
            }

            foreach (var spawnPoint in currentMap)
            {
                string spawnName = (string)spawnPoint.Name;

                spawnpoint_vec = new Vector3(
                    (float)spawnPoint.coord_X,
                    (float)spawnPoint.coord_Y,
                    (float)spawnPoint.coord_Z);

                float distance = Vector3.Distance(currentExfilPosition, spawnpoint_vec);

                JObject newSpawnpoint = new JObject
                {
                    ["Name"] = spawnName,
                    ["Id"] = spawnPoint.Id,
                    ["Rotation_X"] = spawnPoint.Rotation_X,
                    ["Rotation_Y"] = spawnPoint.Rotation_Y,
                    ["Rotation_Z"] = spawnPoint.Rotation_Z,
                    ["coord_X"] = spawnPoint.coord_X,
                    ["coord_Y"] = spawnPoint.coord_Y,
                    ["coord_Z"] = spawnPoint.coord_Z
                };

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSpawn = newSpawnpoint;
                }
            }

            if (closestSpawn == null)
            {
                Plugin.logIssue("[UnlockedEntries] `ClosestSpawn` JObject was null", false);
                return;
            }

            Vector3 coords = new Vector3(
                (float)closestSpawn["coord_X"],
                (float)closestSpawn["coord_Y"],
                (float)closestSpawn["coord_Z"]);

            Vector2 rotation = new Vector2(
                (float)closestSpawn["Rotation_X"],
                (float)closestSpawn["Rotation_Z"]);

            if (coords == null)
            {
                Plugin.logIssue("[UnlockedEntries] Closest spawn coordinates were null", false);
                return;
            }
            if (rotation == null)
            {
                Plugin.logIssue("[UnlockedEntries] Closest spawn rotation was null", false);
                return;
            }

            string currentExfilCoords =
                " X: " + currentExfilPosition.x.ToString() +
                " Y: " + currentExfilPosition.y.ToString() +
                " Z: " + currentExfilPosition.z.ToString();

            try
            {
                player.Rotation = rotation;
            }
            catch (Exception ex)
            {
                Plugin.logIssue("[UnlockedEntries] Player Rotation error: " + ex.Message.ToString(), false);
            }

            try
            {
                Plugin.hasSpawned = true;
                player.Teleport(coords, true);
            }
            catch (Exception ex)
            {
                Plugin.logIssue("[UnlockedEntries] Player Teleport error: " + ex.Message.ToString(), false);
            }
        }
    }
}
