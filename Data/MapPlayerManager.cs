using System;
using System.Collections.Generic;
using UnityEngine;
using EFT;
using Newtonsoft.Json;
using UnlockedEntries;
using System.IO;

namespace hazelify.UnlockedEntries.Data
{
    public class MapPlayerManager
    {

        public PlayerData GetPlayerData(string mapName)
        {
            if (Plugin.playerDataDictionary.ContainsKey(mapName))
            {
                return Plugin.playerDataDictionary[mapName];
            }
            return null;
        }

        public void SetPlayerData(string mapName, Vector3 position, Vector2 rotation)
        {
            float positionX = position.x;
            float positionY = position.y;
            float positionZ = position.z;

            float rotationX = rotation.x;
            float rotationY = rotation.y;

            var playerData = new PlayerData(positionX, positionY, positionZ, rotationX, rotationY);
            Plugin.playerDataDictionary[mapName] = playerData;
            SavePlayerData(Plugin.playerDataDictionary);
        }

        public void SavePlayerData(Dictionary<string, PlayerData> playerDataDict)
        {
            string json = JsonConvert.SerializeObject(playerDataDict, jsonSettings);
            try
            {
                File.WriteAllText(Plugin.playerDataFile, json);
                Plugin.logIssue("SavePlayerData -> Success, saved player data into file `PlayerData.json`", false);
            }
            catch (Exception ex)
            {
                Plugin.logIssue(ex.Message.ToString(), false);
            }
        }

        public static Dictionary<string, PlayerData> LoadPlayerData(string currentFile)
        {
            if (!File.Exists(currentFile) || Plugin.playerDataContent == null)
            {
                GeneratePlayerData(currentFile);
            }

            return JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(Plugin.playerDataContent, jsonSettings);
        }

        public static void GeneratePlayerData(string currentFile)
        {
            Plugin.logIssue("PlayerData.json file not found, creating a new one.", false);
            File.Create(currentFile).Close();

            Dictionary<string, PlayerData> newPlayerData = new Dictionary<string, PlayerData>
            {
                {"factory4_day", new PlayerData( 0, 0, 0, 0, 0)},
                {"factory4_night", new PlayerData( 0, 0, 0, 0, 0)},
                {"bigmap", new PlayerData( 0, 0, 0, 0, 0)},
                {"sandbox", new PlayerData( 0, 0, 0, 0, 0)},
                {"sandbox_high", new PlayerData( 0, 0, 0, 0, 0)},
                {"rezervbase", new PlayerData( 0, 0, 0, 0, 0)},
                {"shoreline", new PlayerData( 0, 0, 0, 0, 0)},
                {"woods", new PlayerData( 0, 0, 0, 0, 0)},
                {"interchange", new PlayerData( 0, 0, 0, 0, 0)},
                {"laboratory", new PlayerData( 0, 0, 0, 0, 0)},
                {"tarkovstreets", new PlayerData( 0, 0, 0, 0, 0)},
                {"lighthouse", new PlayerData( 0, 0, 0, 0, 0)}
            };

            string json = JsonConvert.SerializeObject(newPlayerData, jsonSettings);
            File.WriteAllText(currentFile, json);
            Plugin.readPlayerDataFile();
        }


        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            FloatFormatHandling = FloatFormatHandling.DefaultValue,
            FloatParseHandling = FloatParseHandling.Double,
            Formatting = Formatting.Indented
        };

        public bool DoesPlayerDataExist(string mapName)
        {
            return Plugin.playerDataDictionary.ContainsKey(mapName);
        }
    }
}
