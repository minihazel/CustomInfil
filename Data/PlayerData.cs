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
        public Vector3 Position { get; set; }
        public Vector2 Rotation { get; set; }

        public PlayerData(string profileId, Vector3 position, Vector2 rotation)
        {
            ProfileId = profileId;
            Position = position;
            Rotation = rotation;
        }

        public Vector3 GetPosition() => new Vector3(Position.x, Position.y, Position.z);
        public Vector3 GetRotation() => new Vector2(Rotation.x, Rotation.y);
    }
}
