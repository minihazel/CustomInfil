using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI;
using hazelify.UnlockedEntries.Data;
using hazelify.UnlockedEntries.Patches;
using hazelify.UnlockedEntries.Patches.DebugPatches;
using hazelify.UnlockedEntries.Patches.DebugPatches;
using hazelify.UnlockedEntries.Patches.PhysicsTriggers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using static EFT.SpeedTree.TreeWind;

namespace UnlockedEntries;

[BepInPlugin("hazelify.UnlockedEntries", "UnlockedEntries", "1.0.4")]
[BepInDependency("Jehree.HomeComforts", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static string currentEnv = Environment.CurrentDirectory; // main SPT dir
    public static bool isLITInstalled { get; private set; } // check if LIT is installed
    public static string LITmod = "Jehree.HomeComforts";

    public static bool isFikaInstalled { get; private set; } // check if Fika is installed
    public static string Fikamod = "com.fika.core";

    // ..\..\..\BepInEx\plugins\hazelify.UnlockedEntries\
    // $(ProjectDir)\Build\hazelify.UnlockedEntries\

    public static MapPlayerManager playerManager = null;
    public static Dictionary<string, PlayerData> playerDataDictionary = null;
    public static Dictionary<string, List<SpawnpointsData>> spawnDataDictionary = null;
    public static bool hasSpawned = false;

    public static ConfigEntry<string> Customs_Exfils;
    public static ConfigEntry<string> Factory_Exfils;
    public static ConfigEntry<string> GZ_Exfils;
    public static ConfigEntry<string> Interchange_Exfils;
    public static ConfigEntry<string> Shoreline_Exfils;
    public static ConfigEntry<string> Woods_Exfils;
    public static ConfigEntry<string> Reserve_Exfils;
    public static ConfigEntry<string> Lighthouse_Exfils;
    public static ConfigEntry<string> Labs_Exfils;
    public static ConfigEntry<string> Streets_Exfils;

    public static ConfigEntry<string> Customs_Exfils_Scavs;
    public static ConfigEntry<string> Factory_Exfils_Scavs;
    public static ConfigEntry<string> GZ_Exfils_Scavs;
    public static ConfigEntry<string> Interchange_Exfils_Scavs;
    public static ConfigEntry<string> Shoreline_Exfils_Scavs;
    public static ConfigEntry<string> Woods_Exfils_Scavs;
    public static ConfigEntry<string> Reserve_Exfils_Scavs;
    public static ConfigEntry<string> Lighthouse_Exfils_Scavs;
    public static ConfigEntry<string> Labs_Exfils_Scavs;
    public static ConfigEntry<string> Streets_Exfils_Scavs;

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
    public static ConfigEntry<bool> debug_exfildumper;
    public static ConfigEntry<bool> debug_spawndumper;

    public static ConfigEntry<string> usedExfil;
    public static ConfigEntry<bool> useLastExfil;
    public static ConfigEntry<bool> chooseInfil;
    public static ConfigEntry<bool> wipePlayerData;
    public static ConfigEntry<bool> LITmodEntry;

    private void Awake()
    {
        isLITInstalled = Chainloader.PluginInfos.ContainsKey(LITmod);
        isFikaInstalled = Chainloader.PluginInfos.ContainsKey(Fikamod);

        Logger = base.Logger;
        Logger.LogInfo($"hazelify.UnlockedEntries has loaded!");

        readLocales();
        readPlayerDataFile();
        readSpawnpointsFile();

        playerManager = new MapPlayerManager();
        playerDataDictionary = MapPlayerManager.LoadPlayerData(playerDataFile);

        if (isFikaInstalled)
        {
            new FikaLocalRaidEndedPatch().Enable();
        }
        else
        {
            new LocalRaidEndedPatch().Enable();
        }

        new RaidStartPatch().Enable();
        new OnTriggerEnterPatch().Enable();
        new OnTriggerExitPatch().Enable();
        new ExfilDumper().Enable();
        new SpawnpointDumper().Enable();

        if (isLITInstalled)
        {
            addHCSupport();

            LITmodEntry = Config.Bind(
                "1. HomeComforts detected - Use Last Exfil disabled.",
                "Mod detected",
                true,
                "All this means is that toggling \"Spawn into your last exfil\" will do nothing, you will always spawn at your HomeComforts safehouse.");
        }

        debug_exfildumper = Config.Bind(
            "2. Core",
            "A. Dump Map Exfils",
            false,
            new ConfigDescription("Debug option: If you wish to dump all the exfiltration zones from the map you enter next. This option will disable itself once you spawn in.\n\n" +
            "Dumped info will be saved into: hazelify.UnlockedEntries\\debug_exfils.json",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));
        debug_spawndumper = Config.Bind(
            "2. Core",
            "B. Dump Map Spawns",
            false,
            new ConfigDescription("Debug option: If you wish to dump all the spawnpoints from the map you enter next. This option will disable itself once you spawn in.\n\n" +
            "Dumped info will be saved into: hazelify.UnlockedEntries\\spawnpoints.json",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

        useLastExfil = Config.Bind(
            "2. Core",
            "A. Spawn into your last exfil?",
            true,
            "Toggle if you want to spawn where you last exfiltrated (if you go back to the same map)");
        chooseInfil = Config.Bind(
            "2. Core",
            "B. Choose infil spawn?",
            false,
            "Toggle if you want to choose which exfil to spawn at.\n\nUse the map dropdown lists to select which exfiltration zone to infiltrate into (spawn at).\n\nWARNING! This may not work unless you have all exfils opened via SVM or another mod.");
        wipePlayerData = Config.Bind(
            "2. Core",
            "C. Wipe saved exfil spawn data",
            false,
            "This will wipe all existing data for where you last exfiltrated on all maps.\n\nTHIS IS IRREVERSIBLE AND WILL HAPPEN INSTANTLY");

        Customs_Exfils = Config.Bind(
            "A. Customs",
            "A. Customs Spawn Zone",
            "ZB-013",
            new ConfigDescription("Choose which exfil on Customs you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.bigmap.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Customs_Exfils_Scavs = Config.Bind(
            "A. Customs",
            "B. Customs Scav Spawn Zone",
            "Military Base CP",
            new ConfigDescription("Choose which exfil on Customs you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.bigmap_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Factory_Exfils = Config.Bind(
            "B. Factory",
            "A. Factory Spawn Zone",
            "Cellars",
            new ConfigDescription("Choose which exfil on Factory you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.factory4_day.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Factory_Exfils_Scavs = Config.Bind(
            "B. Factory",
            "B. Factory Scav Spawn Zone",
            "Cellars",
            new ConfigDescription("Choose which exfil on Factory you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.factory4_day_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Interchange_Exfils = Config.Bind(
            "C. Interchange",
            "A. Interchange Spawn Zone",
            "Hole in the Fence",
            new ConfigDescription("Choose which exfil on Interchange you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.interchange.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Interchange_Exfils_Scavs = Config.Bind(
            "C. Interchange",
            "B. Interchange Scav Spawn Zone",
            "Emercom Checkpoint",
            new ConfigDescription("Choose which exfil on Interchange you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.interchange_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Lighthouse_Exfils = Config.Bind(
            "D. Lighthouse",
            "A. Lighthouse Spawn Zone",
            "Armored Train",
            new ConfigDescription("Choose which exfil on Lighthouse you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.lighthouse.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Lighthouse_Exfils_Scavs = Config.Bind(
            "D. Lighthouse",
            "B. Lighthouse Scav Spawn Zone",
            "Side Tunnel (Co-Op)",
            new ConfigDescription("Choose which exfil on Lighthouse you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.lighthouse_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Reserve_Exfils = Config.Bind(
            "E. Reserve",
            "A. Reserve Spawn Zone",
            "Armored Train",
            new ConfigDescription("Choose which exfil on Reserve you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.rezervbase.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Reserve_Exfils_Scavs = Config.Bind(
            "E. Reserve",
            "B. Reserve Scav Spawn Zone",
            "Hole in the Wall by the Mountains",
            new ConfigDescription("Choose which exfil on Reserve you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.rezervbase_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        GZ_Exfils = Config.Bind(
            "F. Ground Zero",
            "A. Ground Zero Spawn Zone",
            "Police Cordon V-Ex",
            new ConfigDescription("Choose which exfil on Ground Zero you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.sandbox.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        GZ_Exfils_Scavs = Config.Bind(
            "F. Ground Zero",
            "B. Ground Zero Scav Spawn Zone",
            "Emercom Checkpoint",
            new ConfigDescription("Choose which exfil on Ground Zero you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.sandbox_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        
        Shoreline_Exfils = Config.Bind(
            "G. Shoreline",
            "A. Shoreline Spawn Zone",
            "Road to North V-Ex",
            new ConfigDescription("Choose which exfil on Shoreline you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.shoreline.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Shoreline_Exfils_Scavs = Config.Bind(
            "G. Shoreline",
            "B. Shoreline Scav Spawn Zone",
            "Road to Customs",
            new ConfigDescription("Choose which exfil on Shoreline you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.shoreline_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Streets_Exfils = Config.Bind(
            "H. Streets of Tarkov",
            "A. Streets Spawn Zone",
            "Courtyard",
            new ConfigDescription("Choose which exfil on Streets you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.tarkovstreets.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Streets_Exfils_Scavs = Config.Bind(
            "H. Streets of Tarkov",
            "B. Streets Scav Spawn Zone",
            "Entrance to Catacombs",
            new ConfigDescription("Choose which exfil on Streets you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.tarkovstreets_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Woods_Exfils = Config.Bind(
            "I. Woods",
            "A. Woods Spawn Zone",
            "Friendship Bridge (Co-Op)",
            new ConfigDescription("Choose which exfil on Woods you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.woods.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Woods_Exfils_Scavs = Config.Bind(
            "I. Woods",
            "B. Woods Scav Spawn Zone",
            "Northern UN Roadblock",
            new ConfigDescription("Choose which exfil on Woods you want to spawn by as a Scav",
            new AcceptableValueList<string>(ExfilDescData.woods_scav.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Labs_Exfils = Config.Bind(
            "K. Laboratory",
            "A. Labs Spawn Zone",
            "Medical Block Elevator",
            new ConfigDescription("Choose which exfil on Labs you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.laboratory.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        Factory_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        GZ_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Interchange_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Shoreline_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Woods_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Reserve_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Lighthouse_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Labs_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Customs_Exfils.SettingChanged += OnExfilDropdownSettingChanged;
        Streets_Exfils.SettingChanged += OnExfilDropdownSettingChanged;

        useLastExfil.SettingChanged += onExfilSettingChanged;
        chooseInfil.SettingChanged += onInfilSettingChanged;
    }

    private void addHCSupport()
    {
        ExfilDescData.factory4_day.Add("Home Comforts Safehouse");
        ExfilDescData.factory4_day_scav.Add("Home Comforts Safehouse");
        ExfilDescData.factory4_night.Add("Home Comforts Safehouse");
        ExfilDescData.factory4_night_scav.Add("Home Comforts Safehouse");

        ExfilDescData.bigmap.Add("Home Comforts Safehouse");
        ExfilDescData.bigmap_scav.Add("Home Comforts Safehouse");

        ExfilDescData.sandbox.Add("Home Comforts Safehouse");
        ExfilDescData.sandbox_scav.Add("Home Comforts Safehouse");

        ExfilDescData.sandbox_high.Add("Home Comforts Safehouse");
        ExfilDescData.sandbox_high_scav.Add("Home Comforts Safehouse");

        ExfilDescData.rezervbase.Add("Home Comforts Safehouse");
        ExfilDescData.rezervbase_scav.Add("Home Comforts Safehouse");

        ExfilDescData.lighthouse.Add("Home Comforts Safehouse");
        ExfilDescData.lighthouse_scav.Add("Home Comforts Safehouse");

        ExfilDescData.shoreline.Add("Home Comforts Safehouse");
        ExfilDescData.shoreline_scav.Add("Home Comforts Safehouse");

        ExfilDescData.woods.Add("Home Comforts Safehouse");
        ExfilDescData.woods_scav.Add("Home Comforts Safehouse");

        ExfilDescData.interchange.Add("Home Comforts Safehouse");
        ExfilDescData.interchange_scav.Add("Home Comforts Safehouse");

        ExfilDescData.tarkovstreets.Add("Home Comforts Safehouse");
        ExfilDescData.tarkovstreets_scav.Add("Home Comforts Safehouse");

        ExfilDescData.laboratory.Add("Home Comforts Safehouse");


        ExfilData.factory4_day.Add("homecomforts_safehouse");
        ExfilData.factory4_day_scav.Add("homecomforts_safehouse");
        ExfilData.factory4_night.Add("homecomforts_safehouse");
        ExfilData.factory4_night_scav.Add("homecomforts_safehouse");

        ExfilData.bigmap.Add("homecomforts_safehouse");
        ExfilData.bigmap_scav.Add("homecomforts_safehouse");

        ExfilData.sandbox.Add("homecomforts_safehouse");
        ExfilData.sandbox_scav.Add("homecomforts_safehouse");

        ExfilData.sandbox_high.Add("homecomforts_safehouse");
        ExfilData.sandbox_high_scav.Add("homecomforts_safehouse");

        ExfilData.rezervbase.Add("homecomforts_safehouse");
        ExfilData.rezervbase_scav.Add("homecomforts_safehouse");

        ExfilData.lighthouse.Add("homecomforts_safehouse");
        ExfilData.lighthouse_scav.Add("homecomforts_safehouse");

        ExfilData.shoreline.Add("homecomforts_safehouse");
        ExfilData.shoreline_scav.Add("homecomforts_safehouse");

        ExfilData.woods.Add("homecomforts_safehouse");
        ExfilData.woods_scav.Add("homecomforts_safehouse");

        ExfilData.interchange.Add("homecomforts_safehouse");
        ExfilData.interchange_scav.Add("homecomforts_safehouse");

        ExfilData.tarkovstreets.Add("homecomforts_safehouse");
        ExfilData.tarkovstreets_scav.Add("homecomforts_safehouse");

        ExfilData.laboratory.Add("homecomforts_safehouse");
    }

    private void OnExfilDropdownSettingChanged(object sender, EventArgs e)
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

    public static void readSpawnpointsFile()
    {
        spawnpointsFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.UnlockedEntries", "spawnpoints.json");
        if (spawnpointsFile == null)
        {
            File.Create(spawnpointsFile).Close();
            File.WriteAllText(spawnpointsFile, "{}");
            Logger.LogInfo("`spawnpointsFile` is null");
        }
        else
        {
            if (!File.Exists(spawnpointsFile))
            {
                File.Create(spawnpointsFile).Close();
                File.WriteAllText(spawnpointsFile, "{}");
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
            var originalList = field.GetValue(null) as List<string>;
            if (originalList != null)
            {
                return originalList.Select(s => s.ToLower()).ToList();
            }
        }

        return null;
    }

    public static void logIssue(string message, bool logOnConsole)
    {
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