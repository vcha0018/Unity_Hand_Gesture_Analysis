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
It is middle class to interact between UI and Analysis functions.
*/

using Analysis;
using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// It is Middleware Singleton class operate between UI components classes to Analysis classes.
/// </summary>
public class GestureProcessor
{
    /// <summary>
    /// To obtain read-write lock on singleton class.
    /// </summary>
    private static readonly object objLock = new object();
    /// <summary>
    /// gesture collection copy used for analysis purpose only.
    /// </summary>
    protected List<Person> _gestureAnalysisCollection;
    /// <summary>
    /// Holds already performed analysis operation on gesture type.
    /// </summary>
    private Dictionary<GestureTypeFormat, ComparisionResult> _resultSet;

    /// <summary>
    /// Set true, if want to re-intialize this singleton class.
    /// </summary>
    public static bool ReInitialize { get; set; }


    private static GestureProcessor _gestureProcessor;
    /// <summary>
    /// It is singleton object of the class.
    /// </summary>
    public static GestureProcessor Instance
    {
        get
        {
            lock (objLock)
            {
                if (_gestureProcessor == null || ReInitialize)
                    _gestureProcessor = new GestureProcessor();
                return _gestureProcessor;
            }
        }
    }

    /// <summary>
    /// Formatted Gesture collection from csv data.
    /// </summary>
    public List<Person> GestureCollection { get; private set; }

    /// <summary>
    /// Get total number of gestures in GestureCollection.
    /// </summary>
    public int TotalGestures
    {
        get
        {
            return GestureCollection.Sum(pitem => pitem.Gestures.Sum(gItem => gItem.Value.Count));
        }
    }

    /// <summary>
    /// Define gesture frame rate for analysis.
    /// </summary>
    public int FPS { get; set; }

    private GestureProcessor()
    {
        _resultSet = new Dictionary<GestureTypeFormat, ComparisionResult>();
        ReInitialize = false;
        GestureCollection = IO.LoadGesturesPersonWise();
        _gestureAnalysisCollection = (from item in GestureCollection select item.GetClone()).ToList();
        FPS = Constants.DEFAULT_FRAME_RATE;
        //NormalizeGesturesForAnalysis(false);
    }

    /// <summary>
    /// Runs the following normalization steps for whole-body gestures:
    ///     1. resample gesture (body poses will be uniformly distributed in time)
    ///     2. normalize height
    ///     3. translate to origin (body centroid = (0,0,0))
    /// </summary>
    private void NormalizeGesturesForAnalysis(bool useFixedNumberOfPoses)
    {
        foreach (Person person in _gestureAnalysisCollection)
            foreach (KeyValuePair<GestureTypeFormat, List<Gesture>> gestureItem in person.Gestures)
                foreach (Gesture gesture in gestureItem.Value)
                {
                    gesture.Resample(useFixedNumberOfPoses ? FPS : (int)((gesture.GetProductionTimeInMilliseconds() / 1000.0) * FPS));
                    gesture.NormalizeHeight();
                    gesture.TranslateToOrigin2();
                }
    }

    /// <summary>
    /// Get Consensus result between all person's gesture (same type) in the dataset.
    /// </summary>
    /// <param name="dissimilarityFunctionType">Specify which dissimilarity function want to use.</param>
    /// <param name="aggregationType">Specify which aggregation function want to use if one person has multiple data of same gesture type.</param>
    /// <param name="gestureType">Specify gesture type of person to compare.</param>
    /// <param name="customTolerance">Specify custom tolerance value if want to get tolerance of it.</param>
    /// <param name="graphScale">Specify range of values to calculate tolerance from zero to maximum possible tolerance.</param>
    /// <returns></returns>
    public ComparisionResult GetConsensusOfPersons(
        DissimilarityFunctionType dissimilarityFunctionType,
        AggregationType aggregationType,
        GestureTypeFormat gestureType,
        double customTolerance = -1,
        int graphScale = -1)
    {
        if (_resultSet.ContainsKey(gestureType))
            return _resultSet[gestureType];

        Analyzer.aggregatorFunction = GetAggregatorType(aggregationType);
        Analyzer.dissimilarityFunction = GetDissimilarityFunctionFromType(dissimilarityFunctionType);
        Analyzer.normalizationFactor = Constants.NUM_JOINTS;
        var result = Analyzer.GetConsensusBetweenPersons(
            ref _gestureAnalysisCollection,
            gestureType,
            toleranceDataPoints: graphScale,
            tolerance: (customTolerance > -1) ? Math.Round(customTolerance, 2) : -1).ElementAt(0);

        if (!_resultSet.ContainsKey(result.Key))
            _resultSet.Add(result.Key, result.Value);
        return result.Value;
    }

