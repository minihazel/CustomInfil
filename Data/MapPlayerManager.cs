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

        public PlayerData GetPlayerData(string mapName)
        {
            if (Plugin.playerDataDictionary.ContainsKey(mapName))
            {
                return Plugin.playerDataDictionary[mapName];
            }
            return null;
        }

        public void SetPlayerData(string profileId, string mapName, Vector3 position, Vector2 rotation)
        {
            float positionX = position.x;
            float positionY = position.y;
            float positionZ = position.z;

            float rotationX = rotation.x;
            float rotationY = rotation.y;

            var playerData = new PlayerData(profileId, positionX, positionY, positionZ, rotationX, rotationY);
            Plugin.playerDataDictionary[mapName] = playerData;
            SavePlayerData(Plugin.playerDataDictionary);
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
            return Plugin.playerDataDictionary.ContainsKey(mapName);
        }
    }
}
