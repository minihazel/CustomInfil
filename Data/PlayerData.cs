using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT;

namespace hazelify.CustomInfil.Data
{
    public class PlayerData
    {
        public string ProfileId { get; set; }
        public string MapName { get; set; }
        public Vector3 Position { get; set; }
        public Vector2 Rotation { get; set; }

        public PlayerData(string profileId, string mapName, Vector3 position, Vector2 rotation)
        {
            ProfileId = profileId;
            MapName = mapName;
            Position = position;
            Rotation = rotation;
        }
    }
}
