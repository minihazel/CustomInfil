using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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
    public static ConfigEntry<string> Reserve_Exfils;
    public static ConfigEntry<string> Lighthouse_Exfils;
    public static ConfigEntry<string> Labs_Exfils;
    public static ConfigEntry<string> Customs_Exfils;
    public static ConfigEntry<string> Streets_Exfils;

    public static List<string> FactoryExfils = null;
    public static List<string> GZExfils = null;
    public static List<string> InterchangeExfils = null;
    public static List<string> ShorelineExfils = null;
    public static List<string> ReserveExfils = null;
    public static List<string> LighthouseExfils = null;
    public static List<string> LabsExfils = null;
    public static List<string> CustomsExfils = null;
    public static List<string> StreetsExfils = null;

    public static string localesFile = null;
    public static string localesContent = null;
    public static JObject localesObj = null;

    public static string mapFile = null;
    public static string mapContent = null;
    public static JObject allExfils = null;

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

        // readLocales();
        readExfils();
        readSpawnpointsFile();
        assignExfils();

        new RaidStartPatch().Enable();

        Factory_Exfils = Config.Bind(
            "Exfils",
            "Factory Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Factory you want to spawn by",
            new AcceptableValueList<string>(FactoryExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        GZ_Exfils = Config.Bind(
            "Exfils",
            "GZ Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Ground Zero you want to spawn by",
            new AcceptableValueList<string>(GZExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Interchange_Exfils = Config.Bind(
            "Exfils",
            "Interchange Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Interchange you want to spawn by",
            new AcceptableValueList<string>(InterchangeExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Shoreline_Exfils = Config.Bind(
            "Exfils",
            "Shoreline Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Shoreline you want to spawn by",
            new AcceptableValueList<string>(ShorelineExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Reserve_Exfils = Config.Bind(
            "Exfils",
            "Reserve Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Reserve you want to spawn by",
            new AcceptableValueList<string>(ReserveExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Lighthouse_Exfils = Config.Bind(
            "Exfils",
            "Lighthouse Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Lighthouse you want to spawn by",
            new AcceptableValueList<string>(LighthouseExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Labs_Exfils = Config.Bind(
            "Exfils",
            "Labs Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Labs you want to spawn by",
            new AcceptableValueList<string>(LabsExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Customs_Exfils = Config.Bind(
            "Exfils",
            "Customs Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Customs you want to spawn by",
            new AcceptableValueList<string>(CustomsExfils.ToArray()),
            new ConfigurationManagerAttributes { Order = 10 }));
        Streets_Exfils = Config.Bind(
            "Exfils",
            "Streets Infil Zone",
            "Random",
            new ConfigDescription("Choose which exfil on Streets you want to spawn by",
            new AcceptableValueList<string>(StreetsExfils.ToArray()),
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
        localesFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.CustomInfil", "locales.json");

        if (localesFile == null) return;
        if (!File.Exists(localesFile)) return;

        localesContent = File.ReadAllText(localesFile);
        localesObj = JObject.Parse(localesContent);
    }

    public void readExfils()
    {
        mapFile = Path.Combine(currentEnv, "BepInEx", "plugins", "hazelify.CustomInfil", "exfils.json");

        if (mapFile == null) return;
        if (!File.Exists(mapFile)) return;

        mapContent = File.ReadAllText(mapFile);
        allExfils = JObject.Parse(mapContent);
    }

    public void assignExfils()
    {
        FactoryExfils = new List<string>();
        GZExfils = new List<string>();
        InterchangeExfils = new List<string>();
        ShorelineExfils = new List<string>();
        ReserveExfils = new List<string>();
        LighthouseExfils = new List<string>();
        LabsExfils = new List<string>();
        CustomsExfils = new List<string>();
        StreetsExfils = new List<string>();
        foreach (var exfil in allExfils)
        {
            string location = exfil.Key.ToString();
            JArray exfilArray = (JArray)exfil.Value["exfils"];
            foreach (var xfil in exfilArray)
            {
                string name = xfil["Name"].ToString();
                string fetched_local_version = (string)localesObj[location][name];

                switch (location)
                {
                    case "factory4_day":
                        FactoryExfils.Add(name);
                        break;
                    case "sandbox":
                        GZExfils.Add(name);
                        break;
                    case "interchange":
                        InterchangeExfils.Add(name);
                        break;
                    case "shoreline":
                        ShorelineExfils.Add(name);
                        break;
                    case "rezervbase":
                        ReserveExfils.Add(name);
                        break;
                    case "lighthouse":
                        LighthouseExfils.Add(name);
                        break;
                    case "laboratory":
                        LabsExfils.Add(name);
                        break;
                    case "bigmap":
                        CustomsExfils.Add(name);
                        break;
                    case "tarkovstreets":
                        StreetsExfils.Add(name);
                        break;
                }
            }
        }
    }
}