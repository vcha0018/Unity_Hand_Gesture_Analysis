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
Defines the dissimilarity functions.
*/

using DataStructure;
using System;

namespace Analysis
{
    /// <summary>
    /// Dissimilarity measures: Euclidean, DTW, Modified Hausdorff
    /// </summary>
    public class DissimilarityFunctions
    {
        #region Euclidean distance Measurements

        /// <summary>
        /// Computes the Euclidean distance between two 3-D points.
        /// </summary>
        public static double EuclideanDistance(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        {
            return Math.Sqrt(SqrEuclideanDistance(a, b));
        }

        /// <summary>
        /// Computes the squared Euclidean distance between two 3-D points.
        /// </summary>
        public static double SqrEuclideanDistance(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        }

        #endregion

        #region Dissimilarity measures: Euclidean distance
        /// <summary>
        /// Computes the Euclidean distance between two body poses 
        /// as the sum of the (weighted) Euclidean distances between their corresponding joints.
        /// </summary>
        public static double EuclideanDistance(HandPose pose1, HandPose pose2, double[] jointWeights = null)
        {
            if (jointWeights == null)
                jointWeights = WeightsArray.GenerateUnitWeights(pose1.Joints.Length);

            if (jointWeights[0] != 1 || jointWeights[10] != 1 || jointWeights[pose1.Joints.Length - 1] != 1)
            {
                Console.WriteLine("Found different weights for joints: ");
            }

            // EDIT: This function calculates, (weighted) Euclidean distances.
            double d = 0;
            for (int i = 0; i < pose1.Joints.Length; i++)
                d += EuclideanDistance(pose1.Joints[i], pose2.Joints[i]);
            return d;
        }

        /// <summary>
        /// Computes the squared Euclidean distance between two body poses 
        /// as the sum of the (weighted) Euclidean distances between their corresponding joints.
        /// </summary>
        public static double SqrEuclideanDistance(HandPose pose1, HandPose pose2, double[] jointWeights = null)
        {
            if (jointWeights == null)
                jointWeights = WeightsArray.GenerateUnitWeights(pose1.Joints.Length);

            double d = 0;
            for (int i = 0; i < pose1.Joints.Length; i++)
                d += SqrEuclideanDistance(pose1.Joints[i], pose2.Joints[i]);
            return d;
        }

        #endregion

        /// <summary>
        /// Prototype definition of a function that computes the dissimilarity between two hand gestures.
        /// </summary>
        public delegate double GestureDissimilarity(Gesture gesture1, Gesture gesture2, double[] jointWeights = null);

        /// <summary>
        /// Computes the Euclidean distance between two whole-body gestures.
        /// </summary>
        public static double EuclideanDistance(Gesture gesture1, Gesture gesture2, double[] jointWeights = null)
        {
            int n = gesture1.HandPoses.Count;
            int m = gesture2.HandPoses.Count;
            if (n == 0 || m == 0) return 0;
            if (n != m)
                throw new ArgumentException(String.Format("The two gestures should have the same number of body poses in order to compute the Euclidean distance ({0} vs. {1} poses)", n, m));

            double d = 0;
            for (int i = 0; i < n; i++)
                d += EuclideanDistance(gesture1.HandPoses[i], gesture2.HandPoses[i], jointWeights);
            return d;
        }

        /// <summary>
        /// Computes the DTW distance between two whole-body gestures.
        /// </summary>
        public static double DTW(Gesture gesture1, Gesture gesture2, double[] jointWeights = null)
        {
            int n = gesture1.HandPoses.Count;
            int m = gesture2.HandPoses.Count;
            if (n == 0 || m == 0) return 0;

            double[,] cost = new double[n, m];

            cost[0, 0] = EuclideanDistance(gesture1.HandPoses[0], gesture2.HandPoses[0], jointWeights);
            for (int j = 1; j < m; j++)
                cost[0, j] = cost[0, j - 1] + EuclideanDistance(gesture1.HandPoses[0], gesture2.HandPoses[j], jointWeights);
            for (int i = 1; i < n; i++)
                cost[i, 0] = cost[i - 1, 0] + EuclideanDistance(gesture1.HandPoses[i], gesture2.HandPoses[0], jointWeights);

            for (int i = 1; i < n; i++)
                for (int j = 1; j < m; j++)
                {
                    double min = Math.Min(cost[i - 1, j - 1], Math.Min(cost[i - 1, j], cost[i, j - 1]));
                    cost[i, j] = min + EuclideanDistance(gesture1.HandPoses[i], gesture2.HandPoses[j], jointWeights);
                }
            return cost[n - 1, m - 1];
        }

        /// <summary>
        /// Computes the normalized DTW distance between two whole-body gestures.
        /// </summary>
        public static double NormalizedDTW(Gesture gesture1, Gesture gesture2, double[] jointWeights = null)
        {
            int n = gesture1.HandPoses.Count;
            int m = gesture2.HandPoses.Count;
            if (n == 0 || m == 0) return 0;

            double[,] cost = new double[n, m];
            int[,] length = new int[n, m];

            cost[0, 0] = EuclideanDistance(gesture1.HandPoses[0], gesture2.HandPoses[0], jointWeights);
            length[0, 0] = 1;
            for (int j = 1; j < m; j++)
            {
                cost[0, j] = cost[0, j - 1] + EuclideanDistance(gesture1.HandPoses[0], gesture2.HandPoses[j], jointWeights);
                length[0, j] = length[0, j - 1] + 1;
            }
            for (int i = 1; i < n; i++)
            {
                cost[i, 0] = cost[i - 1, 0] + EuclideanDistance(gesture1.HandPoses[i], gesture2.HandPoses[0], jointWeights);
                length[i, 0] = length[i - 1, 0] + 1;
            }

            for (int i = 1; i < n; i++)
                for (int j = 1; j < m; j++)
                {
                    double min = cost[i - 1, j - 1];
                    int l = length[i - 1, j - 1];

                    if (min > cost[i - 1, j])
                    {
                        min = cost[i - 1, j];
                        l = length[i - 1, j];
                    }

                    if (min > cost[i, j - 1])
                    {
                        min = cost[i, j - 1];
                        l = length[i, j - 1];
                    }

                    cost[i, j] = min + EuclideanDistance(gesture1.HandPoses[i], gesture2.HandPoses[j], jointWeights);
                    length[i, j] = l + 1;
                }
            return cost[n - 1, m - 1] / length[n - 1, m - 1];
        }

        /// <summary>
        /// Computes the Modified Hausdorff distance between two body gestures
        /// </summary>
        public static double ModifiedHausdorff(Gesture gesture1, Gesture gesture2, double[] jointWeights = null)
        {
            return Math.Max(ComputeModifiedHausdorff(gesture1, gesture2, jointWeights), ComputeModifiedHausdorff(gesture2, gesture1, jointWeights));
        }

        private static double ComputeModifiedHausdorff(Gesture gesture1, Gesture gesture2, double[] jointWeights = null)
        {
            int n = gesture1.HandPoses.Count;
            int m = gesture2.HandPoses.Count;
            if (n == 0 || m == 0) return 0;

            double avg = 0;
            for (int i = 0; i < n; i++)
            {
                double min = double.MaxValue;
                for (int j = 0; j < m; j++)
                    min = Math.Min(min, EuclideanDistance(gesture1.HandPoses[i], gesture2.HandPoses[j], jointWeights));
                avg += min;
            }
            avg /= n;
            return avg;
        }
    }

    class WeightsArray
    {
        /// <summary>
        /// Returns an array of n unit weights (1, 1, 1, ..., 1)
        /// </summary>
        public static double[] GenerateUnitWeights(int n)
        {
            double[] weights = new double[n];
            for (int i = 0; i < n; i++)
                weights[i] = 1.0;
            return weights;
        }

        /// <summary>
        /// Returns an array of n weights where only the two hand joints have non-zero values.
        /// </summary>
        //public static double[] GenerateHandsWeights(int n)
        //{
        //    double[] weights = new double[n];
        //    Array.Clear(weights, 0, weights.Length);

        //    weights[(int)JointType.HandLeft] = 1.0;
        //    weights[(int)JointType.HandRight] = 1.0;

        //    return weights;
        //}
    }
}
