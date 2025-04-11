using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnlockedEntries;

namespace hazelify.UnlockedEntries.Patches
{
    public class SpawnpointDumper : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance)
        {
            if (!Plugin.debug_spawndumper.Value)
            {
                return;
            }

            if (__instance == null) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.MainPlayer;
            string location = gameWorld.LocationId.ToString().ToLower();

            if (!File.Exists(Plugin.spawnpointsFile))
            {
                Plugin.readSpawnpointsFile();
            }

            string content = File.ReadAllText(Plugin.spawnpointsFile);
            JObject contentObj = JObject.Parse(content);

            var _spawnPointMarkers = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<SpawnPointMarker>().ToList();

            JArray locArray = (JArray)contentObj[location];
            if (locArray == null)
            {
                Plugin.logIssue("SpawnpointDumper -> " + location + " does not exist in file, creating it", true);
                locArray = new JArray();
                contentObj[location] = (JArray)locArray as JArray;
                return;
            }

            foreach (var v in _spawnPointMarkers)
            {
                if (v == null) continue;
                int pointId = (int)v.SpawnPoint.CorePointId;
                string _Name = v.SpawnPoint.Name.ToString();
                float rotationX = v.SpawnPoint.Rotation.x;
                float rotationY = v.SpawnPoint.Rotation.y;
                float rotationZ = v.SpawnPoint.Rotation.z;
                float coordX = v.SpawnPoint.Position.x;
                float coordY = v.SpawnPoint.Position.y;
                float coordZ = v.SpawnPoint.Position.z;

                if (_Name.ToLower().Contains("boss"))
                {
                    ConsoleScreen.Log("SpawnpointDumper -> Spawn " + _Name + " is a boss spawnpoint, skipping");
                    continue;
                }

                JObject newObject = new JObject
                {
                    ["Name"] = _Name,
                    ["Id"] = pointId,
                    ["Rotation_X"] = rotationX,
                    ["Rotation_Y"] = rotationY,
                    ["Rotation_Z"] = rotationZ,
                    ["coord_X"] = coordX,
                    ["coord_Y"] = coordY,
                    ["coord_Z"] = coordZ
                };

                locArray.Add(newObject);
            }

            File.WriteAllText(Plugin.spawnpointsFile, contentObj.ToString());
        }
    }
}
