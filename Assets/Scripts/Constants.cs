using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Constants
{
    public static int NUM_JOINTS = 21;
    public static List<string> GESTURE_DIR_PATHS = new List<string>()
    {
        @"results\mediapipe\vivek1\",
        @"results\mediapipe\vivek2\",
        @"results\mediapipe\vivek3\",
        @"results\mediapipe\vivek4\"
    };
}

public enum GestureTypeFormat : ushort
{
    None = 0,
    Export = 1,
    Filter = 2,
    Highlight = 3,
    MultiSelect = 4,
    Pan = 5,
    Rotate = 6,
    SaveView = 7,
    SelectAxis = 8,
    SelectCluster = 9,
    SelectLasso = 10,
    SelectSingle = 11,
    Zoom = 12,
    SelectRange = 13,
}

public enum HandTypeFormat: ushort
{
    LEFT = 0,
    RIGHT = 1
}

public enum ComparisionTypeFormat: ushort
{
    PersonWise = 1,
    GestureWise = 2
}
