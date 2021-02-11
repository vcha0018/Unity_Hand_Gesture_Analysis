using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class BoundingBox
    {
        public float maxX = float.NegativeInfinity;
        public float minX = float.PositiveInfinity;
        public float maxY = float.NegativeInfinity;
        public float minY = float.PositiveInfinity;
        public float maxZ = float.NegativeInfinity;
        public float minZ = float.PositiveInfinity;
    }
}
