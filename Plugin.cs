using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI;
using hazelify.CustomInfil.Data;
using hazelify.CustomInfil.Patches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using static EFT.SpeedTree.TreeWind;

namespace CustomInfil;

[BepInPlugin("hazelify.custominfil", "Last Infil", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static string currentEnv = Environment.CurrentDirectory; // main SPT dir
    public static MapPlayerManager playerManager = null;

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
        Logger.LogInfo($"hazelify.CustomInfil has loaded!");

        readLocales();
        readSpawnpointsFile();
        readPlayerDataFile();

        playerManager = new MapPlayerManager();
        new RaidStartPatch().Enable();
        new LocalRaidEndedPatch().Enable();

        chooseInfil = Config.Bind(
            "CustomInfil",
            "Choose infil manually?",
            true,
            "Toggle if you want to choose which exfil to spawn at.");

        wipePlayerData = Config.Bind(
            "CustomInfil",
            "Wipe existing player data",
            false,
            "This will wipe all existing exfiltration data for all maps, so where you last exfiltrated on any given map is wiped clean.");

        Factory_Exfils = Config.Bind(
            "Exfils",
            "Factory Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Factory you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.factory4_day.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        GZ_Exfils = Config.Bind(
            "Exfils",
            "GZ Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Ground Zero you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.sandbox.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Interchange_Exfils = Config.Bind(
            "Exfils",
            "Interchange Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Interchange you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.interchange.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Shoreline_Exfils = Config.Bind(
            "Exfils",
            "Shoreline Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Shoreline you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.shoreline.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Woods_Exfils = Config.Bind(
            "Exfils",
            "Woods Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Woods you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.woods.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Reserve_Exfils = Config.Bind(
            "Exfils",
            "Reserve Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Reserve you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.rezervbase.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Lighthouse_Exfils = Config.Bind(
            "Exfils",
            "Lighthouse Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Lighthouse you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.lighthouse.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Labs_Exfils = Config.Bind(
            "Exfils",
            "Labs Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Labs you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.laboratory.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Customs_Exfils = Config.Bind(
            "Exfils",
            "Customs Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Customs you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.bigmap.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Streets_Exfils = Config.Bind(
            "Exfils",
            "Streets Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Streets you want to spawn by",
            new AcceptableValueList<string>(ExfilDescData.tarkovstreets.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        useLastExfil = Config.Bind(
            "Last Infil",
            "Toggle mod",
            true,
            "Toggle if the mod should act during spawn or not.");

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
    }

    private void OnExfilsSettingChanged(object sender, EventArgs e)
    {
        if (sender is ConfigEntry<string> entry &&
        entry.Description.AcceptableValues is AcceptableValueList<string> valueList)
        {
            var current = entry.Value;
            int index = Array.IndexOf(valueList.AcceptableValues, current);

            // If it's a category marker like ↓ PMC EXFILS ↓, select the one below it
            if (current.StartsWith("↓") && index + 1 < valueList.AcceptableValues.Length)
            {
                entry.Value = valueList.AcceptableValues[index + 1];
            }
        }
    }

    public void readSpawnpointsFile()
    {
        spawnpointsFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.CustomInfil", "spawnpoints.json");

        if (spawnpointsFile == null) return;
        if (!File.Exists(spawnpointsFile)) return;

        spawnpointsContent = File.ReadAllText(spawnpointsFile);
        spawnpointsObj = JObject.Parse(spawnpointsContent);
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
                localesObj = JObject.Parse(localesContent);
            }
        }
    }

    public void readPlayerDataFile()
    {
        playerDataFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.CustomInfil", "PlayerData.json");

        if (playerDataFile == null)
        {
            Logger.LogInfo("`playerDataFile` is null");
        }
        else
        {
            if (!File.Exists(playerDataFile))
            {
                Logger.LogInfo("`playerDataFile` does not exist");
            }
            else
            {
                playerDataContent = File.ReadAllText(playerDataFile);
                playerDataObj = JObject.Parse(playerDataContent);
            }
        }
    }

    public static string GetLocalizedName(string name)
    {
        if (localesObj.ContainsKey(name) && localesObj[name] != null)
            return localesObj[name].ToString().Localized();
        return name;
    }

    public static string GetOriginalName(string location, string localizedName)
    {
        if (localesObj.TryGetValue(location, out JToken locationObj))
        {
            foreach (var prop in locationObj.Children<JProperty>())
            {
                if (prop.Value.ToString().Equals(localizedName, StringComparison.OrdinalIgnoreCase))
                    return prop.Name;
            }
        }
        return localizedName;
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

        if (localesObj.ContainsKey(exfil))
        {
            string translatedName = localesObj[exfil].ToString().Localized();
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

    public static Dictionary<string, PlayerData> LoadPlayerData()
    {
        if (!File.Exists(playerDataFile))
        {
            logIssue("PlayerData.json file not found, creating a new one.", true);
            File.Create(playerDataFile).Close();
            return new Dictionary<string, PlayerData>();
        }

        string json = File.ReadAllText(playerDataFile);
        return JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(json);
        return new Dictionary<string, PlayerData>();
    }

    public static void logIssue(string message, bool isError)
    {
        message = "[CustomInfil] " + message;

        ConsoleScreen.Log(message);
        if (isError)
        {
            Logger.LogError(message);
        }
        else
        {
            Logger.LogInfo(message);
        }
    }
}