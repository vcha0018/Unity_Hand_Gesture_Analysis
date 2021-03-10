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
Data structure to store one Gesture frame with its joints.
*/

using System;
using UnityEngine;

namespace DataStructure
{
    public class HandPose
    {
        private Vector3[] _joints;
        /// <summary>
        /// List of 21 3D hand joints at specific time.
        /// </summary>
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

        private float _timestamp;
        /// <summary>
        /// Time frame number.
        /// </summary>
        public float TimeStamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }
        public HandPose()
        {
            _joints = new Vector3[Constants.NUM_JOINTS];
            _timestamp = 0;
        }

        /// <summary>
        /// To get clone of this handpose object.
        /// </summary>
        /// <returns></returns>
        public HandPose GetClone()
        {
            return new HandPose() { Joints = (Vector3[])Joints.Clone(), TimeStamp = TimeStamp };
        }

        /// <summary>
        /// Computes the bounding box of this hand pose.
        /// </summary>
        public Cuboid GetBoundingCuboid()
        {
            Cuboid cuboid = new Cuboid()
            {
                TopLeft = new Vector3() { x = float.MaxValue, y = float.MaxValue, z = float.MaxValue },
                BottomRight = new Vector3() { x = float.MinValue, y = float.MinValue, z = float.MinValue },
            };

            foreach (Vector3 point in Joints)
            {
                if (cuboid.TopLeft.x > point.x) cuboid.TopLeft.x = point.x;
                if (cuboid.TopLeft.y > point.y) cuboid.TopLeft.y = point.y;
                if (cuboid.TopLeft.z > point.z) cuboid.TopLeft.z = point.z;

                if (cuboid.BottomRight.x < point.x) cuboid.BottomRight.x = point.x;
                if (cuboid.BottomRight.y < point.y) cuboid.BottomRight.y = point.y;
                if (cuboid.BottomRight.z < point.z) cuboid.BottomRight.z = point.z;
            }

            return cuboid;
        }

        /// <summary>
        /// Computes the centroid of this hand pose.
        /// </summary>
        public Vector3 GetCentroid()
        {
            Vector3 cg = new Vector3() { x = 0, y = 0, z = 0 };

            if (_joints.Length > 0)
            {
                foreach (Vector3 point in Joints)
                {
                    cg.x += point.x;
                    cg.y += point.y;
                    cg.z += point.z;
                }
                cg.x /= Joints.Length;
                cg.y /= Joints.Length;
                cg.z /= Joints.Length;
            }

            return cg;
        }
    }
}
