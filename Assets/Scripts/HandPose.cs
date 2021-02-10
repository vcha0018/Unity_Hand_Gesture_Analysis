using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataStructure
{
    public class HandPose
    {
        private Vector3[] _joints;
        public Vector3[] Joints
        {
            get
            {
                return _joints;
            }
            set
            {
                if (value != null)
                    _joints = value;
                else
                    throw new ArgumentException("Joints cannot be null!");
            }
        }

        private int _timestamp;
        public int TimeStamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }
        public HandPose()
        {
            _joints = new Vector3[Constants.NUM_JOINTS];
            _timestamp = 0;
        }
    }
}
