using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using CustomInfil;
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
using hazelify.CustomInfil.Data;

namespace hazelify.CustomInfil.Patches
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
                Plugin.logIssue("GameWorld location is null", true);
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
                    float existingPosX = existingPlayerData.Position_X;
                    float existingPosY = existingPlayerData.Position_Y;
                    float existingPosZ = existingPlayerData.Position_Z;

                    float existingRotX = existingPlayerData.Rotation_X;
                    float existingRotY = existingPlayerData.Rotation_Y;

                    Vector3 existingPos = new Vector3(existingPosX, existingPosY, existingPosZ);
                    Vector2 existingRot = new Vector2(existingRotX, existingRotY);

                    if (existingPos == null)
                    {
                        Plugin.logIssue("RaidStartPatch -> PlayerData position is null", true);
                        return;
                    }

                    if (existingRot == null)
                    {
                        Plugin.logIssue("RaidStartPatch -> PlayerData rotation is null", true);
                        return;
                    }

                    Plugin.hasSpawned = true;
                    ExfiltrationControllerClass.Instance.BannedPlayers.Add(player.Id);

                    player.Teleport(existingPos, true);
                    player.Rotate(existingRot);
                    Plugin.logIssue("[CustomInfil] Teleported player to last known position: " + existingPos.ToString() + " with rotation " + existingRot.ToString(), false);
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
                Plugin.logIssue("[CustomInfil] `ClosestSpawn` JObject was null", true);
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
                Plugin.logIssue("[CustomInfil] Closest spawn coordinates were null", true);
                return;
            }
            if (rotation == null)
            {
                Plugin.logIssue("[CustomInfil] Closest spawn rotation was null", true);
                return;
            }

            string currentExfilCoords =
                " X: " + currentExfilPosition.x.ToString() +
                " Y: " +currentExfilPosition.y.ToString() +
                " Z: " + currentExfilPosition.z.ToString();

            try
            {
                player.Teleport(coords, true);
                Plugin.logIssue("[CustomInfil] Teleported player successfully to: " + currentExfilCoords + " (" + detectedExfilName + ")", false);
            }
            catch (Exception ex)
            {
                Plugin.logIssue("[CustomInfil] Player Teleport error: " + ex.Message.ToString(), true);
            }

            try
            {
                player.Rotation = rotation;
                Plugin.logIssue("[CustomInfil] Rotated player successfully to: " + rotation.ToString(), false);
            }
            catch (Exception ex)
            {
                Plugin.logIssue("[CustomInfil] Player Rotation error: " + ex.Message.ToString(), true);
            }
        }
    }
}

// math
/*
double closestDistance = double.MaxValue;
JObject closestSpawn = null;

for (int i = 0; i < Plugin.spawnpointsObj.Count; i++)
{
    JObject spawn = Plugin.spawnpointsObj[currentLoc][i] as JObject;
    string selectedExfil = Plugin.Factory_Exfils.Value.ToString();

    float exfil_pos_x = (float)Plugin.allExfils[currentLoc]["exfils"][i]["X"];
    float exfil_pos_y = (float)Plugin.allExfils[currentLoc]["exfils"][i]["Y"];
    float exfil_pos_z = (float)Plugin.allExfils[currentLoc]["exfils"][i]["Z"];

    float spawnpoint_pos_x = (float)Plugin.spawnpointsObj[currentLoc][i]["coord_X"];
    float spawnpoint_pos_y = (float)Plugin.spawnpointsObj[currentLoc][i]["coord_Y"];
    float spawnpoint_pos_z = (float)Plugin.spawnpointsObj[currentLoc][i]["coord_Z"];

    double distance = Math.Sqrt(
        Math.Pow(exfil_pos_x - spawnpoint_pos_x, 2) +
        Math.Pow(exfil_pos_y - spawnpoint_pos_y, 2) +
        Math.Pow(exfil_pos_z - spawnpoint_pos_z, 2)
    );

    if (distance < closestDistance)
    {
        closestDistance = distance;
        closestSpawn = spawn;
    }
}

if (closestSpawn == null) return;

Vector3 coords = new Vector3(
    (float)closestSpawn["coord_X"],
    (float)closestSpawn["coord_Y"],
    (float)closestSpawn["coord_Z"]);

Vector2 rotation = new Vector2(
    (float)closestSpawn["Rotation_X"],
    (float)closestSpawn["Rotation_Z"]);

if (coords == null) return;
if (rotation == null) return;

try
{
    player.Teleport(coords, true);
}
catch (Exception ex)
{
    ConsoleScreen.Log("[CustomInfil] Player Teleport error: " + ex.Message.ToString());
}

try
{
    player.Rotation = rotation;
}
catch (Exception ex)
{
    ConsoleScreen.Log("[CustomInfil] Player Rotation error: " + ex.Message.ToString());
}
*/

