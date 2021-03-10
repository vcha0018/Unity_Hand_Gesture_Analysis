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
Data structure to store Person detail.
*/

using System.Collections.Generic;
using System.Linq;

namespace DataStructure
{
    public class Person
    {
        private string _name;
        /// <summary>
        /// Person name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Dictionary<GestureTypeFormat, List<Gesture>> _gestures;
        /// <summary>
        /// List of person's different gestures.
        /// </summary>
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

        /// <summary>
        /// To make a clone object this Person object.
        /// </summary>
        /// <returns></returns>
        public Person GetClone()
        {
            return new Person()
            {
                Name = _name,
                Gestures = (from item in _gestures select 
                            new KeyValuePair<GestureTypeFormat, List<Gesture>>(
                                item.Key, 
                                (from childItem in item.Value select childItem.GetClone()).ToList())).ToDictionary(x => x.Key, x=> x.Value)
            };
        }
    }
}
