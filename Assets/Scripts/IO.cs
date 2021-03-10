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
Read gesture dataset from csv format to custom object type.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DataStructure
{
    public class IO
    {
        /// <summary>
        /// Load gestures dataset from directory and parse into Person object type.
        /// </summary>
        /// <param name="processBothHads">Specify to read both hand type or not.</param>
        /// <returns></returns>
        public static List<Person> LoadGesturesPersonWise(bool processBothHads = false)
        {
            List<Person> person_gestures = new List<Person>();
            foreach (string dir_path in Constants.GESTURE_DIR_PATHS)
            {
                if (Directory.Exists(dir_path))
                {
                    string dir_name = dir_path.Trim('\\').Substring(dir_path.Trim('\\').LastIndexOf('\\') + 1).Trim().ToLower();
                    Person currentPerson = null;
                    if (person_gestures.Any(item => item.Name == dir_name))
                        currentPerson = person_gestures.Find(item => item.Name == dir_name);
                    else
                        currentPerson = new Person() { Name = dir_name };
                    List<string> files = Directory.GetFiles(dir_path).ToList();
                    if (files.Count > 1)
                    {
                        files = files.OrderBy(item => item.Substring(item.LastIndexOf("#"))).ToList();
                        Dictionary<GestureTypeFormat, List<Gesture>> gestures = new Dictionary<GestureTypeFormat, List<Gesture>>();
                        for (int j = 0; j < files.Count; j += 2)
                        {
                            if (File.Exists(files[j]) && File.Exists(files[j + 1]))
                            {
                                FileInfo f1 = new FileInfo(files[j]);
                                FileInfo f2 = new FileInfo(files[j + 1]);
                                FileInfo f = (f1.Length > f2.Length) ? f1 : f2;
                                GestureTypeFormat addedGestureType = GestureTypeFormat.None;
                                if (!processBothHads)
                                    addedGestureType = AddSingleHand(ref gestures, ref f);
                                else
                                    addedGestureType = AddBothHands(ref gestures, ref f1, ref f2);
                            }
                        }
                        currentPerson.Gestures = gestures;
                        person_gestures.Add(currentPerson);
                    }
                }
            }
            return person_gestures;
        }

        /// <summary>
        /// To adding single hand to gesture dataset.
        /// </summary>
        /// <param name="gestures"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private static GestureTypeFormat AddSingleHand(ref Dictionary<GestureTypeFormat, List<Gesture>> gestures, ref FileInfo fileInfo)
        {
            string gkey = fileInfo.Name.Substring(fileInfo.Name.IndexOf("_") + 1, fileInfo.Name.IndexOf('#') - fileInfo.Name.IndexOf("_") - 1);
            string htype = fileInfo.Name.Substring(fileInfo.Name.IndexOf("#") + 1, fileInfo.Name.LastIndexOf('#') - fileInfo.Name.IndexOf("#") - 1);
            GestureTypeFormat ges_key = (GestureTypeFormat)Enum.Parse(typeof(GestureTypeFormat), gkey, true);
            HandTypeFormat handtype = htype.ToUpper()[0] == 'L' ? HandTypeFormat.LEFT : HandTypeFormat.RIGHT;
            if (gestures.ContainsKey(ges_key))
                gestures[ges_key].Add(new Gesture(handtype, ParseHandText(File.ReadAllText(fileInfo.FullName))));
            else
                gestures.Add(ges_key, new List<Gesture>()
                {
                    new Gesture(handtype, ParseHandText(File.ReadAllText(fileInfo.FullName)))
                });
            return ges_key;
        }

        /// <summary>
        /// To adding both hands to gesture dataset.
        /// </summary>
        /// <param name="gestures"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        private static GestureTypeFormat AddBothHands(ref Dictionary<GestureTypeFormat, List<Gesture>> gestures, ref FileInfo f1, ref FileInfo f2)
        {
            string f1_gkey = f1.Name.Substring(f1.Name.IndexOf("_") + 1, f1.Name.IndexOf('#') - f1.Name.IndexOf("_") - 1);
            string f2_gkey = f2.Name.Substring(f2.Name.IndexOf("_") + 1, f2.Name.IndexOf('#') - f2.Name.IndexOf("_") - 1);
            if (f1_gkey == f2_gkey)
            {
                string left_hand_text = string.Empty;
                string right_hand_text = string.Empty;
                if (f1.Name.Substring(f1.Name.IndexOf("#") + 1, f1.Name.LastIndexOf('#') - f1.Name.IndexOf("#") - 1)[0] == 'L')
                {
                    left_hand_text = File.ReadAllText(f1.FullName);
                    right_hand_text = File.ReadAllText(f2.FullName);
                }
                else
                {
                    right_hand_text = File.ReadAllText(f1.FullName);
                    left_hand_text = File.ReadAllText(f2.FullName);
                }
                GestureTypeFormat ges_key = (GestureTypeFormat)Enum.Parse(typeof(GestureTypeFormat), f1_gkey, true);
                if (gestures.ContainsKey(ges_key))
                {
                    gestures[ges_key].Add(new Gesture(HandTypeFormat.LEFT, ParseHandText(left_hand_text)));
                    gestures[ges_key].Add(new Gesture(HandTypeFormat.RIGHT, ParseHandText(right_hand_text)));
                }
                else
                {
                    gestures.Add(ges_key, new List<Gesture>()
                    {
                        new Gesture(HandTypeFormat.LEFT, ParseHandText(left_hand_text)),
                        new Gesture(HandTypeFormat.RIGHT, ParseHandText(right_hand_text))
                    });
                }
                return ges_key;
            }
            else
                throw new Exception("Mismatch information found on gesture type while reading.");
        }

        /// <summary>
        /// Parse gesture file to list of handposes.
        /// </summary>
        /// <param name="text">csv text</param>
        /// <returns></returns>
        private static List<HandPose> ParseHandText(string text)
        {
            List<HandPose> handPoses = new List<HandPose>();
            List<string> lines = text.Split('\n').ToList();
            lines.RemoveAt(0);
            lines.RemoveAt(lines.Count - 1);
            foreach (string row in lines)
            {
                List<string> points = row.Split(',').ToList();
                HandPose handPose = new HandPose();
                handPose.TimeStamp = int.Parse(points[0]);
                points.RemoveAt(0);
                if (points.Count != 63)
                    continue;
                List<Vector3> joints = new List<Vector3>();
                for (int i = 0; i < points.Count; i += 3)
                {
                    joints.Add(
                        new Vector3(
                            float.Parse(points[i].Trim()),
                            float.Parse(points[i + 1].Trim()),
                            float.Parse(points[i + 2].Trim())
                            ));
                }
                handPose.Joints = joints.ToArray();
                handPoses.Add(handPose);
            }
            return handPoses;
        }

    }
}
