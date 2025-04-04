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

namespace hazelify.CustomInfil.Patches
{
    public class RaidStartPatch : ModulePatch
    {
        public static bool isFinished = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance)
        {
            // source checks
            if (!Plugin.useLastExfil.Value) return;
            if (__instance == null) return;

            // inverted if checks for ensuring reliability
            if (!Singleton<GameWorld>.Instantiated) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld.LocationId == null) return;
            string currentLoc = gameWorld.LocationId.ToString().ToLower();
            if (currentLoc == null) return;
            Player player = gameWorld.MainPlayer;
            if (player == null) return;

            // actual code
            if (currentLoc == "factory4_night")
            {
                double closestDistance = double.MaxValue;
                JObject closestSpawn = null;

                for (int i = 0; i < Plugin.spawnpointsObj.Count; i++)
                {
                    JObject spawn = Plugin.spawnpointsObj[currentLoc][i] as JObject;
                    string selectedExfil = Plugin.Factory_Exfils.Value.ToString();

                    float exfil_pos_x = (float)Plugin.allExfils["factory4_day"]["exfils"][i]["X"];
                    float exfil_pos_y = (float)Plugin.allExfils["factory4_day"]["exfils"][i]["Y"];
                    float exfil_pos_z = (float)Plugin.allExfils["factory4_day"]["exfils"][i]["Z"];

                    float spawnpoint_pos_x = (float)Plugin.spawnpointsObj["factory4_day"][i]["coord_X"];
                    float spawnpoint_pos_y = (float)Plugin.spawnpointsObj["factory4_day"][i]["coord_Y"];
                    float spawnpoint_pos_z = (float)Plugin.spawnpointsObj["factory4_day"][i]["coord_Z"];

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
            }
            else
            {
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
            }
        }
    }
}

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

// currently used
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