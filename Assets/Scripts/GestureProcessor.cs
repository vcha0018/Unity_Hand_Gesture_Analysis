using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis;


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
    /// Get 
    /// </summary>
    /// <param name="dissimilarityFunctionType"></param>
    /// <param name="aggregationType"></param>
    /// <param name="gestureType"></param>
    /// <param name="customTolerance"></param>
    /// <param name="graphScale"></param>
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

public class ComparisionResult
{
    public List<Tuple<double, double>> toleranceConsensusPair;
    public double[,] dissimilarityMatric;

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

    public Tuple<double, double> GetHighestToleranceConsensusPair()
    {
        return new Tuple<double, double>(
            toleranceConsensusPair[toleranceConsensusPair.Count - 1].Item1, 
            toleranceConsensusPair[toleranceConsensusPair.Count - 1].Item2
            );
    }

    public double GetRelativeConsensus(double tolerance)
    {
        return Math.Round(toleranceConsensusPair.OrderBy(item => Math.Abs(tolerance - item.Item1)).First().Item2, 2);
    }
}
