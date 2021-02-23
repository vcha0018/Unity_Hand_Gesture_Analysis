using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    private void OnMouseEnter()
    {
        gestureLabel.text = gestureName;
        for (int i = 1; i < this.transform.parent.childCount; i++)
        {
            defaultJointColors[i - 1] = this.transform.parent.GetChild(i).GetComponent<Renderer>().material.color;
            this.transform.parent.GetChild(i).GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void OnMouseExit()
    {
        gestureLabel.text = string.Empty;
        for (int i = 1; i < this.transform.parent.childCount; i++)
        {
            this.transform.parent.GetChild(i).GetComponent<Renderer>().material.color = defaultJointColors[i - 1];
        }
    }
}
