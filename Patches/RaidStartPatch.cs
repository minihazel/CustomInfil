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
            if (__instance == null) return;
            Player player = __instance.MainPlayer;
            if (player == null) return;

            if (__instance.LocationId == null)
            {
                Plugin.logIssue("GameWorld location is null", false);
                return;
            }

            int currentIndex = 0;
            int pmcIndex = 0;
            int scavIndex = 0;

            string selectedExfil = null;
            string translatedInternalSelectedExfil = null;
            string currentLoc = __instance.LocationId.ToString().ToLower();
            var exfilController = __instance.ExfiltrationController;
            var side = player.Side;

            if (!Plugin.chooseInfil.Value && Plugin.useLastExfil.Value)
            {
                if (Plugin.playerManager.DoesPlayerDataExist(currentLoc))
                {
                    PlayerData existingPlayerData = Plugin.playerManager.GetPlayerData(currentLoc);

                    string pId = existingPlayerData.ProfileId;
                    if (pId != player.ProfileId)
                    {
                        Plugin.logIssue("RaidStartPatch -> PlayerData profile ID does not match current player ID. This is NOT an error. Use an EXFIL zone to update your player data!", false);
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

                    player.Teleport(existingPos, true);
                    player.Rotation = existingRot;
                    Plugin.logIssue("Teleported player to last known position: " + existingPos.ToString() + " with rotation " + existingRot.ToString(), false);
                }
                return;
            };

            switch (currentLoc)
            {
                case "factory4_day":
                    selectedExfil = Plugin.Factory_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.factory4_day.IndexOf(selectedExfil);
                    pmcIndex  = ExfilDescData.factory4_day.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.factory4_day.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Factory_Exfils.Value = ExfilDescData.factory4_day[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Factory_Exfils.Value = ExfilDescData.factory4_day[pmcIndex + 1];
                    }
                    break;
                case "factory4_night":
                    selectedExfil = Plugin.Factory_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.factory4_night.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.factory4_night.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.factory4_night.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Factory_Exfils.Value = ExfilDescData.factory4_night[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Factory_Exfils.Value = ExfilDescData.factory4_night[pmcIndex + 1];
                    }
                    break;
                case "bigmap":
                    selectedExfil = Plugin.Customs_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.bigmap.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.bigmap.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.bigmap.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Customs_Exfils.Value = ExfilDescData.bigmap[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Customs_Exfils.Value = ExfilDescData.bigmap[pmcIndex + 1];
                    }
                    break;
                case "sandbox":
                    selectedExfil = Plugin.GZ_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.sandbox.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.sandbox.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.sandbox.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.GZ_Exfils.Value = ExfilDescData.sandbox[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.GZ_Exfils.Value = ExfilDescData.sandbox[pmcIndex + 1];
                    }
                    break;
                case "rezervbase":
                    selectedExfil = Plugin.Reserve_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.rezervbase.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.rezervbase.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.rezervbase.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Reserve_Exfils.Value = ExfilDescData.rezervbase[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Reserve_Exfils.Value = ExfilDescData.rezervbase[pmcIndex + 1];
                    }
                    break;
                case "lighthouse":
                    selectedExfil = Plugin.Lighthouse_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.lighthouse.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.lighthouse.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.lighthouse.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Lighthouse_Exfils.Value = ExfilDescData.lighthouse[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Lighthouse_Exfils.Value = ExfilDescData.lighthouse[pmcIndex + 1];
                    }
                    break;
                case "shoreline":
                    selectedExfil = Plugin.Shoreline_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.shoreline.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.shoreline.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.shoreline.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Shoreline_Exfils.Value = ExfilDescData.shoreline[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Shoreline_Exfils.Value = ExfilDescData.shoreline[pmcIndex + 1];
                    }
                    break;
                case "woods":
                    selectedExfil = Plugin.Woods_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.woods.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.woods.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.woods.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Woods_Exfils.Value = ExfilDescData.woods[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Woods_Exfils.Value = ExfilDescData.woods[pmcIndex + 1];
                    }
                    break;
                case "interchange":
                    selectedExfil = Plugin.Interchange_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.interchange.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.interchange.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.interchange.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Interchange_Exfils.Value = ExfilDescData.interchange[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Interchange_Exfils.Value = ExfilDescData.interchange[pmcIndex + 1];
                    }
                    break;
                case "tarkovstreets":
                    selectedExfil = Plugin.Streets_Exfils.Value.ToString();
                    currentIndex = ExfilDescData.tarkovstreets.IndexOf(selectedExfil);
                    pmcIndex = ExfilDescData.tarkovstreets.IndexOf("↓ PMC EXFILS ↓");
                    scavIndex = ExfilDescData.tarkovstreets.IndexOf("↓ PMC SCAV ↓");

                    if (side == EPlayerSide.Savage && currentIndex < scavIndex)
                    {
                        Plugin.Streets_Exfils.Value = ExfilDescData.tarkovstreets[scavIndex + 1];
                    }
                    else if (side == EPlayerSide.Bear || side == EPlayerSide.Usec && currentIndex > scavIndex)
                    {
                        Plugin.Streets_Exfils.Value = ExfilDescData.tarkovstreets[pmcIndex + 1];
                    }
                    break;
                case "laboratory":
                    selectedExfil = Plugin.Labs_Exfils.Value.ToString();
                    break;
            }



            // CODE ACTUALLY STARTS HERE
            translatedInternalSelectedExfil = ExfilLookup.GetInternalName(currentLoc, selectedExfil);
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
                    break;
            }

            foreach (var foundExfil in points)
            {
                string _Name = foundExfil.Settings.Name.ToString();
                string _Id = foundExfil.Settings.Id.ToString();

                List<string> currentMapExfils = Plugin.GetExfilList(currentLoc);
                if (currentMapExfils.Contains(_Name))
                {
                    if (string.IsNullOrEmpty(translatedInternalSelectedExfil)) return;
                    if (_Name == translatedInternalSelectedExfil)
                    {
                        float _X = foundExfil.transform.position.x;
                        float _Y = foundExfil.transform.position.y;
                        float _Z = foundExfil.transform.position.z;

                        currentExfilPosition = new Vector3(_X, _Y, _Z);
                        detectedExfilName = _Name.ToString();
                        break;
                    }
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
                    ["Infiltration"] = spawnPoint.Infiltration,
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
                " Y: " +currentExfilPosition.y.ToString() +
                " Z: " + currentExfilPosition.z.ToString();

            try
            {
                player.Teleport(coords, true);
            }
            catch (Exception ex)
            {
                Plugin.logIssue("[UnlockedEntries] Player Teleport error: " + ex.Message.ToString(), false);
            }

            try
            {
                player.Rotation = rotation;
            }
            catch (Exception ex)
            {
                Plugin.logIssue("[UnlockedEntries] Player Rotation error: " + ex.Message.ToString(), false);
            }
        }
    }
}
