using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static Dictionary<double, double> GetToleranceConsensusPair(
            int toleranceDataPoints,
            double[,] dissimilarityMatric,
            double defaultTolerance = -1)
        {
            if (toleranceDataPoints < 0)
            {
                defaultTolerance = (defaultTolerance < 0) ? dissimilarityMatric.Cast<double>().Max() * 1.01 : defaultTolerance;
                return new Dictionary<double, double>()
                    { { defaultTolerance, Consensus(dissimilarityMatric, defaultTolerance) } };
            }
            else
            {
                double maxT = dissimilarityMatric.Cast<double>().Max();
                double step = maxT / (toleranceDataPoints - 1);
                Dictionary<double, double> tcPair = new Dictionary<double, double>();
                for (double t = 0; t < maxT; t += step)
                    tcPair.Add(t, Consensus(dissimilarityMatric, t));
                tcPair.Add(maxT * 1.01, Consensus(dissimilarityMatric, maxT * 1.01));
                return tcPair;
            }
        }

        internal static Dictionary<GestureTypeFormat, Dictionary<double, double>> GetConsensus(
            ComparisionTypeFormat comparisionType,
            GestureTypeFormat gestureType = GestureTypeFormat.None,
            HandTypeFormat handType = HandTypeFormat.LEFT,
            int toleranceDataPoints = -1,
            double tolerance = -1)
        {
            Dictionary<GestureTypeFormat, Dictionary<double, double>> consensusCollection = new Dictionary<GestureTypeFormat, Dictionary<double, double>>();
            double[,] dissimilarityMatric;
            switch (comparisionType)
            {
                case ComparisionTypeFormat.PersonWise:
                    // list of all gesture types
                    List<GestureTypeFormat> GESTURE_TYPES = GestureProcessor.Instance.
                        GestureCollection.
                            Select(pItem =>
                                pItem.Gestures.
                                    Select(gItem => gItem.Key).
                                    Distinct()).
                            SelectMany(x => x).
                            Distinct().
                            ToList();
                    foreach (GestureTypeFormat gType in GESTURE_TYPES)
                    {
                        dissimilarityMatric = GetDissimilarityMatric(GestureProcessor.Instance.GestureCollection, gType, handType);
                        consensusCollection.Add(gType, GetToleranceConsensusPair(toleranceDataPoints, dissimilarityMatric, tolerance));
                    }
                    break;
                case ComparisionTypeFormat.GestureWise:
                    if (gestureType == GestureTypeFormat.None)
                        throw new ArgumentException("To get consensus, gesture type cannot be 'None' type.");
                    dissimilarityMatric = GetDissimilarityMatric(GestureProcessor.Instance.GestureCollection, gestureType, handType);
                    consensusCollection.Add(gestureType, GetToleranceConsensusPair(toleranceDataPoints, dissimilarityMatric, tolerance));
                    break;
            }
            return consensusCollection;
        }

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
