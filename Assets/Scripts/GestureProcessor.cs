using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis;

public sealed class GestureProcessor
{
    private static readonly object objLock = new object();

    public static bool ReInitialize { get; set; }

    private static GestureProcessor _gestureProcessor;
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
    private GestureProcessor()
    {
        ReInitialize = false;
        GestureCollection = IO.LoadGesturesPersonWise();
    }

    public KeyValuePair<double, double> GetConsensusByGestureType(GestureTypeFormat gestureType, double customTolerance = -1)
    {
        Analyzer.aggregatorFunction = Analyzer.AverageAggregator;
        Analyzer.dissimilarityFunction = DissimilarityFunctions.NormalizedDTW;
        Analyzer.normalizationFactor = Constants.NUM_JOINTS;
        return Analyzer.GetConsensus(
            ComparisionTypeFormat.GestureWise, 
            gestureType, 
            tolerance:(customTolerance > -1) ? customTolerance : -1).Values.ElementAt(0).ElementAt(0);
    }
}
