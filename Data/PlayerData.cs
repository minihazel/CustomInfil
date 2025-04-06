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
        public float Position_X { get; set; }
        public float Position_Y { get; set; }
        public float Position_Z { get; set; }
        public float Rotation_X { get; set; }
        public float Rotation_Y { get; set; }

        public PlayerData(string profileId,
            float position_x,
            float position_y,
            float position_z,
            float rotation_x,
            float rotation_y)
        {
            ProfileId = profileId;
            Position_X = position_x;
            Position_Y = position_y;
            Position_Z = position_z;
            Rotation_X = rotation_x;
            Rotation_Y = rotation_y;
        }

        public Vector3 GetPosition() => new Vector3(Position_X, Position_Y, Position_Z);
        public Vector2 GetRotation() => new Vector2(Rotation_X, Rotation_Y);
    }
}
