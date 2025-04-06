using System;
using System.Collections.Generic;
using UnityEngine;
using EFT;
using Newtonsoft.Json;
using CustomInfil;
using System.IO;

namespace hazelify.CustomInfil.Data
{
    public class MapPlayerManager
    {
        private Dictionary<string, PlayerData> playerDataDictionary = new Dictionary<string, PlayerData>();

        public PlayerData GetPlayerData(string mapName)
        {
            if (playerDataDictionary.ContainsKey(mapName))
            {
                return playerDataDictionary[mapName];
            }
            return null;
        }

        public void SetPlayerData(string profileId, string mapName, Vector3 position, Vector2 rotation)
        {
            var playerData = new PlayerData(profileId, position, rotation);
            playerDataDictionary[mapName] = playerData;

            SavePlayerData(playerDataDictionary);
        }

        public void SavePlayerData(Dictionary<string, PlayerData> playerDataDict)
        {
            string json = JsonConvert.SerializeObject(playerDataDict, Formatting.Indented);
            try
            {
                File.WriteAllText(Plugin.playerDataFile, json);
            }
            catch (Exception ex)
            {
                Plugin.logIssue(ex.Message.ToString(), true);
            }
        }

        public bool DoesPlayerDataExist(string mapName)
        {
            return playerDataDictionary.ContainsKey(mapName);
        }
    }
}
