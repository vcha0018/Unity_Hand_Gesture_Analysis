using UnityEngine;

namespace DataStructure
{
    /// <summary>
    /// Boundung box of a gesture.
    /// </summary>
    public class BoundingBox
    {
        public float maxX = float.NegativeInfinity;
        public float minX = float.PositiveInfinity;
        public float maxY = float.NegativeInfinity;
        public float minY = float.PositiveInfinity;
        public float maxZ = float.NegativeInfinity;
        public float minZ = float.PositiveInfinity;
    }

    /// <summary>
    /// Cuboid (center) of a gesture.
    /// </summary>
    public class Cuboid
    {
        public Vector3 TopLeft;
        public Vector3 BottomRight;

        #region Properties: width, height, depth, area, volume

        public float Width
        {
            get
            {
                return BottomRight.x - TopLeft.x;
            }
        }

        public float Height
        {
            get
            {
                return BottomRight.y - TopLeft.y;
            }
        }

        public float Depth
        {
            get
            {
                return BottomRight.z - TopLeft.z;
            }
        }

        public float AreaXY
        {
            get
            {
                return Width * Height;
            }
        }

        public float Volume
        {
            get
            {
                return Width * Height * Depth;
            }
        }

        #endregion
    }
}
