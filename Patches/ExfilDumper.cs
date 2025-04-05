using Comfort.Common;
using CustomInfil;
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

namespace hazelify.CustomInfil.Patches
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
            if (__instance == null) return;
            var gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.MainPlayer;
            string mapFile = Path.Combine(Plugin.currentEnv, "BepInEx", "plugins", "hazelify.CustomInfil", "scavexfils.json");
            string location = gameWorld.LocationId.ToString().ToLower();

            ExfiltrationPoint[] exfils = gameWorld.ExfiltrationController.ScavExfiltrationPoints;

            if (exfils == null)
            {
                ConsoleScreen.Log("Null exfil");
            }

            for (int i = 0; i < exfils.Length; i++)
            {
                string _Name = exfils[i].Settings.Name.ToString();

                string json = File.ReadAllText(mapFile);
                JObject data = JObject.Parse(json);
                JArray map = (JArray)data[location] as JArray;
                string exfilItem = _Name;

                map.Add(exfilItem);
                File.WriteAllText(mapFile, data.ToString());
            }
        }
    }
}
