using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI;
using hazelify.UnlockedEntries.Data;
using hazelify.UnlockedEntries.Patches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using static EFT.SpeedTree.TreeWind;

namespace UnlockedEntries;

[BepInPlugin("hazelify.UnlockedEntries", "Last Infil", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static string currentEnv = Environment.CurrentDirectory; // main SPT dir

    public static MapPlayerManager playerManager = null;
    public static Dictionary<string, PlayerData> playerDataDictionary = null;
    public static Dictionary<string, List<SpawnpointsData>> spawnDataDictionary = null;
    public static bool hasSpawned = false;

    public static ConfigEntry<string> Factory_Exfils;
    public static ConfigEntry<string> GZ_Exfils;
    public static ConfigEntry<string> Interchange_Exfils;
    public static ConfigEntry<string> Shoreline_Exfils;
    public static ConfigEntry<string> Woods_Exfils;
    public static ConfigEntry<string> Reserve_Exfils;
    public static ConfigEntry<string> Lighthouse_Exfils;
    public static ConfigEntry<string> Labs_Exfils;
    public static ConfigEntry<string> Customs_Exfils;
    public static ConfigEntry<string> Streets_Exfils;

    public static List<string> translated_factory4_day = null;
    public static List<string> translated_factory4_night = null;
    public static List<string> translated_bigmap = null;
    public static List<string> translated_sandbox = null;
    public static List<string> translated_rezervbase = null;
    public static List<string> translated_shoreline = null;
    public static List<string> translated_woods = null;
    public static List<string> translated_interchange = null;
    public static List<string> translated_tarkovstreets = null;
    public static List<string> translated_lighthouse = null;

    public static string localesFile = null;
    public static string localesContent = null;
    public static JObject localesObj = null;
    public static Dictionary<string, string> locales = null;

    public static string spawnpointsFile = null;
    public static string spawnpointsContent = null;
    public static JObject spawnpointsObj = null;

    public static string playerDataFile = null;
    public static string playerDataContent = null;
    public static JObject playerDataObj = null;

    // config options
    public static ConfigEntry<string> usedExfil;
    public static ConfigEntry<bool> useLastExfil;
    public static ConfigEntry<bool> chooseInfil;
    public static ConfigEntry<bool> wipePlayerData;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"hazelify.UnlockedEntries has loaded!");

        readLocales();
        readPlayerDataFile();
        readSpawnpointsFile();

        playerManager = new MapPlayerManager();
        playerDataDictionary = MapPlayerManager.LoadPlayerData(playerDataFile);

        new RaidStartPatch().Enable();
        new LocalRaidEndedPatch().Enable();
        new OnPlayerExit().Enable();

        useLastExfil = Config.Bind(
            "Core",
            "A. Spawn into your last exfil?",
            true,
            "Toggle if you want to spawn where you last exfiltrated (if you go back to the same map)");
        chooseInfil = Config.Bind(
            "Core",
            "B. Choose infil spawn?",
            false,
            "Toggle if you want to choose which exfil to spawn at.\n\nUse the map dropdown lists to select which exfiltration zone to infiltrate into (spawn at).");
        wipePlayerData = Config.Bind(
            "Core",
            "C. Wipe saved exfil spawn data",
            false,
            "This will wipe all existing data for where you last exfiltrated on all maps.\n\nTHIS IS IRREVERSIBLE AND WILL HAPPEN INSTANTLY");

        Customs_Exfils = Config.Bind(
            "Exfiltration Zones",
            "A. Customs Spawn Zone",
            "ZB-013",
            new ConfigDescription("Choose which exfil on Customs you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.bigmap.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Factory_Exfils = Config.Bind(
            "Exfiltration Zones",
            "B. Factory Spawn Zone",
            "Cellars",
            new ConfigDescription("Choose which exfil on Factory you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.factory4_day.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Interchange_Exfils = Config.Bind(
            "Exfiltration Zones",
            "C. Interchange Spawn Zone",
            "Hole in the Fence",
            new ConfigDescription("Choose which exfil on Interchange you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.interchange.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Labs_Exfils = Config.Bind(
            "Exfiltration Zones",
            "D. Labs Spawn Zone",
            "Medical Block Elevator",
            new ConfigDescription("Choose which exfil on Labs you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.laboratory.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Lighthouse_Exfils = Config.Bind(
            "Exfiltration Zones",
            "E. Lighthouse Spawn Zone",
            "Armored Train",
            new ConfigDescription("Choose which exfil on Lighthouse you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.lighthouse.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Reserve_Exfils = Config.Bind(
            "Exfiltration Zones",
            "F. Reserve Spawn Zone",
            "Armored Train",
            new ConfigDescription("Choose which exfil on Reserve you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.rezervbase.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        GZ_Exfils = Config.Bind(
            "Exfiltration Zones",
            "G. Ground Zero Spawn Zone",
            "Police Cordon V-Ex",
            new ConfigDescription("Choose which exfil on Ground Zero you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.sandbox.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Shoreline_Exfils = Config.Bind(
            "Exfiltration Zones",
            "H. Shoreline Spawn Zone",
            "Road to North V-Ex",
            new ConfigDescription("Choose which exfil on Shoreline you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.shoreline.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Streets_Exfils = Config.Bind(
            "Exfiltration Zones",
            "I. Streets Spawn Zone",
            "Courtyard",
            new ConfigDescription("Choose which exfil on Streets you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.tarkovstreets.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Woods_Exfils = Config.Bind(
            "Exfiltration Zones",
            "J. Woods Spawn Zone",
            "Friendship Bridge (Co-Op)",
            new ConfigDescription("Choose which exfil on Woods you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.woods.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Factory_Exfils.SettingChanged += OnExfilsSettingChanged;
        GZ_Exfils.SettingChanged += OnExfilsSettingChanged;
        Interchange_Exfils.SettingChanged += OnExfilsSettingChanged;
        Shoreline_Exfils.SettingChanged += OnExfilsSettingChanged;
        Woods_Exfils.SettingChanged += OnExfilsSettingChanged;
        Reserve_Exfils.SettingChanged += OnExfilsSettingChanged;
        Lighthouse_Exfils.SettingChanged += OnExfilsSettingChanged;
        Labs_Exfils.SettingChanged += OnExfilsSettingChanged;
        Customs_Exfils.SettingChanged += OnExfilsSettingChanged;
        Streets_Exfils.SettingChanged += OnExfilsSettingChanged;

        useLastExfil.SettingChanged += onExfilSettingChanged;
        chooseInfil.SettingChanged += onInfilSettingChanged;
    }

    private void OnExfilsSettingChanged(object sender, EventArgs e)
    {
        if (sender is ConfigEntry<string> entry && entry.Description.AcceptableValues is AcceptableValueList<string> valueList)
        {
            var current = entry.Value;
            int index = Array.IndexOf(valueList.AcceptableValues, current);
            if (current.StartsWith("↓") && index + 1 < valueList.AcceptableValues.Length)
            {
                entry.Value = valueList.AcceptableValues[index + 1];
            }
        }
    }

    private void onExfilSettingChanged(object sender, EventArgs e)
    {
        if (useLastExfil.Value)
        {
            chooseInfil.Value = false;
        }
    }

    private void onInfilSettingChanged(object sender, EventArgs e)
    {
        if (chooseInfil.Value)
        {
            useLastExfil.Value = false;
        }
    }

    public void readSpawnpointsFile()
    {
        spawnpointsFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.UnlockedEntries", "spawnpoints.json");
        if (spawnpointsFile == null)
        {
            Logger.LogInfo("`spawnpointsFile` is null");
        }
        else
        {
            if (!File.Exists(spawnpointsFile))
            {
                Logger.LogInfo("`spawnpointsFile` does not exist");
            }
            else
            {
                spawnpointsContent = File.ReadAllText(spawnpointsFile);
                spawnDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<SpawnpointsData>>>(spawnpointsContent);
            }
        }
    }

    public void readLocales()
    {
        localesFile = Path.Combine(currentEnv, "SPT_Data", "Server", "database", "locales", "global", "en.json");
        if (localesFile == null)
        {
            Logger.LogInfo("`localesFile` is null");
        }
        else
        {
            if (!File.Exists(localesFile))
            {
                Logger.LogInfo("`localesFile` does not exist");
            }
            else
            {
                localesContent = File.ReadAllText(localesFile);
                locales = JsonConvert.DeserializeObject<Dictionary<string, string>>(localesContent);
            }
        }
    }

    public static void readPlayerDataFile()
    {
        playerDataFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.UnlockedEntries", "PlayerData.json");

        if (playerDataFile == null)
        {
            Logger.LogInfo("`playerDataFile` is null, regenerating");
            MapPlayerManager.GeneratePlayerData(playerDataFile);
        }
        else
        {
            if (!File.Exists(playerDataFile))
            {
                Logger.LogInfo("`playerDataFile` does not exist, regenerating");
                MapPlayerManager.GeneratePlayerData(playerDataFile);
            }
            else
            {
                playerDataContent = File.ReadAllText(playerDataFile);
            }
        }
    }

    public static string GetLocalizedName(string name)
    {
        if (locales.ContainsKey(name) && locales[name] != null)
            return locales[name].ToString().Localized();
        return name;
    }

    public List<string> assignTranslatedNames(List<string> names)
    {
        List<string> translatedNames = new List<string>();
        foreach (string name in names)
        {
            string translatedName = GetLocalizedName(name);
            string merged_name = name + " (" + translatedName + ")";
            translatedNames.Add(merged_name);
        }

        return translatedNames;
    }

    public string translateExfilName(string exfil)
    {
        string translated = null;

        if (locales.ContainsKey(exfil))
        {
            string translatedName = locales[exfil].ToString().Localized();
            translated = translatedName;
        }

        return translated;
    }

    public static List<string> GetExfilList(string mapName)
    {
        var type = typeof(ExfilData);
        var field = type.GetField(mapName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (field != null)
        {
            return field.GetValue(null) as List<string>;
        }

        return null;
    }

    public static void logIssue(string message, bool logOnConsole)
    {
        message = "[UnlockedEntries] " + message;

        if (logOnConsole)
        {
            Logger.LogError(message);
            ConsoleScreen.Log(message);
        }
        else
        {
            Logger.LogInfo(message);
        }
    }
}