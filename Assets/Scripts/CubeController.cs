using System.Collections;
using System.Collections.Generic;
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

    // Update is called once per frame
    void Update()
    {
        
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
