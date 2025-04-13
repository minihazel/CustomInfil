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
        private static void PatchPostfix(ref GameWorld __instance)
        {
            if (!Plugin.debug_exfildumper.Value)
            {
                return;
            }

            if (__instance == null) return;
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.MainPlayer;
            var exfilController = __instance.ExfiltrationController;
            string mapFile = Path.Combine(Plugin.currentEnv, "BepInEx", "plugins", "hazelify.UnlockedEntries", "debug_exfils.json");
            string location = gameWorld.LocationId.ToString().ToLower();

            List<ExfiltrationPoint> points = [];
            points.AddRange(exfilController.ScavExfiltrationPoints);
            points.AddRange(exfilController.ExfiltrationPoints);
            points.AddRange(exfilController.SecretExfiltrationPoints);

            if (!File.Exists(mapFile))
            {
                File.WriteAllText(mapFile, "{}");
            }

            foreach (var foundExfil in points)
            {
                string _Name = foundExfil.Settings.Name.ToString().ToLower();
                string _Id = foundExfil.Settings.Id.ToString();
                List<string> currentMapExfils = new List<string>();

                string content = File.ReadAllText(mapFile);
                JObject data = JObject.Parse(content);

                JArray map = data[location] as JArray;
                if (map == null)
                {
                    data[location] = map as JArray;
                }
                string exfilItem = _Name;
                map.Add(exfilItem);
                File.WriteAllText(mapFile, data.ToString());
            }
        }
    }
}
