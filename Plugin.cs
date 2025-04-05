using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.UI;
using hazelify.CustomInfil;
using hazelify.CustomInfil.Patches;
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

    public static string localesFile = null;
    public static string localesContent = null;
    public static JObject localesObj = null;

    public static string spawnpointsFile = null;
    public static string spawnpointsContent = null;
    public static JObject spawnpointsObj = null;

    // config options
    public static ConfigEntry<string> usedExfil;
    public static ConfigEntry<bool> useLastExfil;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"hazelify.CustomInfil has loaded!");

        readLocales();
        readSpawnpointsFile();

        //new RaidStartPatch().Enable();
        // new ExfilDumper().Enable();

        Factory_Exfils = Config.Bind(
            "Exfils",
            "Factory Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Factory you want to spawn by",
            new AcceptableValueList<string>(ExfilData.factory4_day.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        GZ_Exfils = Config.Bind(
            "Exfils",
            "GZ Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Ground Zero you want to spawn by",
            new AcceptableValueList<string>(ExfilData.sandbox.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Interchange_Exfils = Config.Bind(
            "Exfils",
            "Interchange Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Interchange you want to spawn by",
            new AcceptableValueList<string>(ExfilData.interchange.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Shoreline_Exfils = Config.Bind(
            "Exfils",
            "Shoreline Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Shoreline you want to spawn by",
            new AcceptableValueList<string>(ExfilData.shoreline.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Woods_Exfils = Config.Bind(
            "Exfils",
            "Woods Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Woods you want to spawn by",
            new AcceptableValueList<string>(ExfilData.woods.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Reserve_Exfils = Config.Bind(
            "Exfils",
            "Reserve Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Reserve you want to spawn by",
            new AcceptableValueList<string>(ExfilData.rezervbase.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Lighthouse_Exfils = Config.Bind(
            "Exfils",
            "Lighthouse Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Lighthouse you want to spawn by",
            new AcceptableValueList<string>(ExfilData.lighthouse.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Labs_Exfils = Config.Bind(
            "Exfils",
            "Labs Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Labs you want to spawn by",
            new AcceptableValueList<string>(ExfilData.laboratory.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Customs_Exfils = Config.Bind(
            "Exfils",
            "Customs Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Customs you want to spawn by",
            new AcceptableValueList<string>(ExfilData.bigmap.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Streets_Exfils = Config.Bind(
            "Exfils",
            "Streets Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Streets you want to spawn by",
            new AcceptableValueList<string>(ExfilData.tarkovstreets.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));

        useLastExfil = Config.Bind(
            "Last Infil",
            "Toggle mod",
            true,
            "Toggle if the mod should act during spawn or not.");
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