/*
JArray currentSpawnpoints = (JArray)Plugin.spawnpointsObj[currentLoc];
if (currentSpawnpoints.Count > 0)
    ConsoleScreen.Log("[CustomInfil] Skipping location " + currentLoc + ": spawnpoints are already logged");

List<SpawnPointMarker> _spawnPointMarkers = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<SpawnPointMarker>().ToList();
if (_spawnPointMarkers == null)
{
    ConsoleScreen.Log("Spawnpoints for location " + currentLoc + " were null");
}

for (int i = 0; i < _spawnPointMarkers.Count; i++)
{
    var v = _spawnPointMarkers[i];

    if (v.SpawnPoint.Sides.Contain(EPlayerSide.Usec) ||
        v.SpawnPoint.Sides.Contain(EPlayerSide.Bear) &&
        v.SpawnPoint.Categories == ESpawnCategoryMask.Player)
    {
        // useful info
        string infiltrationPoint = (string)v.SpawnPoint.Infiltration.ToString();
        if (string.IsNullOrEmpty(infiltrationPoint))
        {
            infiltrationPoint = "Error";
        };

        string spawnPointName = (string)v.SpawnPoint.Name.ToString();
        if (string.IsNullOrEmpty(spawnPointName))
        {
            spawnPointName = "Error";
        };
        int pointId = (int)v.SpawnPoint.CorePointId;
        if (pointId == null)
        {
            pointId = 404;
        }

        // position
        Vector3 pos = (Vector3)v.SpawnPoint.Position;
        if (pos == null)
        {
            pos = new Vector3(0, 0, 0);
        }

        // rotation
        Quaternion rot = (Quaternion)v.SpawnPoint.Rotation;
        if (rot == null) return;

        JObject newSpawnPoint = new JObject
        {
            ["Name"] = spawnPointName,
            ["Id"] = pointId,
            ["Infiltration"] = infiltrationPoint,
            ["Rotation_X"] = rot.x,
            ["Rotation_Y"] = rot.y,
            ["Rotation_Z"] = rot.z,
            ["coord_X"] = pos.x,
            ["coord_Y"] = pos.y,
            ["coord_Z"] = pos.z
        };

        currentSpawnpoints.Add(newSpawnPoint);

        File.WriteAllText(Plugin.spawnpointsFile, Plugin.spawnpointsObj.ToString());
    }
}

ConsoleScreen.Log("[CustomInfil] Logged " + _spawnPointMarkers.Count.ToString() + " spawnpoints to file of location: " + currentLoc);
*/

/*
if (string.IsNullOrEmpty(Plugin.usedExfil.Value.ToString())) return;

JArray current_exfils = (JArray)Plugin.allExfils[currentLoc]["exfils"];
foreach (JObject xfil in current_exfils)
{
    string _Name = xfil["Name"].ToString().ToLower();
    if (_Name == Plugin.usedExfil.Value.ToLower())
    {
        float _X = (float)xfil["X"];
        float _Y = (float)xfil["Y"];
        float _Z = (float)xfil["Z"];

        Vector3 newPos = new Vector3(_X, _Y, _Z);

        if (newPos == null) return;
        player.Teleport(newPos, true);
        break;
    }
}
*/

// currently used
/*
ExfiltrationPoint[] exfils = player.Side == EPlayerSide.Savage
    ? gameWorld.ExfiltrationController.ScavExfiltrationPoints
    : gameWorld.ExfiltrationController.ExfiltrationPoints;

ExfiltrationPoint[] pmcExfils = gameWorld.ExfiltrationController.ExfiltrationPoints;

if (pmcExfils == null) return;

for (int i = 0; i < pmcExfils.Length; i++)
{
    string json = File.ReadAllText(Plugin.mapFile);
    JObject data = JObject.Parse(json);

    string _Name = pmcExfils[i].Settings.Name.ToString();
    string _Id = pmcExfils[i].Id.ToString();
    float _X = pmcExfils[i].transform.position.x;
    float _Y = pmcExfils[i].transform.position.y;
    float _Z = pmcExfils[i].transform.position.z;

    JObject map = (JObject)data[location] as JObject;

    JArray mapexfils = (JArray)map["exfils"] as JArray;

    JObject newExfil = new JObject
    {
        ["Name"] = _Name,
        ["Id"] = _Id,
        ["X"] = _X,
        ["Y"] = _Y,
        ["Z"] = _Z
    };

    mapexfils.Add(newExfil);

    File.WriteAllText(Plugin.mapFile, data.ToString());
    isFinished = true;
}

if (isFinished)
    ConsoleScreen.Log("[LastInfil] Map " + location + " saved to exfils.json");
*/

