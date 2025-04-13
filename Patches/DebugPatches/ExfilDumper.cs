using Comfort.Common;
using UnlockedEntries;
using EFT;
using EFT.Interactive;
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
using EFT.Game.Spawning;
using System.IO.Compression;

namespace hazelify.UnlockedEntries.Patches.DebugPatches
{
    public class ExfilDumper : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance)
        {
            if (!Plugin.debug_exfildumper.Value)
            {
                return;
            }

            if (__instance == null) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.MainPlayer;
            string mapFile = Path.Combine(Plugin.currentEnv, "BepInEx", "plugins", "hazelify.UnlockedEntries", "debug_exfils.json");
            string location = gameWorld.LocationId.ToString().ToLower();

            ExfiltrationPoint[] exfils = gameWorld.ExfiltrationController.ScavExfiltrationPoints;
            ExfiltrationPoint[] pmcexfils = gameWorld.ExfiltrationController.ExfiltrationPoints;
            ExfiltrationPoint[] tryExfils = gameWorld.ExfiltrationController.SecretExfiltrationPoints;

            ExfiltrationPoint[] mergeTwo = pmcexfils.Concat(tryExfils).ToArray();
            ExfiltrationPoint[] mergeSecrets = mergeTwo.Concat(tryExfils).ToArray();

            if (!File.Exists(mapFile))
            {
                File.WriteAllText(mapFile, "{}");
            }

            if (exfils == null)
            {
                ConsoleScreen.Log("ExfilDumper -> `exfils` was null");
                return;
            }

            if (pmcexfils == null)
            {
                ConsoleScreen.Log("ExfilDumper -> `pmcexfils` was null");
                return;
            }

            if (mergeTwo == null)
            {
                ConsoleScreen.Log("ExfilDumper -> `mergeTwo` was null");
                return;
            }

            if (mergeSecrets == null)
            {
                ConsoleScreen.Log("ExfilDumper -> `mergeSecrets` was null");
                return;
            }

            for (int i = 0; i < mergeSecrets.Length; i++)
            {
                string _Name = mergeSecrets[i].Settings.Name.ToString();

                string content = File.ReadAllText(mapFile);
                JObject data = JObject.Parse(content);
                JArray map = data[location] as JArray;
                if (map == null)
                {
                    map = new JArray();
                    data[location] = map as JArray;
                }
                string exfilItem = _Name;

                map.Add(exfilItem);
                File.WriteAllText(mapFile, data.ToString());
            }
        }
    }
}
