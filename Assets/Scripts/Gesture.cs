using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class Gesture
    {
        private List<HandPose> _leftHandPoses;
        public List<HandPose> LeftHandPoses
        {
            get
            {
                return _leftHandPoses;
            }
            set
            {
                if (value != null && value.Count > 0)
                    _leftHandPoses = value;
                else
                    throw new ArgumentException("HandPoses cannot be null or empty!");
            }
        }

        private List<HandPose> _rightHandPoses;
        public List<HandPose> RightHandPoses
        {
            get
            {
                return _rightHandPoses;
            }
            set
            {
                if (value != null && value.Count > 0)
                    _rightHandPoses = value;
                else
                    throw new ArgumentException("HandPoses cannot be null or empty!");
            }
        }

        public Gesture()
        {
            _leftHandPoses = new List<HandPose>();
            _rightHandPoses = new List<HandPose>();
        }

        public List<HandPose> GetDominateHand(bool emptyFiltered = true)
        {
            List<HandPose> dominate_hand = new List<HandPose>();
            int maxCount = LeftHandPoses.Count > RightHandPoses.Count ? LeftHandPoses.Count : RightHandPoses.Count;
            int leftBucket = 0;
            int rightBucket = 0;
            for (int i = 0; i < maxCount; i++)
            {
                if (LeftHandPoses.Count <= i)
                {
                    rightBucket += (RightHandPoses.Count - i);
                    break;
                }
                if (RightHandPoses.Count <= i)
                {
                    leftBucket += (LeftHandPoses.Count - i);
                    break;
                }
                if (LeftHandPoses[i].Joints.Length > RightHandPoses[i].Joints.Length)
                    leftBucket++;
                else
                    rightBucket++;
            }
            dominate_hand = leftBucket > rightBucket ? LeftHandPoses : RightHandPoses;
            if (emptyFiltered)
                dominate_hand.RemoveAll(item => item.Joints.Length < 1);
            return dominate_hand;
        }
    }
}