/*
    float closestDistance = float.MaxValue;
    Logger.LogInfo("3");
    ConsoleScreen.Log(selectedExfil);
    Logger.LogInfo(selectedExfil);
    JArray currentMap = (JArray)Plugin.spawnpointsObj[currentLoc];
    Logger.LogInfo("4");

    JObject closestSpawn = null;

    Vector3 exfil_vec = Vector3.zero;
    Vector3 spawnpoint_vec = Vector3.zero;

    foreach (JObject exfil in available_exfils)
    {
        Logger.LogInfo("8");
        if ((string)exfil["Name"] == selectedExfil)
        {
            exfil_vec = new Vector3(
                (float)exfil["X"],
                (float)exfil["Y"],
                (float)exfil["Z"]);
            Logger.LogInfo("X " + (string)exfil["X"] +
                            "Y" + (string)exfil["Y"] +
                            "Z" + (string)exfil["Z"]);
            ConsoleScreen.Log("X " + (string)exfil["X"] +
                            "Y" + (string)exfil["Y"] +
                            "Z" + (string)exfil["Z"]);
        }
        Logger.LogInfo("9");
        break;
    }

    Logger.LogInfo("10");
    foreach (JObject spawnPoint in currentMap)
    {
        Logger.LogInfo("11");
        string spawnName = (string)spawnPoint["Name"];

        spawnpoint_vec = new Vector3(
            (float)spawnPoint["coord_X"],
            (float)spawnPoint["coord_Y"],
            (float)spawnPoint["coord_Z"]);
        Logger.LogInfo("X " + (float)spawnPoint["coord_X"] +
                            "Y" + (float)spawnPoint["coord_Y"] +
                            "Z" + (float)spawnPoint["coord_Z"]);

        ConsoleScreen.Log("X " + (float)spawnPoint["coord_X"] +
                            "Y" + (float)spawnPoint["coord_Y"] +
                            "Z" + (float)spawnPoint["coord_Z"]);

        Logger.LogInfo("12");
        float distance = Vector3.Distance(exfil_vec, spawnpoint_vec);
        // distance from exfil center to spawnpoint
                
        if (distance < closestDistance)
        {
            // if distance from exfil center to spawnpoint is shorter
            closestDistance = distance;
            closestSpawn = spawnPoint;
        }
        else if (distance == closestDistance)
        {
            // if distance from exfil to spawnpoint is the same as previously
            closestSpawn = spawnPoint;
        }
    }

    Logger.LogInfo("13");
    if (closestSpawn != null)
    {
        Logger.LogInfo("15");
        Logger.LogInfo(closestSpawn.ToString());
        Vector3 coords = new Vector3(
            (float)closestSpawn["coord_X"],
            (float)closestSpawn["coord_Y"],
            (float)closestSpawn["coord_Z"]);
        Logger.LogInfo("16");

        Vector2 rotation = new Vector2(
            (float)closestSpawn["Rotation_X"],
            (float)closestSpawn["Rotation_Z"]);
        Logger.LogInfo("17");

        if (coords == null) return;
        Logger.LogInfo("18");
        if (rotation == null) return;
        Logger.LogInfo("19");

        try
        {
            Logger.LogInfo("20");
            player.Teleport(coords, true);
            ConsoleScreen.Log("[CustomInfil] Teleported player successfully to: " + selectedExfil);
            Logger.LogInfo("21");
        }
        catch (Exception ex)
        {
            ConsoleScreen.Log("[CustomInfil] Player Teleport error: " + ex.Message.ToString());
            Logger.LogError("[CustomInfil] Player Teleport error: " + ex.Message.ToString());
        }

        try
        {
            Logger.LogInfo("22");
            player.Rotation = rotation;
            ConsoleScreen.Log("[CustomInfil] Rotated player successfully to: " + rotation.ToString());
            Logger.LogInfo("23");
        }
        catch (Exception ex)
        {
            ConsoleScreen.Log("[CustomInfil] Player Rotation error: " + ex.Message.ToString());
            Logger.LogError("[CustomInfil] Player Rotation error: " + ex.Message.ToString());
        }
    }
    else
    {
        Logger.LogInfo("14");
        ConsoleScreen.Log("`closestSpawn` variable is null, aborting");
        Logger.LogError("`closestSpawn` variable is null, aborting");
        return;
    }
*/