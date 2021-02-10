using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class Person
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Dictionary<GestureTypeFormat, List<Gesture>> _gestures;
        public Dictionary<GestureTypeFormat, List<Gesture>> Gestures
        {
            get { return _gestures; }
            set { _gestures = value; }
        }

        public Person()
        {
            _name = string.Empty;
            _gestures = new Dictionary<GestureTypeFormat, List<Gesture>>();
        }
    }
}
