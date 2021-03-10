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
This class used to detect mouse hover events on gesture view.
*/

using TMPro;
using UnityEngine;

/// <summary>
/// It is Transparent Cube component, used to detect mouse events
/// </summary>
public class CubeController : MonoBehaviour
{
    public string gestureName;
    public TMP_Text gestureLabel;
    private Color[] defaultJointColors;

    // Start is called before the first frame update
    void Start()
    {
        defaultJointColors = new Color[Constants.NUM_JOINTS];
    }

    /// <summary>
    /// Display Gesture information on mouse over HandModel view.
    /// </summary>
    private void OnMouseEnter()
    {
        gestureLabel.text = gestureName;
        var jointsObject = this.transform.parent.Find("Joints");
        for (int i = 0; i < jointsObject.childCount; i++)
        {
            defaultJointColors[i] = jointsObject.GetChild(i).GetComponent<Renderer>().material.color;
            jointsObject.GetChild(i).GetComponent<Renderer>().material.color = Color.red;
        }
    }

    /// <summary>
    /// Hide Gesture information on mouse leave from HandModel view.
    /// </summary>
    private void OnMouseExit()
    {
        gestureLabel.text = string.Empty;
        var jointsObject = this.transform.parent.Find("Joints");
        for (int i = 0; i < jointsObject.childCount; i++)
            jointsObject.GetChild(i).GetComponent<Renderer>().material.color = defaultJointColors[i];
    }
}
