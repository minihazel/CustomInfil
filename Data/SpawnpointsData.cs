using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hazelify.CustomInfil.Data
{
    public class SpawnpointsData
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Infiltration { get; set; }

        public float Rotation_X { get; set; }
        public float Rotation_Y { get; set; }
        public float Rotation_Z { get; set; }

        public float coord_X { get; set; }
        public float coord_Y { get; set; }
        public float coord_Z { get; set; }
    }
}