    /// <summary>
    /// Get Consensus result between gestures of same type for given person.
    /// </summary>
    /// <param name="dissimilarityFunctionType">Specify which dissimilarity function want to use.</param>
    /// <param name="aggregationType">Specify which aggregation function want to use if one person has multiple data of same gesture type.</param>
    /// <param name="personName">Specify person name.</param>
    /// <param name="gestureType">Specify gesture type of person to compare.</param>
    /// <param name="customTolerance">Specify custom tolerance value if want to get tolerance of it.</param>
    /// <returns></returns>
    public ComparisionResult GetConsensusOfPerson(
        DissimilarityFunctionType dissimilarityFunctionType,
        AggregationType aggregationType,
        string personName,
        GestureTypeFormat gestureType,
        double customTolerance = -1)
    {
        if (_resultSet.ContainsKey(gestureType))
            return _resultSet[gestureType];

        Analyzer.aggregatorFunction = GetAggregatorType(aggregationType);
        Analyzer.dissimilarityFunction = GetDissimilarityFunctionFromType(dissimilarityFunctionType);
        Analyzer.normalizationFactor = Constants.NUM_JOINTS;
        Person person = GestureCollection.Find(p => p.Name.Trim().ToLower().Equals(personName.Trim().ToLower()));
        if (person == null)
            throw new Exception("Cannot locate person from given person name!");
        var result = Analyzer.GetConsensusBetweenGestures(
            person,
            gestureType,
            toleranceDataPoints: 50,
            tolerance: (customTolerance > -1) ? customTolerance : -1).ElementAt(0);

        if (!_resultSet.ContainsKey(result.Key))
            _resultSet.Add(result.Key, result.Value);
        return result.Value;
    }

    /// <summary>
    /// To convert related enum type to dissimilarity function.
    /// </summary>
    /// <param name="dissimilarityFunctionType"></param>
    /// <returns></returns>
    private DissimilarityFunctions.GestureDissimilarity GetDissimilarityFunctionFromType(DissimilarityFunctionType dissimilarityFunctionType)
    {
        switch (dissimilarityFunctionType)
        {
            case DissimilarityFunctionType.EuclideanDistance:
                return DissimilarityFunctions.EuclideanDistance;
            case DissimilarityFunctionType.DTW:
                return DissimilarityFunctions.DTW;
            case DissimilarityFunctionType.NormalizedDTW:
                return DissimilarityFunctions.NormalizedDTW;
            case DissimilarityFunctionType.ModifiedHausdorff:
                return DissimilarityFunctions.ModifiedHausdorff;
            default:
                throw new Exception("No match found for given dissimilarity function type!");
        }
    }

    /// <summary>
    /// To convert related enum type to aggregation function.
    /// </summary>
    /// <param name="aggregationType"></param>
    /// <returns></returns>
    private Analyzer.Aggregator GetAggregatorType(AggregationType aggregationType)
    {
        switch (aggregationType)
        {
            case AggregationType.Average:
                return Analyzer.AverageAggregator;
            case AggregationType.Max:
                return Analyzer.MaxAggregator;
            case AggregationType.Min:
                return Analyzer.MinAggregator;
            default:
                throw new Exception("No match found for given aggregation type!");
        }
    }
}

/// <summary>
/// Holds gesture consensus result and dissimilarity result after comparision.
/// </summary>
public class ComparisionResult
{
    public List<Tuple<double, double>> toleranceConsensusPair;
    public double[,] dissimilarityMatric;

    /// <summary>
    /// Convert dissimilarity matric result to List of Tuples.
    /// Each item in list indicats unique comparision pair and its result.
    /// Items in Tuple includes:
    /// Item1: sample 1's gesture
    /// Item2: sample 2's gesture
    /// Item3: comparision result between sample 1 and sample 2 gesture.
    /// </summary>
    /// <returns></returns>
    public List<Tuple<int, int, double>> GetDissimilarityMatric()
    {
        List<Tuple<int, int, double>> matricPair = new List<Tuple<int, int, double>>();
        if (dissimilarityMatric.Length < 1)
            return matricPair;
        for (int i = 0; i < dissimilarityMatric.GetLength(0); i++)
        {
            for (int j = i + 1; j < dissimilarityMatric.GetLength(0); j++)
            {
                matricPair.Add(new Tuple<int, int, double>(i, j, Math.Round(dissimilarityMatric[i, j], 2)));
            }
        }
        return matricPair;
    }

    /// <summary>
    /// Get max tolerance value for 100% consensus.
    /// </summary>
    /// <returns></returns>
    public Tuple<double, double> GetHighestToleranceConsensusPair()
    {
        return new Tuple<double, double>(
            toleranceConsensusPair[toleranceConsensusPair.Count - 1].Item1, 
            toleranceConsensusPair[toleranceConsensusPair.Count - 1].Item2
            );
    }

    /// <summary>
    /// Get consensus value for given tolerance.
    /// </summary>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public double GetRelativeConsensus(double tolerance)
    {
        return Math.Round(toleranceConsensusPair.OrderBy(item => Math.Abs(tolerance - item.Item1)).First().Item2, 2);
    }
}
