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

        public void SetPlayerData(string profileId, string mapName, Vector3 position, Vector2 rotation)
        {
            string playerKey = profileId + "_" + mapName;
            var playerData = new PlayerData(profileId, mapName, position, rotation);
            playerDataDictionary[playerKey] = playerData;
        }

        public PlayerData GetPlayerData(string profileId, string mapName)
        {
            string playerKey = profileId + "_" + mapName;
            if (playerDataDictionary.ContainsKey(playerKey))
            {
                return playerDataDictionary[playerKey];
            }
            return null;
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

        public bool DoesPlayerDataExist(string profileId, string mapName)
        {
            string playerKey = profileId + "_" + mapName;
            return playerDataDictionary.ContainsKey(playerKey);
        }
    }
}
