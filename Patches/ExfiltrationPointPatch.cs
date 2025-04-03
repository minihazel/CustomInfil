using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using CustomInfil;
using SPT.Reflection.Patching;
using Unity;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using hazelify.CustomInfil;
using System.IO;
using hazelify.CustomInfil.Patches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vector3 = UnityEngine.Vector3;
using static LocationSettingsClass;

namespace hazelify.CustomInfil.Patches
{
    public class ExfiltrationPointPatch : ModulePatch
    {
        public static bool isFinished = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance)
        {
            if (!Plugin.useLastExfil.Value) return;
            if (__instance == null) return;

            if (!Singleton<GameWorld>.Instantiated) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.MainPlayer;
            if (player == null) return;
            if (gameWorld.LocationId == null) return;

            string currentLoc = gameWorld.LocationId.ToString().ToLower();
            if (currentLoc == null) return;

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
        }
    }
}

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