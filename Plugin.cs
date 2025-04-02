using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using hazelify.CustomInfil.Patches;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using static EFT.SpeedTree.TreeWind;

namespace CustomInfil;

[BepInPlugin("hazelify.custominfil", "Last Infil", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static string currentEnv = Environment.CurrentDirectory; // main SPT dir

    public static string mapFile = null;
    public static string mapContent = null;
    public static JObject allExfils = null;

    public static string settingsFile = null;
    public static string settingsContent = null;
    public static JObject settingsObj = null;

    // config options
    public static ConfigEntry<string> usedExfil;
    public static ConfigEntry<bool> useLastExfil;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"hazelify.CustomInfil has loaded!");
        mapFile = Path.Combine(currentEnv, "BepInEx", "plugins", "CustomInfil", "exfils.json");

        new ExfiltrationPointPatch().Enable();

        usedExfil = Config.Bind(
        "Exfils",       // Category
        "Enter the exfil name",     // Name
        "Cellars",         // Default Value
        new ConfigDescription("Type the name of the exfil zone you wish to use",
        null,
        new ConfigurationManagerAttributes { IsAdvanced = false }));

        useLastExfil = Config.Bind(
            "Last Infil",
            "Toggle mod",
            true,
            "Toggle if the mod should act during spawn or not.");
    }

    public void readSettings()
    {
        if (settingsFile == null) return;
        if (!File.Exists(settingsFile)) return;

        settingsContent = File.ReadAllText(settingsFile);
        settingsObj = JObject.Parse(settingsContent);
    }

    public void readMapFile()
    {
        if (mapFile == null) return;
        if (!File.Exists(mapFile)) return;

        mapContent = File.ReadAllText(mapFile);
        allExfils = JObject.Parse(mapContent);
    }
}

// EXFILS THAT DON'T WORK:
// Gate_o (factory4_day)