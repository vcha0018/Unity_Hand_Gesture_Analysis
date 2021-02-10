using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private GestureProcessor()
    {
        ReInitialize = false;
        GestureCollection = IO.LoadGesturesPersonWise();
    }
}
