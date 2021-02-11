using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataStructure
{
    public class Gesture
    {
        /// <summary>
        /// The data will have a bounding box of size (300,300,35) after re-scaling
        /// </summary>
        private Vector3 _rescaleReference = new Vector3(300, 300, 35);
        private float _csv_current_timestamp = float.NaN;
        private float _csv_previous_timestamp = float.NaN;
        private int _currentHandPoseIndex = 0;
        private float _timer = 0f;
        private Transform[] _jointTransforms;
        private bool isInInitialStage = true;

        private BoundingBox _boundingBox;
        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        public HandTypeFormat HandType { get; set; }

        private List<HandPose> _handPoses;
        public List<HandPose> HandPoses
        {
            get
            {
                return _handPoses;
            }
            set
            {
                if (value != null && value.Count > 0)
                    _handPoses = value;
                else
                    throw new ArgumentException("HandPoses cannot be null or empty!");
            }
        }

        public GameObject HandModel { get; set; }

        public string Tag { get; set; }

        private Vector3 _positionFactor = new Vector3(0, 0, 0);
        public Vector3 PositionFactor
        {
            get
            {
                return _positionFactor;
            }
            set
            {
                if (value != null)
                    _positionFactor = value;
                else
                    throw new ArgumentException("Position Factor cannot be null.");
            }
        }

        private Vector3 _centroid;
        public Vector3 Centroid
        {
            get
            {
                return _centroid;
            }
        }

        public Gesture()
        {
            _handPoses = new List<HandPose>();
        }

        public Gesture(HandTypeFormat handType, List<HandPose> handPoses)
        {
            HandType = handType;
            _handPoses = handPoses;
        }

        private void SetBoundingBox()
        {
            _boundingBox = new BoundingBox();
            foreach (HandPose pose in HandPoses)
            {
                foreach (Vector3 coordinate in pose.Joints)
                {
                    if (coordinate.x > _boundingBox.maxX)
                    {
                        _boundingBox.maxX = coordinate.x;
                    }
                    if (coordinate.x < _boundingBox.minX)
                    {
                        _boundingBox.minX = coordinate.x;
                    }
                    if (coordinate.y > _boundingBox.maxY)
                    {
                        _boundingBox.maxY = coordinate.y;
                    }
                    if (coordinate.y < _boundingBox.minY)
                    {
                        _boundingBox.minY = coordinate.y;
                    }
                    if (coordinate.z > _boundingBox.maxZ)
                    {
                        _boundingBox.maxZ = coordinate.z;
                    }
                    if (coordinate.z < _boundingBox.minZ)
                    {
                        _boundingBox.minZ = coordinate.z;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the centroid for the recorded gesture based on its bounding box
        /// </summary>
        private void SetCentroid()
        {
            Vector3 size = GetBoundingBoxSize();
            _centroid = new Vector3(size.x / 2 + _boundingBox.minX, size.y / 2 + _boundingBox.minY, size.z / 2 + _boundingBox.minZ);
        }

        /// <summary>
        /// Translates the gesture so that its centroid becomes (0, 0, 0).
        /// </summary>
        private void TranslateToOrigin()
        {
            foreach (HandPose pose in HandPoses)
            {
                for (int i = 0; i < pose.Joints.Length; i++)
                {
                    pose.Joints[i].x -= _centroid.x;
                    pose.Joints[i].y -= _centroid.y;
                    pose.Joints[i].z -= _centroid.z;
                }
            }
            SetBoundingBox();
            SetCentroid();
        }

        /// <summary>
        /// Rescales the recorded gesture based on rescaleReference.
        /// Z coordinate is commented out because the data recorded does not reflect the acutal movement of hand along z-axis.
        /// uncomment rescale for z coordinate if this is fixed in the future.
        /// </summary>
        private void Rescale()
        {
            Vector3 size = GetBoundingBoxSize();
            float xScale = _rescaleReference.x / size.x;
            float yScale = _rescaleReference.y / size.y;
            //float zScale = size.z / rescaleReference.z;

            foreach (HandPose pose in HandPoses)
            {
                for (int i = 0; i < pose.Joints.Length; i++)
                {
                    pose.Joints[i].x = pose.Joints[i].x * xScale;
                    pose.Joints[i].y = pose.Joints[i].y * yScale;
                    //data.jointsCoordinate[i].z = data.jointsCoordinate[i].z * zScale;
                }
            }
            SetBoundingBox();
            SetCentroid();
        }

        /// <summary>
        /// Calculates and returns the size of the bounding box.
        /// </summary>
        public Vector3 GetBoundingBoxSize()
        {
            return new Vector3(_boundingBox.maxX - _boundingBox.minX, _boundingBox.maxY - _boundingBox.minY, _boundingBox.maxZ - _boundingBox.minZ);
        }

        public bool NormalizeGesture()
        {
            SetBoundingBox();
            SetCentroid();
            TranslateToOrigin();
            Rescale();
            return false;
        }

        public void TransformJoints()
        {
            // CHECK 1
            _csv_current_timestamp = 0;
            _csv_previous_timestamp = 0;
            _timer = 0f;
            _currentHandPoseIndex++;
            _jointTransforms = HandModel.GetComponentsInChildren<Transform>();
            _jointTransforms[0].localPosition = new Vector3(0, 0, 0);
            for (int i = 1; i < _jointTransforms.Length; i++)
            {
                _jointTransforms[i].localPosition = HandPoses[0].Joints[i - 1] + _positionFactor;
            }
        }

        public void Reset()
        {
            _jointTransforms = HandModel.GetComponentsInChildren<Transform>();
            _timer = 0f;
            _csv_current_timestamp = HandPoses[0].TimeStamp;
            // CHECK 2
            _currentHandPoseIndex = 1;
            isInInitialStage = true;
            _currentHandPoseIndex++;
        }

        /// <summary>
        /// Called in Update(), update position of the hand model by regularly tracking the system timer with the current csv_time. 
        /// Enables the animation to loop.
        /// </summary>
        public void ConditionCheck()
        {
            float timer = _timer * 1000;
            if (timer > (HandPoses.Last().TimeStamp - HandPoses[0].TimeStamp))
                this.Reset();
            if (timer > _csv_current_timestamp)
            {
                _csv_previous_timestamp = _csv_current_timestamp;
                _currentHandPoseIndex++;
                _csv_current_timestamp = HandPoses[_currentHandPoseIndex].TimeStamp - HandPoses[0].TimeStamp;
                //csv_coordinates = processed_data[row_count].jointsCoordinate;
            }
        }

        /// <summary>
        /// Animates the gesture in the scene based on given .csv file.
        /// </summary>
        public void AnimateInAnimationMode()
        {
            if (isInInitialStage)
                isInInitialStage = false;
            else
                _timer += Time.deltaTime;

            if (_jointTransforms != null && _jointTransforms.Length > 2 && _csv_previous_timestamp < _csv_current_timestamp)
            {
                for (int i = 1; i < _jointTransforms.Length; i++)
                {
                    Vector3 start_pos = _jointTransforms[i].localPosition;
                    Vector3 end_pos = HandPoses[_currentHandPoseIndex].Joints[i - 1] + _positionFactor;

                    float ratio = ((_timer * 1000) - _csv_previous_timestamp) / (_csv_current_timestamp - _csv_previous_timestamp);
                    _jointTransforms[i].localPosition = Vector3.Lerp(start_pos, end_pos, ratio);
                }
            }
        }
    }

    //public class Gesture
    //{
    //    private List<HandPose> _leftHandPoses;
    //    public List<HandPose> LeftHandPoses
    //    {
    //        get
    //        {
    //            return _leftHandPoses;
    //        }
    //        set
    //        {
    //            if (value != null && value.Count > 0)
    //                _leftHandPoses = value;
    //            else
    //                throw new ArgumentException("HandPoses cannot be null or empty!");
    //        }
    //    }

    //    private List<HandPose> _rightHandPoses;
    //    public List<HandPose> RightHandPoses
    //    {
    //        get
    //        {
    //            return _rightHandPoses;
    //        }
    //        set
    //        {
    //            if (value != null && value.Count > 0)
    //                _rightHandPoses = value;
    //            else
    //                throw new ArgumentException("HandPoses cannot be null or empty!");
    //        }
    //    }

    //    public HandTypeFormat DominateHandType { get; set; }

    //    public Gesture()
    //    {
    //        _leftHandPoses = new List<HandPose>();
    //        _rightHandPoses = new List<HandPose>();
    //    }

    //    public List<HandPose> GetDominateHand(bool emptyFiltered = true)
    //    {
    //        List<HandPose> dominate_hand = new List<HandPose>();
    //        int maxCount = LeftHandPoses.Count > RightHandPoses.Count ? LeftHandPoses.Count : RightHandPoses.Count;
    //        int leftBucket = 0;
    //        int rightBucket = 0;
    //        for (int i = 0; i < maxCount; i++)
    //        {
    //            if (LeftHandPoses.Count <= i)
    //            {
    //                rightBucket += (RightHandPoses.Count - i);
    //                break;
    //            }
    //            if (RightHandPoses.Count <= i)
    //            {
    //                leftBucket += (LeftHandPoses.Count - i);
    //                break;
    //            }
    //            if (LeftHandPoses[i].Joints.Length > RightHandPoses[i].Joints.Length)
    //                leftBucket++;
    //            else
    //                rightBucket++;
    //        }
    //        dominate_hand = leftBucket > rightBucket ? LeftHandPoses : RightHandPoses;
    //        if (emptyFiltered)
    //            dominate_hand.RemoveAll(item => item.Joints.Length < 1);
    //        return dominate_hand;
    //    }
    //}
}
