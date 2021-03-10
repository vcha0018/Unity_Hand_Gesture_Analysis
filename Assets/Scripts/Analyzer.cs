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
Perform Analysis on gesture set with different dissimilarity functions and other custom configurations.
*/

using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public class Analyzer
    {
        #region Properties
        internal static DissimilarityFunctions.GestureDissimilarity dissimilarityFunction;
        internal static Aggregator aggregatorFunction;
        internal static double[] jointWeights;
        internal static double normalizationFactor;
        #endregion

        #region Aggregators
        /// <summary>
        /// Aggregator functions: min, max, average
        /// </summary>
        internal delegate double Aggregator(List<double> values);
        internal static double MinAggregator(List<double> values) { return values.Min(); }
        internal static double MaxAggregator(List<double> values) { return values.Max(); }
        internal static double AverageAggregator(List<double> values) { return values.Average(); }
        #endregion

        /// <summary>
        /// Computes consensus based on a dissimilarity matrix and a tolerance value t.
        /// </summary>
        /// <param name="dissimilarity"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static double Consensus(double[,] dissimilarity, double t)
        {
            int n = dissimilarity.GetLength(0);

            double consensus = 0;
            double count = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (dissimilarity[i, j] != double.MinValue)
                    {
                        if (dissimilarity[i, j] <= t)
                            consensus++;
                        count++;
                    }
            consensus = consensus / count * 100.0;

            return consensus;
        }

        /// <summary>
        /// Build tolerance-consensus pair list based on data points.
        /// </summary>
        /// <param name="toleranceDataPoints">number of pairs in the list equals to number of data points.</param>
        /// <param name="dissimilarityMatric">calculated dissimilarity matrix.</param>
        /// <param name="defaultTolerance">to retrive only one consensus based on default tolerance if any.</param>
        /// <returns></returns>
        private static List<Tuple<double, double>> GetToleranceConsensusPair(
            int toleranceDataPoints,
            double[,] dissimilarityMatric,
            double defaultTolerance = -1)
        {
            if (defaultTolerance <= 1.0)
            {
                if (toleranceDataPoints < 0)
                {
                    defaultTolerance = (defaultTolerance == 1) ? dissimilarityMatric.Cast<double>().Max() * 1.01 : defaultTolerance;
                    return new List<Tuple<double, double>>()
                    {
                        new Tuple<double, double>(defaultTolerance, Consensus(dissimilarityMatric, defaultTolerance))
                    };
                }
                else
                {
                    double maxT = dissimilarityMatric.Cast<double>().Max();
                    double step = maxT / (toleranceDataPoints - 1);
                    List<Tuple<double, double>> tcPair = new List<Tuple<double, double>>();
                    for (double t = 0; t < maxT; t += step)
                        tcPair.Add(new Tuple<double, double>(t, Consensus(dissimilarityMatric, t)));
                    tcPair.Add(new Tuple<double, double>(maxT * 1.01, Consensus(dissimilarityMatric, maxT * 1.01)));
                    return tcPair;
                }
            }
            throw new Exception("Custom tolerance value is not in range (0 to 1).");
        }

        /// <summary>
        /// To find consensus between persons with simillar gesture types.
        /// </summary>
        /// <param name="persons">Person list.</param>
        /// <param name="gestureType">Gesture type to compare. Default is None: indicate to compare for all gesture types.</param>
        /// <param name="handType">Hand Type to compare. Default is Left hand.</param>
        /// <param name="toleranceDataPoints">To retrive list of consensus-tolerance pair with data points.</param>
        /// <param name="tolerance">Find only one consensus for custom tolerance if any.</param>
        /// <returns></returns>
        internal static Dictionary<Tuple<GestureTypeFormat, HandTypeFormat>, ComparisionResult> GetConsensusBetweenPersons(
            ref List<Person> persons,
            GestureTypeFormat gestureType = GestureTypeFormat.None,
            HandTypeFormat handType = HandTypeFormat.LEFT,
            int toleranceDataPoints = -1,
            double tolerance = -1)
        {
            Dictionary<Tuple<GestureTypeFormat, HandTypeFormat>, ComparisionResult> result = new Dictionary<Tuple<GestureTypeFormat, HandTypeFormat>, ComparisionResult>();
            // list of all gesture types
            List<GestureTypeFormat> GESTURE_TYPES = (gestureType != GestureTypeFormat.None)
                ? new List<GestureTypeFormat>() { gestureType }
                : persons.
                    Select(pItem =>
                        pItem.Gestures.
                            Select(gItem => gItem.Key).
                            Distinct()).
                    SelectMany(x => x).
                    Distinct().
                    ToList();
            foreach (GestureTypeFormat gType in GESTURE_TYPES)
            {
                double[,] disMatric = GetDissimilarityMatric(persons, gType, handType);
                result.Add(
                    new Tuple<GestureTypeFormat, HandTypeFormat>(gType, handType),
                    new ComparisionResult()
                    {
                        dissimilarityMatric = disMatric,
                        toleranceConsensusPair = GetToleranceConsensusPair(toleranceDataPoints, disMatric, tolerance)
                    });
            }
            return result;
        }

        /// <summary>
        /// To find consensus between gestures of same type of a person.
        /// </summary>
        /// <param name="person">Person object.</param>
        /// <param name="gestureType">Gesture type to compare. Default is None: indicate to compare for all gesture types.</param>
        /// <param name="handType">Hand Type to compare. Default is Left hand.</param>
        /// <param name="toleranceDataPoints">To retrive list of consensus-tolerance pair with data points.</param>
        /// <param name="tolerance">Find only one consensus for custom tolerance if any.</param>
        /// <returns></returns>
        internal static Dictionary<Tuple<GestureTypeFormat, HandTypeFormat>, ComparisionResult> GetConsensusBetweenGestures(
            Person person,
            GestureTypeFormat gestureType = GestureTypeFormat.None,
            HandTypeFormat handType = HandTypeFormat.LEFT,
            int toleranceDataPoints = -1,
            double tolerance = -1)
        {
            int counter = 0;
            List<Person> persons = (from item in person.Gestures
                                    select (from gesture in item.Value
                                            select new Person()
                                            {
                                                Name = string.Format("{0}_{1}", person.Name, counter++),
                                                Gestures = new Dictionary<GestureTypeFormat, List<Gesture>>()
                                                {
                                                    {
                                                        item.Key, 
                                                        new List<Gesture>() 
                                                        {
                                                            gesture 
                                                        } 
                                                    }
                                                }
                                            }).ToList()).SelectMany(item => item).ToList();
            return GetConsensusBetweenPersons(ref persons, gestureType, handType, toleranceDataPoints, tolerance);
        }

        /// <summary>
        /// Retrive dissimilarity matrix from comparision of gestures of same type and hand but of different person.
        /// </summary>
        /// <param name="persons">Person list.</param>
        /// <param name="gestureType">Gesture type to compare.</param>
        /// <param name="handType">Hand type to compare.</param>
        /// <returns></returns>
        internal static double[,] GetDissimilarityMatric(List<Person> persons, GestureTypeFormat gestureType, HandTypeFormat handType)
        {
            // compute dissimilarity matrix for the current gesture type
            double[,] dissimilarityMatric = new double[persons.Count, persons.Count];
            for (int i = 0; i < persons.Count; i++)
            {
                for (int j = i + 1; j < persons.Count; j++)
                {
                    double d = 0;
                    if (persons[i].Gestures.Count == 0 || persons[j].Gestures.Count == 0)
                        d = double.MinValue;    // no data for this gesture type for participants i and j
                    else
                    {
                        d = GetDissimilarity(persons[i], persons[j], gestureType, handType);
                    }
                    dissimilarityMatric[i, j] = d;
                    dissimilarityMatric[j, i] = d;
                }
            }
            return dissimilarityMatric;
        }

        /// <summary>
        /// Get aggregated dissimilarity value after comparision of same gestures from two people with same hand type.
        /// </summary>
        /// <param name="person1">Person 1 object.</param>
        /// <param name="person2">Person 2 object.</param>
        /// <param name="gestureType">Gesture type to compare.</param>
        /// <param name="handType">Hand type to compare.</param>
        /// <returns></returns>
        internal static double GetDissimilarity(Person person1, Person person2, GestureTypeFormat gestureType, HandTypeFormat handType)
        {
            List<double> values = new List<double>();
            foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem1 in person1.Gestures)
            {
                if (gestureItem1.Key == gestureType)
                    foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem2 in person2.Gestures)
                    {
                        if (gestureItem2.Key == gestureType)
                            values = GetDissimilarity(gestureItem1.Value, gestureItem2.Value, handType);
                    }
            }
            return aggregatorFunction(values);
        }

        /// <summary>
        /// Get list of dissimilarity after comparing list of gestures from two different people.
        /// </summary>
        /// <param name="gestureList1">Person 1's gestures.</param>
        /// <param name="gestureList2">Person 2's gestures.</param>
        /// <param name="handType">Hand type to compare.</param>
        /// <returns></returns>
        internal static List<double> GetDissimilarity(List<Gesture> gestureList1, List<Gesture> gestureList2, HandTypeFormat handType)
        {
            List<double> values = new List<double>();
            foreach (Gesture gesture1 in gestureList1)
                if (gesture1.HandType == handType)
                    foreach (Gesture gesture2 in gestureList2)
                        if (gesture2.HandType == handType)
                            values.Add(dissimilarityFunction(gesture1, gesture2, jointWeights) / normalizationFactor);

            return values;
        }
    }
}
