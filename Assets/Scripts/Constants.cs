/*
Author:
Vivekkumar Chaudhari (vcha0018@student.monash.edu) 
    Student - Master of Information Technology
    Monash University, Clayton, Australia

Purpose:
Developed under Summer Project 'AR Hand Gesture Capture and Analysis'

Supervisors: 
Barrett Ens (barrett.ens@monash.edu)
    Monash University, Clayton, Australia
 Max Cordeil (max.cordeil@monash.edu)
    Monash University, Clayton, Australia

About File:
Application's static values.
*/

using System.Collections.Generic;

public static class Constants
{
    /// <summary>
    /// Define number of hand joints in CSV file.
    /// It is always the same for all gestures all hand types.
    /// </summary>
    public static int NUM_JOINTS = 21;
    public static List<string> GESTURE_DIR_PATHS = new List<string>()
    {
        @"results\mediapipe\vivek1\",
        @"results\mediapipe\vivek2\",
        @"results\mediapipe\vivek3\",
        @"results\mediapipe\vivek4\"
    };
    /// <summary>
    /// Default frame rate, used to measure euclidean distance between gesture set.
    /// </summary>
    public static int DEFAULT_FRAME_RATE = 25;
}

/// <summary>
/// Available gesture types(actions).
/// </summary>
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

/// <summary>
/// Hand Types.
/// </summary>
public enum HandTypeFormat: ushort
{
    LEFT = 0,
    RIGHT = 1
}

/// <summary>
/// Available Dissimilarity functions.
/// </summary>
public enum DissimilarityFunctionType: ushort
{
    EuclideanDistance = 1,
    DTW = 2,
    NormalizedDTW = 3,
    ModifiedHausdorff = 4
}

/// <summary>
/// Available Aggregation functions.
/// </summary>
public enum AggregationType: ushort
{
    Average = 1,
    Max = 2,
    Min = 3
}
